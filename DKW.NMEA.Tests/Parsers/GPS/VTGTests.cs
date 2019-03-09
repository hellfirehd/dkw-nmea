using DKW.NMEA.GPS;
using Shouldly;
using System;
using System.Buffers;
using System.Text;
using Xunit;

namespace DNW.NMEA.Tests.Parsers.GPS
{
    public class VTGTests
    {
        [Fact]
        public void Can_parse_well_formed_sentence()
        {
            var bytes = Encoding.UTF8.GetBytes("$GPVTG,054.7,T,034.4,M,005.5,N,010.2,K*48\r\n");
            var buffer = new ReadOnlySequence<Byte>(bytes);

            var vtg = new VTG().Parse(buffer) as VTG;

            vtg.ShouldNotBeNull();
            vtg.TrueTrack.ShouldBe(54.7, 0.00001d);
            vtg.MagneticTrack.ShouldBe(34.4, 0.00001d);
            vtg.GroundSpeedN.ShouldBe(5.5, 0.00001d);
            vtg.GroundSpeedK.ShouldBe(10.2, 0.00001d);
            vtg.Checksum.ShouldBe(0x48);
        }
    }
}
