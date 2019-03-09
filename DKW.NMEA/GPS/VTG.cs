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
    using DKW.NMEA.Parsing;
    using System;
    using System.Buffers;
    using System.Collections.Generic;
    using System.Text;

    public class VTG : NmeaMessage
    {
        private static readonly ReadOnlyMemory<Byte> KEY = Encoding.UTF8.GetBytes("$GPVTG").AsMemory();
        protected override ReadOnlyMemory<Byte> Key => KEY;

        public Double TrueTrack { get; private set; }
        public Char TrueTrackIndicator { get; set; }
        public Double MagneticTrack { get; private set; }
        public Char MagneticTrackIndicator { get; private set; }
        public Double GroundSpeedN { get; private set; }
        public Char GroundSpeedNIndicator { get; private set; }
        public Double GroundSpeedK { get; private set; }
        public Char GroundSpeedKIndicator { get; private set; }
        public Char ModeIndicator { get; private set; }

        public override String ToString() => $"GPVTG {TrueTrack} {MagneticTrack} {GroundSpeedN} {GroundSpeedK}";

        public override NmeaMessage Parse(ReadOnlySequence<Byte> sentence)
        {
            var lexer = new Lexer(sentence);

            if (lexer.NextString() != "GPVTG")
            {
                throw lexer.Error();
            }

            return new VTG()
            {
                TrueTrack = lexer.NextDouble(),
                TrueTrackIndicator=  lexer.NextChar(),
                MagneticTrack = lexer.NextDouble(),
                MagneticTrackIndicator = lexer.NextChar(),
                GroundSpeedN = lexer.NextDouble(),
                GroundSpeedNIndicator = lexer.NextChar(),
                GroundSpeedK = lexer.NextDouble(),
                GroundSpeedKIndicator = lexer.NextChar(),
                ModeIndicator = lexer.NextChar(),
                Checksum = lexer.NextChecksum()
            };
        }
    }
}
