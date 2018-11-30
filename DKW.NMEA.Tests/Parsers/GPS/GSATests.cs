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

    public class GSATests
    {
        [Fact]
        public void Can_parse_well_formed_sentence()
        {
            var bytes = Encoding.UTF8.GetBytes("$GPGSA,A,3,,,,,,16,18,,22,24,,,3.6,2.1,2.2*3C\r\n");
            var buffer = new ReadOnlySequence<Byte>(bytes);

            var gsa = new GSA().Parse(buffer) as GSA;

            gsa.ShouldNotBeNull();
            gsa.FixMode.ShouldBe('A');
            gsa.FixType.ShouldBe(FixType.Fix3D);
            gsa.SV.ShouldContain(16);
            gsa.SV.ShouldContain(18);
            gsa.SV.ShouldContain(22);
            gsa.SV.ShouldContain(24);
            gsa.Pdop.ShouldBe(3.6d);
            gsa.Hdop.ShouldBe(2.1d);
            gsa.Vdop.ShouldBe(2.2d);
            gsa.Checksum.ShouldBe(0x3C);
        }
    }
}
