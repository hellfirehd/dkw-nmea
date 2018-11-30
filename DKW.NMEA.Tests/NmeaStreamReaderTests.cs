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

namespace DKW.NMEA.Tests
{
    using System;
    using System.IO;
    using DKW.NMEA;
    using Xunit;

    public class NmeaStreamReaderTests
    {
        [Fact]
        public async void NmeaStreamReader_will_throw_if_no_parsers_have_been_added()
        {
            using (var stream = new MemoryStream())
            {
                var nsr = new NmeaStreamReader();
                await Assert.ThrowsAsync<InvalidOperationException>(async () => await nsr.ParseStreamAsync(stream, (m) => { }));
            }
        }
    }
}
