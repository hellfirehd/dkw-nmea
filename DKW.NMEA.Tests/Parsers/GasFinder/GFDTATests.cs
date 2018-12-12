namespace DKW.Parsers.GasFinder
{
    using System;
    using System.Buffers;
    using System.Text;
    using DKW.NMEA.GasFinder;
    using Shouldly;
    using Xunit;

    public class GFDTATests
    {
        [Fact]
        public void Can_parse_well_formed_sentence()
        {
            var bytes = Encoding.UTF8.GetBytes("$GFDTA, 5.0 ,98,2.0,12941,2004/10/29 18:49:55,CH4AB-1015,1*47\r\n");
            var buffer = new ReadOnlySequence<Byte>(bytes);

            var gcp = new GFDTA().Parse(buffer) as GFDTA;

            gcp.ShouldNotBeNull();
            gcp.Concentration.ShouldBe(5.0d);
            gcp.R2.ShouldBe(98);
            gcp.Distance.ShouldBe(2.0d);
            gcp.Light.ShouldBe(12941);
            gcp.SerialNumber.ShouldBe("CH4AB-1015");
            gcp.Status.ShouldBe("1");

            gcp.DateTime.ShouldBe(new DateTime(2004, 10, 29, 18, 49, 55));

            gcp.Checksum.ShouldBe(0x47);
        }
    }
}
