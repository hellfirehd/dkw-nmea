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

namespace DKW.NMEA.Parsing
{
    using System;
    using System.Buffers;
    using System.Buffers.Text;
    using System.Runtime.CompilerServices;
    using System.Text;

    public static class Extensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static String ToString(in this ReadOnlySequence<Byte> buffer, Encoding encoding)
        {
            if (buffer.IsSingleSegment) {
                return encoding.GetString(buffer.First.Span);
            }

            return String.Create((Int32)buffer.Length, buffer, (span, sequence) =>
            {
                foreach (var segment in sequence) {
                    encoding.GetChars(segment.Span, span);
                    span = span.Slice(segment.Length);
                }
            });
        }


        public static Boolean IsMatch<T>(in this ReadOnlySequence<T> source, ReadOnlySpan<T> value, SequencePosition? from = null) where T : IEquatable<T>
        {
            var sequence = from == null ? source : source.Slice(from.Value);
            if (sequence.Length < value.Length)
                return false;

            var slice = sequence.Slice(0, value.Length);

            var i = 0;
            foreach (var memory in slice) {
                foreach (var t in memory.Span) {
                    if (!t.Equals(value[i++]))
                        return false;
                }
            }
            return true;
        }

        public static Boolean TryParse(this ReadOnlySequence<Byte> slice, out Int32 value)
        {
            if (slice.IsSingleSegment) {

                if (Utf8Parser.TryParse(slice.First.Span, out Int32 number, out var bytesConsumed)) {
                    value = number;
                    return true;
                }
            } else {
                if (Utf8Parser.TryParse(slice.ToArray(), out Int32 number, out var bytesConsumed)) {
                    value = number;
                    return true;
                }
            }

            value = default;
            return false;
        }

        public static Boolean TryParse(this ReadOnlySequence<Byte> slice, out Double value)
        {
            if (slice.IsSingleSegment) {

                if (Utf8Parser.TryParse(slice.First.Span, out Double number, out var bytesConsumed)) {
                    value = number;
                    return true;
                }
            } else {
                if (Utf8Parser.TryParse(slice.ToArray(), out Double number, out var bytesConsumed)) {
                    value = number;
                    return true;
                }
            }

            value = default;
            return false;
        }

        public static Boolean TryParseHex(this ReadOnlySequence<Byte> slice, out Int32 value)
        {
            value = default;
            for (var i = 0; i < slice.Length; i++)
            {
                var b = slice.Slice(i);

                value += HtoI(b.First.Span[0]) << (((Int32)slice.Length - 1 - i) * 4);
            }

            return true;
        }

        private static Int32 HtoI(Byte ch)
        {
            if (ch < 48 || (ch > 57 && ch < 65) || ch > 70)
                throw new ArgumentOutOfRangeException(nameof(ch));

            return (ch < 58) ? ch - 48 : ch - 55;
        }
    }
}
