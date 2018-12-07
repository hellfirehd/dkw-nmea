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

namespace DNW.NMEA.Tests.Parsers.GPS
{
    using System;
    using System.Buffers;
    using System.Text;
    using DKW.NMEA.GPS;
    using Shouldly;
    using Xunit;

    public class RMCTests
    {
        [Fact]
        public void Can_parse_well_formed_sentence()
        {
            var bytes = Encoding.UTF8.GetBytes("$GPRMC,063321.803,A,5234.906,N,01318.184,E,4948.6,043.5,171118,,W*47\r\n");
            var buffer = new ReadOnlySequence<Byte>(bytes);

            var rmc = new RMC().Parse(buffer) as RMC;

            rmc.ShouldNotBeNull();
            rmc.FixTime.ShouldBe(new TimeSpan(6, 33, 21).Add(TimeSpan.FromMilliseconds(803)));
            rmc.Status.ShouldBe('A');
            rmc.Latitude.ShouldBe(52.581767d, 0.00001d);
            rmc.Longitude.ShouldBe(13.30307d, 0.00001d);
            rmc.SpeedOverGround.ShouldBe(4948.6);
            rmc.TrackAngle.ShouldBe(43.5);
            rmc.Date.ShouldBe(new DateTime(2018, 11, 17));
            rmc.MagneticVariation.ShouldBe(Double.NaN);
            rmc.Direction.ShouldBe('W');
            rmc.Checksum.ShouldBe(0x47);
        }


        [Fact]
        public void UBlox_G70xx_happy_path()
        {
            var bytes = Encoding.UTF8.GetBytes("$GPRMC,182630.00,A,4955.65790,N,11926.34845,W,0.045,,051218,,,D*64\r\n");
            var buffer = new ReadOnlySequence<Byte>(bytes);

            var rmc = new RMC().Parse(buffer) as RMC;

            rmc.ShouldNotBeNull();
            rmc.FixTime.ShouldBe(new TimeSpan(18, 26, 30));
            rmc.Status.ShouldBe('A');
            rmc.Latitude.ShouldBe(49.9276317d, 0.0000001d);
            rmc.Longitude.ShouldBe(-119.4391408d, 0.0000001d);
            rmc.SpeedOverGround.ShouldBe(0.045d);
            rmc.TrackAngle.ShouldBe(Double.NaN);
            rmc.Date.ShouldBe(new DateTime(2018, 12, 5));
            rmc.MagneticVariation.ShouldBe(Double.NaN);
            rmc.Direction.ShouldBe(Char.MinValue);
            rmc.Mode.ShouldBe('D');
            rmc.Checksum.ShouldBe(0x64);
        }

        [Fact]
        public void UBlox_G70xx_incomplete_fix()
        {
            var bytes = Encoding.UTF8.GetBytes("$GPRMC,174114.00,V,,,,,,,051218,,,N*74\r\n");
            var buffer = new ReadOnlySequence<Byte>(bytes);

            var rmc = new RMC().Parse(buffer) as RMC;

            rmc.ShouldNotBeNull();
            rmc.FixTime.ShouldBe(new TimeSpan(17, 41, 14));
            rmc.Status.ShouldBe('V');
            rmc.Latitude.ShouldBe(Double.NaN);
            rmc.Longitude.ShouldBe(Double.NaN);
            rmc.SpeedOverGround.ShouldBe(Double.NaN);
            rmc.TrackAngle.ShouldBe(Double.NaN);
            rmc.Date.ShouldBe(new DateTime(2018, 12, 5));
            rmc.MagneticVariation.ShouldBe(Double.NaN);
            rmc.Direction.ShouldBe(Char.MinValue);
            rmc.Mode.ShouldBe('N');
            rmc.Checksum.ShouldBe(0x74);
        }

        [Fact]
        public void NovAtel_happy_path()
        {
            var bytes = Encoding.UTF8.GetBytes("$GPRMC,144326.00,A,5107.0017737,N,11402.3291611,W,0.080,323.3,210307,0.0,E,A*20\r\n");
            var buffer = new ReadOnlySequence<Byte>(bytes);

            var rmc = new RMC().Parse(buffer) as RMC;

            rmc.ShouldNotBeNull();
            rmc.FixTime.ShouldBe(new TimeSpan(14, 43, 26));
            rmc.Status.ShouldBe('A');
            rmc.Latitude.ShouldBe(51.1166962283333d, 0.0000001d);
            rmc.Longitude.ShouldBe(-114.038819351667d, 0.0000001d);
            rmc.SpeedOverGround.ShouldBe(0.080d);
            rmc.TrackAngle.ShouldBe(323.3);
            rmc.Date.ShouldBe(new DateTime(2007, 03, 21));
            rmc.MagneticVariation.ShouldBe(0.0);
            rmc.Direction.ShouldBe('E');
            rmc.Mode.ShouldBe('A');
            rmc.Checksum.ShouldBe(0x20);
        }
    }
}
