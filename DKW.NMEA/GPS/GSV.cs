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

    public class GSV : NmeaMessage
    {
        private static readonly ReadOnlyMemory<Byte> KEY = Encoding.UTF8.GetBytes("$GPGSV").AsMemory();
        protected override ReadOnlyMemory<Byte> Key => KEY;

        public Int32 TotalMessages { get; private set; }
        public Int32 MessageNumber { get; private set; }
        public Int32 SatellitesInView { get; private set; }
        public SV SV1 { get; private set; }
        public SV SV2 { get; private set; }
        public SV SV3 { get; private set; }
        public SV SV4 { get; private set; }

        public override String ToString() => $"GPGSV {TotalMessages} {MessageNumber} {SatellitesInView} {SV1} {SV2} {SV3}";

        public override NmeaMessage Parse(ReadOnlySequence<Byte> sentence)
        {
            var lexer = new Lexer(sentence);

            if (lexer.NextString() != "GPGSV")
            {
                throw lexer.Error();
            }

            var gsv = new GSV()
            {
                TotalMessages = lexer.NextInteger(),
                MessageNumber = lexer.NextInteger(),
                SatellitesInView = lexer.NextInteger(),
                SV1 = SV.Create(lexer.NextInteger(), lexer.NextInteger(), lexer.NextInteger(), lexer.NextInteger())
            };

            if (!lexer.EOL)
            {
                gsv.SV2 = SV.Create(lexer.NextInteger(), lexer.NextInteger(), lexer.NextInteger(), lexer.NextInteger());
            }

            if (!lexer.EOL)
            {
                gsv.SV3 = SV.Create(lexer.NextInteger(), lexer.NextInteger(), lexer.NextInteger(), lexer.NextInteger());
            }

            if (!lexer.EOL)
            {
                gsv.SV4 = SV.Create(lexer.NextInteger(), lexer.NextInteger(), lexer.NextInteger(), lexer.NextInteger());
            }

            gsv.Checksum = lexer.NextChecksum();
            return gsv;
        }

        public class SV
        {
            public Int32 PRN { get; private set; }
            public Int32 Elevation { get; private set; }
            public Int32 Azimuth { get; private set; }
            public Int32 SNR { get; private set; }

            public override String ToString() => $"SV {PRN} {Elevation} {Azimuth} {SNR}";

            internal static SV Create(Int32 prn, Int32 elevation, Int32 azimuth, Int32 snr)
            {
                return new SV()
                {
                    PRN = prn,
                    Elevation = elevation,
                    Azimuth = azimuth,
                    SNR = snr
                };
            }
        }
    }
}
