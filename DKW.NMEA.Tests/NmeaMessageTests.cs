using DKW.NMEA;
using Shouldly;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace DKW
{
    public class NmeaMessageTests
    {
        [Fact]
        public void When_calling_CanHandle_short_lines_do_not_cause_exceptions()
        {
            var bytes = Encoding.UTF8.GetBytes("$NMEA");
            var buffer = new ReadOnlySequence<Byte>(bytes);
            var parser = new MyNmeaMessage();

            parser.CanHandle(buffer).ShouldBeFalse();
        }

        public class MyNmeaMessage : NmeaMessage
        {
            private static readonly ReadOnlyMemory<Byte> _key = Encoding.UTF8.GetBytes("$NMEAG").AsMemory();
            protected override ReadOnlyMemory<Byte> Key => _key;

            public override NmeaMessage Parse(ReadOnlySequence<Byte> sentence)
            {
                throw new NotImplementedException();
            }
        }
    }
}
