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

    public class Token
    {
        private readonly Int64 _index;
        private readonly Double? _number;

        public Token(Int64 index, TokenType type)
            : this(index, type, null)
        {
        }

        public Token(Int64 index, TokenType type, String s)
        {
            _index = index;
            TokenType = type;
            String = s;
        }

        public Token(Int64 index, TokenType type, Double number)
        {
            _index = index;
            TokenType = type;
            _number = number;
        }

        public TokenType TokenType { get; }
        public Double Number => _number.Value;
        public String String { get; }
        public override String ToString() => _number.HasValue ? $"Token({TokenType}, {_number}) at {_index}" : $"Token({TokenType}) at {_index}";
    }
}
