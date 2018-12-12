namespace DKW.NMEA.GasFinder
{
    using System;
    using System.Buffers;
    using System.Text;
    using DKW.NMEA.Parsing;

    public class GFDTA : NmeaMessage
    {
        private static readonly ReadOnlyMemory<Byte> KEY = Encoding.UTF8.GetBytes("$GFDTA").AsMemory();
        protected override ReadOnlyMemory<Byte> Key => KEY;

        public Double Concentration { get; private set; }
        public Int32 R2 { get; private set; }
        public Double Distance { get; private set; }
        public Int32 Light { get; private set; }
        public DateTime DateTime { get; private set; }
        public String SerialNumber { get; private set; }
        public String Status { get; private set; }

        public override String ToString() => $"GFDTA {Concentration} {R2} {Distance} {Light} {DateTime} {SerialNumber} {Status}";

        public override NmeaMessage Parse(ReadOnlySequence<Byte> sentence)
        {
            var lexer = new Lexer(sentence);

            if (lexer.NextString() != "GFDTA")
            {
                throw lexer.Error();
            }

            var dta = new GFDTA();

            dta.Concentration = lexer.NextDouble();
            dta.R2 = lexer.NextInteger();
            dta.Distance = lexer.NextDouble();
            dta.Light = lexer.NextInteger();
            dta.DateTime = lexer.NextDateTime();
            dta.SerialNumber = lexer.NextString();
            dta.Status = lexer.NextString();
            dta.Checksum = lexer.NextChecksum();

            return dta;
        }
    }
}
