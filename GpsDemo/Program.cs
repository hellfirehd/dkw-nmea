using DKW.NMEA;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using System;
using System.Threading.Tasks;

namespace GpsDemo
{
    internal static class Program
    {
        public static async Task Main(String[] args)
        {
            var builder = new HostBuilder()
                .ConfigureServices((hostContext, services) =>
                {
                    services.AddTransient<NmeaStreamReader>();
                    services.AddHostedService<GpsDemoService>();
                })
                .ConfigureLogging((hostingContext, logging) =>
                {
                    logging.AddConsole();
                    logging.SetMinimumLevel(LogLevel.Debug);
                });

            await builder.RunConsoleAsync().ConfigureAwait(false);
        }
    }
}
