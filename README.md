# DKW NMEA

[![Build status](https://ci.appveyor.com/api/projects/status/w3xw1uk3mpvnranb/branch/master?svg=true)](https://ci.appveyor.com/project/dougkwilson/dkw-nmea/branch/master)



A very fast [NMEA](https://en.wikipedia.org/wiki/NMEA_0183) parser.  The speed is achieved by 
using `System.Buffers`, `System.IO.Pipelines`, `Span<T>` and related bits and pieces
avoiding as many allocations as possible.

(This is my first foray into Buffers and Pipelines... Be Gentle!)

## Usage

Synchronous:

```csharp
using (var nr = new NmeaReader(File.Open("track2.nmea")))
{
  while (true)
  {
    var message = nr.ReadNext();
    if (message == null)
    {
      break;
    }
    Console.WriteLine(message);
  }
}
```

Asynchronous:

```csharp
using (var nr = new NmeaReader(File.Open("track2.nmea")))
{
  while (true)
  {
    var message = await nr.ReadNextAsync().ConfigureAwait(false);
    if (message == null)
    {
      break;
    }
    Console.WriteLine(message);
  }
}
```

Bloody freaking crazy Asynchronous:
```csharp
var nsr = NmeaStreamReader.Create();
using (var reader = File.Open("track1.nmea"))
{
  await nsr.ParseStreamAsync(reader, (s) =>
  {
    Console.WriteLine(message);
  }).ConfigureAwait(false);
}
```


## Filtering

If you are only interested in a specific NMEA sentence then build the `NmeaStreamReader` yourself:

```csharp
var nsr = new NmeaStreamReader().Register(new GGA());
```

Parsers are available for:

* GPGGA
* GPGLL
* GPGSA
* GPGSV
* GPRMC

But don't despair!  It's easy to add new sentences.

```csharp
public class GLL : NmeaMessage
{
  private static readonly ReadOnlyMemory<Byte> KEY = Encoding.UTF8.GetBytes("$GPGLL").AsMemory();
  protected override ReadOnlyMemory<Byte> Key => KEY;

  public Double Latitude { get; private set; }
  public Double Longitude { get; private set; }
  public TimeSpan FixTime { get; private set; }
  public Char DataActive { get; private set; }

  public override String ToString() => $"GPGLL {Latitude} {Longitude} {FixTime} {DataActive}";

  public override NmeaMessage Parse(ReadOnlySequence<Byte> sentence)
  {
    var lexer = new Lexer(sentence);

    if (lexer.NextString() != "GPGLL")
    {
      throw lexer.Error();
    }

    return new GLL()
    {
      Latitude = lexer.NextLatitude(),
      Longitude = lexer.NextLongitude(),
      FixTime = lexer.NextTimeSpan(),
      DataActive = lexer.NextChar(),
      Checksum = lexer.NextChecksum()
    };
  }
}
```
