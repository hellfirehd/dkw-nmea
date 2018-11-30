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
        public void NextToken_ReturnsCorrectTokenTypes()
        {
            var bytes = Encoding.UTF8.GetBytes("$GPGGA,232608.000,5057.1975,N,11134.8332,W,2,8,1.06,781.7,M,-18.1,M,0000,0000*62\r\n");
            var buffer = new ReadOnlySequence<Byte>(bytes);
            var lexer = new Lexer(buffer);

            lexer.NextToken().TokenType.ShouldBe(TokenType.Dollar);
            lexer.NextToken().TokenType.ShouldBe(TokenType.String);
            lexer.NextToken().TokenType.ShouldBe(TokenType.Comma);
            lexer.NextToken().TokenType.ShouldBe(TokenType.Number);
            lexer.NextToken().TokenType.ShouldBe(TokenType.Comma);
            lexer.NextToken().TokenType.ShouldBe(TokenType.Number);
            lexer.NextToken().TokenType.ShouldBe(TokenType.Comma);
            lexer.NextToken().TokenType.ShouldBe(TokenType.String);
            lexer.NextToken().TokenType.ShouldBe(TokenType.Comma);
            lexer.NextToken().TokenType.ShouldBe(TokenType.Number);
            lexer.NextToken().TokenType.ShouldBe(TokenType.Comma);
            lexer.NextToken().TokenType.ShouldBe(TokenType.String);
            lexer.NextToken().TokenType.ShouldBe(TokenType.Comma);
            lexer.NextToken().TokenType.ShouldBe(TokenType.Number);
            lexer.NextToken().TokenType.ShouldBe(TokenType.Comma);
            lexer.NextToken().TokenType.ShouldBe(TokenType.Number);
            lexer.NextToken().TokenType.ShouldBe(TokenType.Comma);
            lexer.NextToken().TokenType.ShouldBe(TokenType.Number);
            lexer.NextToken().TokenType.ShouldBe(TokenType.Comma);
            lexer.NextToken().TokenType.ShouldBe(TokenType.Number);
            lexer.NextToken().TokenType.ShouldBe(TokenType.Comma);
            lexer.NextToken().TokenType.ShouldBe(TokenType.String);
            lexer.NextToken().TokenType.ShouldBe(TokenType.Comma);
            lexer.NextToken().TokenType.ShouldBe(TokenType.Number);
            lexer.NextToken().TokenType.ShouldBe(TokenType.Comma);
            lexer.NextToken().TokenType.ShouldBe(TokenType.String);
            lexer.NextToken().TokenType.ShouldBe(TokenType.Comma);
            lexer.NextToken().TokenType.ShouldBe(TokenType.Number);
            lexer.NextToken().TokenType.ShouldBe(TokenType.Comma);
            lexer.NextToken().TokenType.ShouldBe(TokenType.Number);
            lexer.NextToken().TokenType.ShouldBe(TokenType.Asterisk);
            lexer.NextToken().TokenType.ShouldBe(TokenType.Number);
        }

        [Fact]
        public void NextToken_ReturnsCorrectTokenValues()
        {
            var bytes = Encoding.UTF8.GetBytes("$GPGGA,232608.000,5057.1975,N,11134.8332,W,2,8,1.06,781.7,M,-18.1,M,0000,0000*62\r\n");
            var buffer = new ReadOnlySequence<Byte>(bytes);
            var lexer = new Lexer(buffer);

            lexer.NextToken().TokenType.ShouldBe(TokenType.Dollar);
            lexer.NextToken().String.ShouldBe("GPGGA");
            lexer.NextToken().TokenType.ShouldBe(TokenType.Comma);
            lexer.NextToken().Number.ShouldBe(232608.0);
            lexer.NextToken().TokenType.ShouldBe(TokenType.Comma);
            lexer.NextToken().Number.ShouldBe(5057.1975);
            lexer.NextToken().TokenType.ShouldBe(TokenType.Comma);
            lexer.NextToken().String.ShouldBe("N");
            lexer.NextToken().TokenType.ShouldBe(TokenType.Comma);
            lexer.NextToken().Number.ShouldBe(11134.8332);
            lexer.NextToken().TokenType.ShouldBe(TokenType.Comma);
            lexer.NextToken().String.ShouldBe("W");
            lexer.NextToken().TokenType.ShouldBe(TokenType.Comma);
            lexer.NextToken().Number.ShouldBe(2);
            lexer.NextToken().TokenType.ShouldBe(TokenType.Comma);
            lexer.NextToken().Number.ShouldBe(8);
            lexer.NextToken().TokenType.ShouldBe(TokenType.Comma);
            lexer.NextToken().Number.ShouldBe(1.06);
            lexer.NextToken().TokenType.ShouldBe(TokenType.Comma);
            lexer.NextToken().Number.ShouldBe(781.7);
            lexer.NextToken().TokenType.ShouldBe(TokenType.Comma);
            lexer.NextToken().String.ShouldBe("M");
            lexer.NextToken().TokenType.ShouldBe(TokenType.Comma);
            lexer.NextToken().Number.ShouldBe(-18.1);
            lexer.NextToken().TokenType.ShouldBe(TokenType.Comma);
            lexer.NextToken().String.ShouldBe("M");
            lexer.NextToken().TokenType.ShouldBe(TokenType.Comma);
            lexer.NextToken().Number.ShouldBe(0);
            lexer.NextToken().TokenType.ShouldBe(TokenType.Comma);
            lexer.NextToken().Number.ShouldBe(0);
            lexer.NextToken().TokenType.ShouldBe(TokenType.Asterisk);
            lexer.NextToken().Number.ShouldBe(62);
        }

        [Fact]
        public void NextValue_methods_should_return_correct_values()
        {
            var bytes = Encoding.UTF8.GetBytes("$BLARGH,232608.000,5057.1975,N,11134.8332,W,2,,781.7,M,161074*62\r\n");
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
            lexer.NextHexadecimal().ShouldBe(0x62);
        }
    }
}
