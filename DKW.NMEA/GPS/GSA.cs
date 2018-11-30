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

namespace DKW.NMEA.GPS
{
    using System;
    using System.Buffers;
    using System.Text;
    using DKW.NMEA.Parsing;

    public class GSA : NmeaMessage
    {
        private static readonly ReadOnlyMemory<Byte> KEY = Encoding.UTF8.GetBytes("$GPGSA").AsMemory();
        protected override ReadOnlyMemory<Byte> Key => KEY;

        public Char FixMode { get; private set; }
        public FixType FixType { get; private set; }
        public Int32[] SV { get; private set; }
        public Double Pdop { get; private set; }
        public Double Hdop { get; private set; }
        public Double Vdop { get; private set; }

        public override String ToString() => $"GPGSA {FixMode} {FixType} {Pdop} {Hdop} {Vdop}";

        public override NmeaMessage Parse(ReadOnlySequence<Byte> sentence)
        {
            var lexer = new Lexer(sentence);

            // Sentence Identifier
            if (lexer.NextString() != "GPGSA")
            {
                throw lexer.Error();
            }

            // $GPGSA,A,3,04,05,,09,12,,,24,,,,,2.5,1.3,2.1*39
            var gsa = new GSA()
            {
                FixMode = lexer.NextChar(),
                FixType = (FixType)lexer.NextInteger(),
                SV = new Int32[12]
            };

            for (var i = 0; i < 12; i++)
            {
                gsa.SV[i] = lexer.NextInteger();
            }

            gsa.Pdop = lexer.NextDouble();
            gsa.Hdop = lexer.NextDouble();
            gsa.Vdop = lexer.NextDouble();
            gsa.Checksum = lexer.NextChecksum();

            return gsa;
        }
    }
}
