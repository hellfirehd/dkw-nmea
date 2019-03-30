using DKW.NMEA;
using DKW.NMEA.GasFinder;
using DKW.NMEA.GPS;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RJCP.IO.Ports;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace GpsDemo
{
    public sealed class GpsDemoService : BackgroundService
    {
        private readonly ILogger<GpsDemoService> _logger;
        private readonly NmeaStreamReader _nsr;

        public GpsDemoService(ILogger<GpsDemoService> logger, NmeaStreamReader nmeaStreamReader)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _nsr = nmeaStreamReader ?? throw new ArgumentNullException(nameof(nmeaStreamReader));
            _nsr.Register(new GFDTA(), new GGA(), new GLL(), new GSA(), new GSV(), new RMC(), new VTG());
            _nsr.AbortAfterUnparsedLines = 10;
            _logger.LogInformation("Available Ports: {Ports}", String.Join(", ", SerialPortStream.GetPortNames()));
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken = default)
        {
            var portName = SerialPortStream.GetPortNames().First();
            _logger.LogInformation("Opening {0}", portName);
            using (var port = new SerialPortStream(portName))
            {
                port.Open();
                port.Handshake = Handshake.None;
                port.NewLine = "\r\n";
                port.ReadTimeout = 5000;
                port.Write("\r\n");

                var exitReason = await _nsr.ParseStreamAsync(port, async (message) => await DispatchMessage(message).ConfigureAwait(false), stoppingToken).ConfigureAwait(false);
                _logger.LogInformation("Exit Reason: {ExitReason}", exitReason);
            }
        }

        private Task DispatchMessage(NmeaMessage message)
        {
            _logger.LogInformation(message.ToString());
            return Task.CompletedTask;
        }
    }
}
