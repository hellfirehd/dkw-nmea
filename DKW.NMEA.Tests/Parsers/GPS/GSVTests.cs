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

    public class GSVTests
    {
        [Fact]
        public void Can_parse_well_formed_sentence()
        {
            var bytes = Encoding.UTF8.GetBytes("$GPGSV,3,1,11,03,03,111,00,04,15,270,00,06,01,010,00,13,06,292,00*74\r\n");
            var buffer = new ReadOnlySequence<Byte>(bytes);

            var gsv = new GSV().Parse(buffer) as GSV;

            gsv.ShouldNotBeNull();
            gsv.TotalMessages.ShouldBe(3);
            gsv.MessageNumber.ShouldBe(1);
            gsv.SatellitesInView.ShouldBe(11);

            gsv.SV1.PRN.ShouldBe(3);
            gsv.SV1.Elevation.ShouldBe(3);
            gsv.SV1.Azimuth.ShouldBe(111);
            gsv.SV1.SNR.ShouldBe(0);

            gsv.SV2.PRN.ShouldBe(4);
            gsv.SV2.Elevation.ShouldBe(15);
            gsv.SV2.Azimuth.ShouldBe(270);
            gsv.SV2.SNR.ShouldBe(0);

            gsv.SV3.PRN.ShouldBe(6);
            gsv.SV3.Elevation.ShouldBe(1);
            gsv.SV3.Azimuth.ShouldBe(10);
            gsv.SV3.SNR.ShouldBe(0);

            gsv.SV4.PRN.ShouldBe(13);
            gsv.SV4.Elevation.ShouldBe(6);
            gsv.SV4.Azimuth.ShouldBe(292);
            gsv.SV4.SNR.ShouldBe(0);

            gsv.Checksum.ShouldBe(0x74);
        }

        [Fact]
        public void Can_handle_short_sentence()
        {
            var bytes = Encoding.UTF8.GetBytes("$GPGSV,4,4,13,31,02,340,24*4A\r\n");
            var buffer = new ReadOnlySequence<Byte>(bytes);

            var gsv = new GSV().Parse(buffer) as GSV;

            gsv.ShouldNotBeNull();
            gsv.TotalMessages.ShouldBe(4);
            gsv.MessageNumber.ShouldBe(4);
            gsv.SatellitesInView.ShouldBe(13);

            gsv.SV1.PRN.ShouldBe(31);
            gsv.SV1.Elevation.ShouldBe(2);
            gsv.SV1.Azimuth.ShouldBe(340);
            gsv.SV1.SNR.ShouldBe(24);

            gsv.SV2.ShouldBeNull();
            gsv.SV3.ShouldBeNull();
            gsv.SV4.ShouldBeNull();

            gsv.Checksum.ShouldBe(0x4A);
        }
    }
}
