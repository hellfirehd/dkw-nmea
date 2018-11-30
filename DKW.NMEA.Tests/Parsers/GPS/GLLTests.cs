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

    public class GLLTests
    {
        [Fact]
        public void Can_parse_well_formed_sentence()
        {
            var bytes = Encoding.UTF8.GetBytes("$GPGLL,4916.45,N,12311.12,W,225444,A,*1D\r\n");
            var buffer = new ReadOnlySequence<Byte>(bytes);

            var gll = new GLL().Parse(buffer) as GLL;

            gll.ShouldNotBeNull();
            gll.Latitude.ShouldBe(49.27417d, 0.00001d);
            gll.Longitude.ShouldBe(-123.18533d, 0.00001d);
            gll.FixTime.ShouldBe(new TimeSpan(22, 54, 44));
            gll.DataActive.ShouldBe('A');
            gll.Checksum.ShouldBe(0x1D);
        }
    }
}
