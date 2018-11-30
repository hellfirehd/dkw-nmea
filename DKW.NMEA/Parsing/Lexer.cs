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

        private ReadOnlySequence<Byte> _sequence;
        private Int64 _index;
        private Byte _currentByte;
#if DEBUG
        private Char _currentChar;
#endif

        public Lexer(ReadOnlySequence<Byte> sequence)
        {
            _sequence = sequence;
            _index = 0L;
            _currentByte = CharAt(_index);
        }

        internal Exception Error() => new Exception($"Did not expect '{(Char)_currentByte}' at position {_index + 1}.");

        private void Advance()
        {
            _index++;
            _currentByte = CharAt(_index);
#if DEBUG
            _currentChar = (Char)Current();
#endif
        }

        public Byte Current()
        {
            return CharAt(_index);
        }

        private Byte Peek()
        {
            var peekPos = _index + 1;
            return CharAt(peekPos);
        }

        private Byte CharAt(Int64 index)
        {
            if (index > _sequence.Length - 1) return 0;

            var slice = _sequence.Slice(index);
            var candidate = slice.Slice(0, 1);

            return candidate.First.Span[0];
        }

        public Token NextToken()
        {
            SkipWhiteSpace();

            while (_currentByte != 0)
            {

                if (IsNumber(_currentByte))
                {
                    return new Token(_index, TokenType.Number, Number());
                }

                if (IsLetter(_currentByte))
                {
                    return new Token(_index, TokenType.String, String());
                }

                if (_currentByte == '$')
                {
                    Advance();
                    return new Token(_index, TokenType.Dollar);
                }

                if (_currentByte == ',')
                {
                    Advance();
                    return new Token(_index, TokenType.Comma);
                }

                if (_currentByte == '*')
                {
                    Advance();
                    return new Token(_index, TokenType.Asterisk);
                }

                Error();
            }

            return new Token(_index, TokenType.EOF);
        }

        public Char NextChar()
        {
            SkipWhiteSpaceAndSeparator();

            return Char();
        }

        public String NextString()
        {
            SkipWhiteSpaceAndSeparator();
            return String();
        }

        public Double NextDouble()
        {
            SkipWhiteSpaceAndSeparator();
            return Number();
        }

        public Int32 NextInteger()
        {
            SkipWhiteSpaceAndSeparator();
            return Integer();
        }

        public Int32 NextHexadecimal()
        {
            SkipWhiteSpaceAndSeparator();
            return Hexadecimal();
        }

        public TimeSpan NextTimeSpan()
        {
            SkipWhiteSpaceAndSeparator();
            return TimeSpan();
        }

        public Double NextLatitude()
        {
            SkipWhiteSpaceAndSeparator();
            return Latitude();
        }

        public Double NextLongitude()
        {
            SkipWhiteSpaceAndSeparator();
            return Longitude();
        }

        public DateTime NextDate()
        {
            SkipWhiteSpaceAndSeparator();
            return Date();
        }

        public Int32 NextChecksum()
        {
            SkipToChecksum();
            return Hexadecimal();
        }

        private Boolean IsWhiteSpace(Byte b) => b == ' ' || b == '\t';
        private Boolean IsSeparator(Byte b) => b == ',' || b == '*' || b == '$';
        private Boolean IsDigit(Byte b) => (b >= '0' && b <= '9');
        private Boolean IsNumber(Byte b) => (b >= '0' && b <= '9') || b == '-' || b == '.';
        private Boolean IsLetter(Byte b) => (b >= 'A' && b <= 'Z') || (b >= 'a' && b <= 'z');

        private void SkipWhiteSpace()
        {
            while (_currentByte != 0 && IsWhiteSpace(_currentByte))
            {
                Advance();
            }
        }

        private void SkipWhiteSpaceAndSeparator()
        {
            while (_currentByte != 0 && IsWhiteSpace(_currentByte))
            {
                Advance();
            }

            if (IsSeparator(_currentByte))
                Advance();
        }

        private void SkipToChecksum()
        {
            while (_currentByte != 0 && !(_currentByte == '*'))
            {
                Advance();
            }
            Advance();
        }

        private Char Char()
        {
            var c = (Char)_currentByte;
            Advance();
            return c;
        }

        private String String()
        {
            var start = _index;
            while (_currentByte > 0 && IsLetter(_currentByte))
            {
                Advance();
            }

            var slice = _sequence.Slice(_sequence.GetPosition(start), _index - start);

            return slice.ToString(Encoding.UTF8);
        }

        private Int32 Integer()
        {
            var start = _index;
            while (_currentByte > 0 && IsNumber(_currentByte))
            {
                Advance();
            }

            var slice = _sequence.Slice(_sequence.GetPosition(start), _index - start);
            if (slice.Length == 0)
            {
                return 0;
            }

            if (slice.TryParse(out Int32 value))
            {
                return value;
            }

            throw Error();
        }

        private Double Number()
        {
            var start = _index;
            while (_currentByte > 0 && IsNumber(_currentByte))
            {
                Advance();
            }

            var slice = _sequence.Slice(_sequence.GetPosition(start), _index - start);
            if (slice.Length == 0)
            {
                return Double.NaN;
            }

            if (slice.TryParse(out Double value))
            {
                return value;
            }

            throw Error();
        }

        private Int32 Hexadecimal()
        {
            var start = _index;
            while (_currentByte > 0 && (IsDigit(_currentByte) || IsLetter(_currentByte)))
            {
                Advance();
            }

            var slice = _sequence.Slice(_sequence.GetPosition(start), _index - start);
            if (slice.Length == 0)
            {
                return 0;
            }

            if (slice.TryParseHex(out var value))
            {
                return value;
            }

            throw Error();
        }

        private Double Latitude()
        {
            var start = _index;
            while (_currentByte > 0 && IsNumber(_currentByte))
            {
                Advance();
            }

            var slice = _sequence.Slice(_sequence.GetPosition(start), _index - start);
            if (slice.Length < 3)
            {
                return Double.NaN;
            }

            if (slice.Slice(0, 2).TryParse(out Int32 i))
            {
                if (slice.Slice(2).TryParse(out Double d))
                {
                    Advance(); // Eat the separator
                    if (_currentByte == 'N')
                    {
                        Advance(); // Eat the N
                        return i + d / 60;
                    }
                    else if (_currentByte == 'S')
                    {
                        Advance(); // Eat the S
                        return (i + d / 60) * -1;
                    }
                }
            }

            throw Error();
        }

        private Double Longitude()
        {
            var start = _index;
            while (_currentByte > 0 && IsNumber(_currentByte))
            {
                Advance();
            }

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
                    if (_currentByte == 'E')
                    {
                        Advance(); // Eat the E
                        return i + d / 60;
                    }
                    else if (_currentByte == 'W')
                    {
                        Advance(); // Eat the W
                        return (i + d / 60) * -1;
                    }
                }
            }

            throw Error();
        }

        private TimeSpan TimeSpan()
        {
            var start = _index;
            while (_currentByte > 0 && IsNumber(_currentByte))
            {
                Advance();
            }

            var slice = _sequence.Slice(_sequence.GetPosition(start), _index - start);

            if (slice.Length < 6)
            {
                return System.TimeSpan.Zero;
            }

            if (slice.Slice(0, 2).TryParse(out Int32 hours))
            {
                if (slice.Slice(2, 2).TryParse(out Int32 mintues))
                {
                    if (slice.Slice(4).TryParse(out Double seconds))
                    {
                        return new TimeSpan(hours, mintues, 0).Add(System.TimeSpan.FromSeconds(seconds));
                    }
                }
            }

            throw Error();
        }

        private DateTime Date()
        {
            var start = _index;
            while (_currentByte > 0 && IsNumber(_currentByte))
            {
                Advance();
            }

            var slice = _sequence.Slice(_sequence.GetPosition(start), _index - start);

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
                        return new DateTime(CultureInfo.CurrentCulture.Calendar.ToFourDigitYear(twoDigitYear), months, days);
                    }
                    else if (slice.Length == 8 && slice.Slice(4, 4).TryParse(out Int32 fourDigitYear))
                    {
                        return new DateTime(fourDigitYear, months, days);
                    }
                }
            }

            throw Error();
        }
    }
}
