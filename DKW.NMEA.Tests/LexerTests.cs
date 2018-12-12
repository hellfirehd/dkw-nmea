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
    using System.Buffers;
    using System.Text;
    using DKW.NMEA.Parsing;
    using Shouldly;
    using Xunit;

    public class LexerTests
    {
        [Fact]
        public void NextValue_methods_should_return_correct_values()
        {
            var bytes = Encoding.UTF8.GetBytes("$BLARGH,232608.000,5057.1975,N,11134.8332,W,2,,781.7,M,161074,FF,2018/10/16 13:35:55*62\r\n");
            var buffer = new ReadOnlySequence<Byte>(bytes);
            var lexer = new Lexer(buffer);

            lexer.NextString().ShouldBe("BLARGH");
            lexer.NextTimeSpan().ShouldBe(new TimeSpan(23, 26, 8));
            lexer.NextLatitude().ShouldBe(50.9532916666667d, 0.00000001d);
            lexer.NextLongitude().ShouldBe(-111.580553333333d, 0.00000001d);
            lexer.NextInteger().ShouldBe(2);
            lexer.NextInteger().ShouldBe(0);
            lexer.NextDouble().ShouldBe(781.7);
            lexer.NextChar().ShouldBe('M');
            lexer.NextDate().ShouldBe(new DateTime(1974, 10, 16));
            lexer.NextHexadecimal().ShouldBe(0xff);
            lexer.NextDateTime().ShouldBe(new DateTime(2018, 10, 16, 13, 35, 55));
            lexer.NextChecksum().ShouldBe(0x62);
        }

        [Fact]
        public void Advancing_past_the_end_throws()
        {
            var bytes = Encoding.UTF8.GetBytes("$BLARGH,,*62\r\n");
            var buffer = new ReadOnlySequence<Byte>(bytes);
            var lexer = new Lexer(buffer);

            lexer.NextString().ShouldBe("BLARGH");
            lexer.NextDouble().ShouldBe(Double.NaN);
            lexer.NextChecksum().ShouldBe(0x62);
        }
    }
}
