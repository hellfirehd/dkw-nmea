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

    public class RMC : NmeaMessage
    {
        private static readonly ReadOnlyMemory<Byte> KEY = Encoding.UTF8.GetBytes("$GPRMC").AsMemory();
        protected override ReadOnlyMemory<Byte> Key => KEY;

        public TimeSpan FixTime { get; private set; }
        public Char Status { get; private set; }
        public Double Latitude { get; private set; }
        public Double Longitude { get; private set; }
        public Double SpeedOverGround { get; private set; }
        public Double TrackAngle { get; private set; }
        public DateTime Date { get; private set; }
        public Double MagneticVariation { get; private set; }
        public Char Direction { get; private set; }

        public override String ToString() => $"GPRMC {FixTime} {Status} {Latitude} {Longitude} {SpeedOverGround} {TrackAngle} {Date} {MagneticVariation} {Direction}";

        public override NmeaMessage Parse(ReadOnlySequence<Byte> sentence)
        {
            var lexer = new Lexer(sentence);

            if (lexer.NextString() != "GPRMC")
            {
                throw lexer.Error();
            }

            return new RMC()
            {
                FixTime = lexer.NextTimeSpan(),
                Status = lexer.NextChar(),
                Latitude = lexer.NextLatitude(),
                Longitude = lexer.NextLongitude(),
                SpeedOverGround = lexer.NextDouble(),
                TrackAngle = lexer.NextDouble(),
                Date = lexer.NextDate(),
                MagneticVariation = lexer.NextDouble(),
                Direction = lexer.NextChar(),
                Checksum = lexer.NextChecksum()
            };
        }
    }
}
