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
    }
}
