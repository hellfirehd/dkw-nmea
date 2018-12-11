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
    using System.Globalization;
    using System.Text;

    public partial class Lexer
    {
        private static readonly ReadOnlySequence<Byte> Empty = ReadOnlySequence<Byte>.Empty;
        private const Byte Asterisk = (Byte)'*';

        private ReadOnlySequence<Byte> _sequence;
        private Int64 _index;

        // Don't use Auto Property for these... don't want the Property overhead.
        private Byte _currentByte;
        private Boolean _eol = false;

#if DEBUG
        private Char _currentChar;
        private String _sentence;
        private String _sentencePointer;
#endif

        public Lexer(ReadOnlySequence<Byte> sequence)
        {
            _sequence = sequence;
#if DEBUG
            _sentence = sequence.ToString(Encoding.UTF8);
#endif
            Start();
        }

        public Exception Error() => new Exception($"Did not expect '{(Char)_currentByte}' at position {_index + 1}.");
        internal Exception ZeroLength() => new Exception($"Token with Zero length at position {_index + 1}.");

        private void Advance()
        {
            _index++;
            _currentByte = ByteAt(_index);
            if (_currentByte == Asterisk) { _eol = true; }
#if DEBUG
            _currentChar = (Char)_currentByte;
            _sentencePointer = new String(' ', (Int32)_index) + "^";
#endif
        }

        private void Advance(Func<Boolean> predicate)
        {
            if (predicate == null)
                throw new ArgumentNullException(nameof(predicate));

            while (predicate())
            {
                Advance();
            }
        }

        private Byte ByteAt(Int64 index)
        {
            if (index > _sequence.Length - 1) return 0;

            var slice = _sequence.Slice(index);
            var candidate = slice.Slice(0, 1);

            return candidate.First.Span[0];
        }

        public Boolean EOL => _eol;

        public Byte Current => _currentByte;

        public Byte Peek()
        {
            var peekPos = _index + 1;
            return ByteAt(peekPos);
        }

        public void Start()
        {
            _index = 0L;
            _currentByte = ByteAt(_index);
            ConsumeWhiteSpaceAndSeparator();
        }

        public Char NextChar()
        {
            ConsumeWhiteSpace();
            if (IsSeparator(_currentByte))
            {
                // Consume the separator
                Advance();

                return Char.MinValue;
            }

            var c = (Char)_currentByte;
            // Consume Character
            Advance();

            // Consume the separator
            Advance();
            return c;
        }

        public String NextString()
        {
            ConsumeWhiteSpace();

            if (IsSeparator(_currentByte))
            {
                // Consume the separator
                Advance();
                return String.Empty;
            }

            var start = _index;
            Advance(() => _currentByte > 0 && IsLetter(_currentByte));

            var slice = _sequence.Slice(_sequence.GetPosition(start), _index - start);
            if (slice.Length == 0)
            {
                // This should never happen
                throw ZeroLength();
            }

            // Consume the separator
            Advance();
            return slice.ToString(Encoding.UTF8);
        }

        public Double NextDouble()
        {
            ConsumeWhiteSpace();

            if (IsSeparator(_currentByte))
            {
                // Consume the separator
                Advance();
                return Double.NaN;
            }

            var start = _index;
            Advance(() => _currentByte > 0 && IsNumber(_currentByte));

            var slice = _sequence.Slice(_sequence.GetPosition(start), _index - start);
            if (slice.Length == 0)
            {
                // This should never happen
                throw ZeroLength();
            }

            if (slice.TryParse(out Double value))
            {
                // Consume the separator
                Advance();
                return value;
            }

            throw Error();
        }

        public Int32 NextInteger()
        {
            ConsumeWhiteSpace();
            if (IsSeparator(_currentByte))
            {
                // Consume the separator
                Advance();
                return 0;
            }

            var start = _index;
            Advance(() => _currentByte > 0 && IsNumber(_currentByte));

            var slice = _sequence.Slice(_sequence.GetPosition(start), _index - start);
            if (slice.Length == 0)
            {
                // This should never happen
                throw ZeroLength();
            }

            if (slice.TryParse(out Int32 value))
            {
                // Consume the separator
                Advance();
                return value;
            }

            throw Error();
        }

        public Int32 NextHexadecimal()
        {
            ConsumeWhiteSpace();
            if (IsSeparator(_currentByte))
            {
                // Consume the separator
                Advance();
                return 0;
            }

            var start = _index;
            Advance(() => _currentByte > 0 && (IsDigit(_currentByte) || IsLetter(_currentByte)));

            var slice = _sequence.Slice(_sequence.GetPosition(start), _index - start);
            if (slice.Length == 0)
            {
                // This should never happen
                throw ZeroLength();
            }

            if (slice.TryParseHex(out var value))
            {
                // Consume the separator
                Advance();
                return value;
            }

            throw Error();
        }

        public TimeSpan NextTimeSpan()
        {
            ConsumeWhiteSpace();
            if (IsSeparator(_currentByte))
            {
                // Consume the separator
                Advance();
                return TimeSpan.Zero;
            }

            var start = _index;
            while (_currentByte > 0 && IsNumber(_currentByte))
            {
                Advance();
            }

            var slice = _sequence.Slice(_sequence.GetPosition(start), _index - start);
            if (slice.Length == 4)
            {
                return TimeSpan.Zero;
            }

            if (slice.Length < 6)
            {
                return TimeSpan.Zero;
            }

            if (slice.Slice(0, 2).TryParse(out Int32 hours))
            {
                if (slice.Slice(2, 2).TryParse(out Int32 mintues))
                {
                    if (slice.Slice(4).TryParse(out Double seconds))
                    {
                        // Consume the separator
                        Advance();
                        return new TimeSpan(hours, mintues, 0).Add(TimeSpan.FromSeconds(seconds));
                    }
                }
            }

            throw Error();
        }

        public Double NextLatitude()
        {
            ConsumeWhiteSpace();
            if (IsSeparator(_currentByte))
            {
                // Consume the separator
                Advance();
                // Next up should be a Direction
                if (IsLetter(Peek()))
                {
                    // Consume
                    Advance();
                }
                // And then the next separator...
                if (IsSeparator(_currentByte))
                {
                    Advance();
                }
                else
                {
                    throw Error();
                }

                return Double.NaN;
            }

            var start = _index;
            Advance(() => _currentByte > 0 && IsNumber(_currentByte));

            var slice = _sequence.Slice(_sequence.GetPosition(start), _index - start);
            if (slice.Length < 3)
            {
                return Double.NaN;
            }

            if (slice.Slice(0, 2).TryParse(out Int32 i))
            {
                if (slice.Slice(2).TryParse(out Double d))
                {
                    Advance();
                    var c = NextChar();  // Also consumes next separator
                    if (c == 'N')
                    {
                        return i + d / 60;
                    }
                    else if (c == 'S')
                    {
                        return (i + d / 60) * -1;
                    }
                }
            }

            throw Error();
        }

        public Double NextLongitude()
        {
            ConsumeWhiteSpace();
            if (IsSeparator(_currentByte))
            {
                // Consume the separator
                Advance();
                // Next up should be a Direction
                if (IsLetter(Peek()))
                {
                    // Consume
                    Advance();
                }
                // And then the next separator...
                if (IsSeparator(_currentByte))
                {
                    Advance();
                }
                else
                {
                    throw Error();
                }

                return Double.NaN;
            }

            var start = _index;
            Advance(() => _currentByte > 0 && IsNumber(_currentByte));

            var slice = _sequence.Slice(_sequence.GetPosition(start), _index - start);
            if (slice.Length < 4)
            {
                return Double.NaN;
            }

            if (slice.Slice(0, 3).TryParse(out Int32 i))
            {
                if (slice.Slice(3).TryParse(out Double d))
                {
                    Advance(); // Eat the separator
                    var c = NextChar();  // Also consumes next separator
                    if (c == 'E')
                    {
                        return i + d / 60;
                    }
                    else if (c == 'W')
                    {
                        return (i + d / 60) * -1;
                    }
                }
            }

            throw Error();
        }

        public DateTime NextDate()
        {
            ConsumeWhiteSpace();
            if (IsSeparator(_currentByte))
            {
                // Consume the separator
                Advance();
                return DateTime.MinValue;
            }

            var start = _index;
            Advance(() => _currentByte > 0 && IsNumber(_currentByte));

            var slice = _sequence.Slice(_sequence.GetPosition(start), _index - start);
            if (slice.Length == 0)
            {
                // This should never happen
                throw ZeroLength();
            }

            if (slice.Length < 6)
            {
                return DateTime.MinValue;
            }

            if (slice.Slice(0, 2).TryParse(out Int32 days))
            {
                if (slice.Slice(2, 2).TryParse(out Int32 months))
                {
                    if (slice.Length == 6 && slice.Slice(4, 2).TryParse(out Int32 twoDigitYear))
                    {
                        // Consume the separator
                        Advance();
                        return new DateTime(CultureInfo.CurrentCulture.Calendar.ToFourDigitYear(twoDigitYear), months, days);
                    }
                    else if (slice.Length == 8 && slice.Slice(4, 4).TryParse(out Int32 fourDigitYear))
                    {
                        // Consume the separator
                        Advance();
                        return new DateTime(fourDigitYear, months, days);
                    }
                }
            }

            throw Error();
        }

        public Int32 NextChecksum()
        {
            ConsumeToChecksum();
            return NextHexadecimal();
        }

        private Boolean IsWhiteSpace(Byte b) => b == ' ' || b == '\t';
        private Boolean IsSeparator(Byte b) => b == ',' || b == '*' || b == '$';
        private Boolean IsDigit(Byte b) => (b >= '0' && b <= '9');
        private Boolean IsNumber(Byte b) => (b >= '0' && b <= '9') || b == '-' || b == '.';
        private Boolean IsLetter(Byte b) => (b >= 'A' && b <= 'Z') || (b >= 'a' && b <= 'z');

        private void ConsumeWhiteSpace() => Advance(() => _currentByte != 0 && IsWhiteSpace(_currentByte));

        private void ConsumeWhiteSpaceAndSeparator()
        {
            ConsumeWhiteSpace();

            if (IsSeparator(_currentByte))
            {
                // Consume the separator
                Advance();
            }
        }

        private void ConsumeToChecksum()
        {
            Start();
            Advance(() => _currentByte != 0 && _currentByte != '*');
            // Consume the separator
            Advance();
        }
    }
}
