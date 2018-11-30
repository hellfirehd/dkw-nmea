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

    public class GGA : NmeaMessage
    {
        private static readonly ReadOnlyMemory<Byte> KEY = Encoding.UTF8.GetBytes("$GPGGA").AsMemory();
        protected override ReadOnlyMemory<Byte> Key => KEY;

        public override NmeaMessage Parse(ReadOnlySequence<Byte> sentence)
        {
            var lexer = new Lexer(sentence);

            // Sentence Identifier
            if (lexer.NextString() != "GPGGA")
            {
                throw lexer.Error();
            }

            // $GPGGA,232608.000,5057.1975,N,11134.8332,W,2,8,1.06,781.7,M,-18.1,M,0000,0000*62
            return new GGA()
            {
                FixTime = lexer.NextTimeSpan(),
                Latitude = lexer.NextLatitude(),
                Longitude = lexer.NextLongitude(),
                Quality = (FixQuality)lexer.NextInteger(),
                NumberOfSatellites = lexer.NextInteger(),
                Hdop = lexer.NextDouble(),
                Altitude = lexer.NextDouble(),
                AltitudeUnits = lexer.NextChar(),
                HeightOfGeoid = lexer.NextDouble(),
                HeightOfGeoidUnits = lexer.NextChar(),
                TimeSinceLastDgpsUpdate = lexer.NextTimeSpan(),
                DgpsStationId = lexer.NextInteger(),
                Checksum = lexer.NextChecksum()
            };
        }

        public override String ToString() => $"GPGGA {FixTime} {Latitude} {Longitude} {Quality} {NumberOfSatellites}";

        /// <summary>
        /// Fix taken at
        /// </summary>
        public TimeSpan FixTime { get; private set; }

        /// <summary>
        /// Latitude
        /// </summary>
        public Double Latitude { get; private set; }

        /// <summary>
        /// Longitude
        /// </summary>
        public Double Longitude { get; private set; }

        /// <summary>
        /// Fix Quality
        /// </summary>
        public FixQuality Quality { get; private set; }

        /// <summary>
        /// Number of satellites being tracked
        /// </summary>
        public Int32 NumberOfSatellites { get; private set; }

        /// <summary>
        /// Horizontal Dilution of Precision
        /// </summary>
        public Double Hdop { get; private set; }

        /// <summary>
        /// Altitude
        /// </summary>
        public Double Altitude { get; private set; }

        /// <summary>
        /// Altitude units ('M' for Meters)
        /// </summary>
        public Char AltitudeUnits { get; private set; }

        /// <summary>
        /// Height of geoid (mean sea level) above WGS84
        /// </summary>
        public Double HeightOfGeoid { get; private set; }

        /// <summary>
        /// Altitude units ('M' for Meters)
        /// </summary>
        public Char HeightOfGeoidUnits { get; private set; }

        /// <summary>
        /// Time since last DGPS update
        /// </summary>
        public TimeSpan TimeSinceLastDgpsUpdate { get; private set; }

        /// <summary>
        /// DGPS Station ID Number
        /// </summary>
        public Int32 DgpsStationId { get; private set; }
    }
}