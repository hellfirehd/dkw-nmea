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

    public class GLL : NmeaMessage
    {
        private static readonly ReadOnlyMemory<Byte> KEY = Encoding.UTF8.GetBytes("$GPGLL").AsMemory();
        protected override ReadOnlyMemory<Byte> Key => KEY;

        public Double Latitude { get; private set; }
        public Double Longitude { get; private set; }
        public TimeSpan FixTime { get; private set; }
        public Char DataActive { get; private set; }

        public override String ToString() => $"GPGLL {Latitude} {Longitude} {FixTime} {DataActive}";

        public override NmeaMessage Parse(ReadOnlySequence<Byte> sentence)
        {
            var lexer = new Lexer(sentence);

            if (lexer.NextString() != "GPGLL")
            {
                throw lexer.Error();
            }

            return new GLL()
            {
                Latitude = lexer.NextLatitude(),
                Longitude = lexer.NextLongitude(),
                FixTime = lexer.NextTimeSpan(),
                DataActive = lexer.NextChar(),
                Checksum = lexer.NextChecksum()
            };
        }
    }
}
