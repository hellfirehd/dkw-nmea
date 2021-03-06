﻿/*
DKW.NMEA
Copyright (C) 2018 Doug Wilson

This program is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

This program is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with this program.  If not, see <https://www.gnu.org/licenses/>.
*/

namespace DKW.NMEA
{
    using System;
    using System.Buffers;
    using System.Collections.Generic;
    using System.IO;
    using System.IO.Pipelines;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Logging.Abstractions;

    public class NmeaStreamReader
    {
        public static Byte ByteLF = (Byte)'\n';
        private readonly ILogger<NmeaStreamReader> _logger;
        private readonly List<NmeaMessage> _parsers = new List<NmeaMessage>();
        private Int32 _unparsedSequenceLength;
        private ExitReason _exitReason;

        public Int32 LineCount { get; private set; }
        public Int64 BytesReceived { get; private set; }
        public Int64 BytesRead { get; private set; }

        public Int32 AbortAfterUnparsedLines { get; set; } = 0;

        public NmeaStreamReader(ILogger<NmeaStreamReader> logger = null)
        {
            _logger = logger ?? new NullLogger<NmeaStreamReader>();
        }

        public NmeaStreamReader Register(params NmeaMessage[] parsers)
        {
            if (parsers == null)
            {
                throw new ArgumentNullException(nameof(parsers));
            }

            if (parsers.Length == 0)
            {
                throw new ArgumentException("At least one parser must be provided.", nameof(parsers));
            }

            _parsers.AddRange(parsers);

            return this;
        }

        public async Task<ExitReason> ParseStreamAsync(Stream stream, Action<NmeaMessage> callback, CancellationToken cancellationToken = default)
        {
            if (_parsers.Count == 0)
            {
                throw new InvalidOperationException("At least one parser must be registered in this instance before parsing may begin.");
            }

            var pipe = new Pipe();
            var writing = FillPipeAsync(stream, pipe.Writer, cancellationToken);
            var reading = ReadPipeAsync(pipe.Reader, callback, cancellationToken);

            await Task.WhenAll(reading, writing).ConfigureAwait(false);

            if (cancellationToken.IsCancellationRequested)
            {
                return ExitReason.CancellationRequested;
            }

            return _exitReason;
        }

        private async Task FillPipeAsync(Stream stream, PipeWriter writer, CancellationToken cancellationToken = default)
        {
            const Int32 minimumBufferSize = 512;
            while (true)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    _exitReason = ExitReason.CancellationRequested;
                    break;
                }

                try
                {
                    // Allocate at least 512 bytes from the PipeWriter. NOTE: NMEA sentences must be less than 80 bytes.
                    var memory = writer.GetMemory(minimumBufferSize);

                    var bytesRead = await stream.ReadAsync(memory, cancellationToken).ConfigureAwait(false);
                    BytesReceived += bytesRead;
                    if (bytesRead == 0)
                    {
                        _logger.LogTrace("FillPipeAsync: Stream Ended");
                        _exitReason = ExitReason.StreamEnded;
                        break;
                    }

                    // Tell the PipeWriter how much was read from the Stream
                    writer.Advance(bytesRead);

                    // Make the data available to the PipeReader
                    var result = await writer.FlushAsync(cancellationToken).ConfigureAwait(false);
                    if (result.IsCompleted)
                    {
                        _logger.LogTrace("FillPipeAsync: Reader is Completed");
                        break;
                    }
                }
                catch (OperationCanceledException)
                {
                    _exitReason = ExitReason.CancellationRequested;
                    break;
                }
            }

            // Tell the PipeReader that there's no more data coming
            writer.Complete();
        }

        private async Task ReadPipeAsync(PipeReader reader, Action<NmeaMessage> callback, CancellationToken cancellationToken = default)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    var result = await reader.ReadAsync(cancellationToken).ConfigureAwait(false);

                    var buffer = result.Buffer;
                    SequencePosition? position = null;

                    do
                    {
                        // Look for a EOL in the buffer
                        position = buffer.PositionOf(ByteLF);

                        if (position != null)
                        {
                            // Process the line
                            if (ParseSentence(buffer.Slice(0, position.Value), out var message))
                            {
                                callback(message);
                            }

                            // Skip the line + the \n character (basically position)
                            buffer = buffer.Slice(buffer.GetPosition(1, position.Value));
                        }
                    }
                    while (position != null && !cancellationToken.IsCancellationRequested);

                    // Tell the PipeReader how much of the buffer we have consumed
                    reader.AdvanceTo(buffer.Start, buffer.End);

                    // Stop reading if there's no more data coming
                    if (result.IsCompleted)
                    {
                        _logger.LogTrace("ReadPipeAsync: Writer is Completed");
                        break;
                    }
                }
                catch (OperationCanceledException)
                {
                    break;
                }

                if (AbortAfterUnparsedLines != 0 && _unparsedSequenceLength >= AbortAfterUnparsedLines)
                {
                    _logger.LogTrace("ReadPipeAsync: Sequntial Unparsed Lines: {0}  Limit: {1}", _unparsedSequenceLength, AbortAfterUnparsedLines);
                    _exitReason = ExitReason.TooManySequentialUnparsedLines;
                    break;
                }
            }

            // Mark the PipeReader as complete
            reader.Complete();
        }

        private Boolean ParseSentence(ReadOnlySequence<Byte> payload, out NmeaMessage message)
        {
            message = default;

            LineCount++;
            BytesRead += payload.Length;

            foreach (var parser in _parsers)
            {
                if (CanHandle(payload, parser))
                {
                    if (_logger.IsEnabled(LogLevel.Trace))
                    {
                        _logger.LogTrace($"Parsing Line {LineCount} with {parser.GetType().Name}: {payload.ToString(Encoding.UTF8)}");
                    }
                    try
                    {
                        message = parser.Parse(payload);
                        _unparsedSequenceLength = 0;
                        return true;
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, $"Line {LineCount} caused {parser.GetType().Name}.Parse({payload.ToString(Encoding.UTF8)}) to generate the following exception:");
                    }

                    break;
                }
            }

            if (_logger.IsEnabled(LogLevel.Trace))
            {
                _logger.LogTrace($"No parser found for Line {LineCount}: {payload.ToString(Encoding.UTF8)}");
            }

            _unparsedSequenceLength++;
            return false;
        }

        private Boolean CanHandle(ReadOnlySequence<Byte> payload, NmeaMessage parser)
        {
            try
            {
                if (parser.CanHandle(payload))
                {
                    return true;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"{parser.GetType().Name}.CanHandle({payload.ToString(Encoding.UTF8)}) generated the following exception:");
            }

            return false;
        }
    }
}
