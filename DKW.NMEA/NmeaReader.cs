/*
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

using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace DKW.NMEA
{
    public class NmeaReader : IDisposable
    {
        private readonly BufferBlock<NmeaMessage> _queue = new BufferBlock<NmeaMessage>();
        private readonly CancellationTokenSource _cts = new CancellationTokenSource();
        private readonly NmeaStreamReader _nmeaStreamReader = NmeaStreamReader.Create();
        private readonly Object _syncroot = new Object();
        private readonly Stream _stream;

        private Task _task;

        public NmeaReader(Stream stream)
        {
            _stream = stream ?? throw new ArgumentNullException(nameof(stream));
        }

        private void InitializeStreamReader(CancellationToken cancellationToken)
        {
            if (_task == null)
            {
                lock (_syncroot)
                {
                    if (_task == null)
                    {
                        _task = Task.Run(async () =>
                        {
                            await _nmeaStreamReader.ParseStreamAsync(_stream, (m) => _queue.Post(m), cancellationToken);
                        }).ContinueWith(t => {
                            _queue.Complete();
                            if (t.IsFaulted) {
                                throw t.Exception;
                            }
                        });
                    }
                }
            }
        }

        public NmeaMessage ReadNext(CancellationToken cancellationToken = default) => ReadNextAsync(cancellationToken).GetAwaiter().GetResult();

        public async ValueTask<NmeaMessage> ReadNextAsync(CancellationToken cancellationToken = default)
        {
            var ct = cancellationToken == default ? _cts.Token : cancellationToken;
            InitializeStreamReader(ct);

            try
            {
                if (await _queue.OutputAvailableAsync(ct))
                {
                    return await _queue.ReceiveAsync(ct);
                }
            }
            catch (TaskCanceledException)
            {
            }

            return null;
        }

        public void Dispose()
        {
            _cts.Cancel();
            _task.Dispose();
            _task = null;
        }
    }
}
