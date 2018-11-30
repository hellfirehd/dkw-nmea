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

    public class GGATests
    {
        [Fact]
        public void Can_parse_well_formed_sentence()
        {
            var bytes = Encoding.UTF8.GetBytes("$GPGGA,232608.000,5057.1975,N,11134.8332,W,2,8,1.06,781.7,M,-18.1,M,0000,0000*62\r\n");
            var buffer = new ReadOnlySequence<Byte>(bytes);

            var gga = new GGA().Parse(buffer) as GGA;

            gga.ShouldNotBeNull();
            gga.FixTime.ShouldBe(new TimeSpan(23, 26, 8));
            gga.Latitude.ShouldBe(50.9532916666667d, 0.00000001d);
            gga.Longitude.ShouldBe(-111.580553333333d, 0.00000001d);
            gga.Quality.ShouldBe(FixQuality.DgpsFix);
            gga.NumberOfSatellites.ShouldBe(8);
            gga.Hdop.ShouldBe(1.06);
            gga.Altitude.ShouldBe(781.7);
            gga.AltitudeUnits.ShouldBe('M');
            gga.HeightOfGeoid.ShouldBe(-18.1);
            gga.HeightOfGeoidUnits.ShouldBe('M');
            gga.TimeSinceLastDgpsUpdate.ShouldBe(TimeSpan.Zero);
            gga.DgpsStationId.ShouldBe(0);
            gga.Checksum.ShouldBe(0x62);
        }
    }
}
