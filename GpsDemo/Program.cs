using DKW.NMEA;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace GpsDemo
{
    internal static class Program
    {
        public static async Task Main(String[] args)
        {
            var useConsole = Debugger.IsAttached || args.Contains("--console");

            var builder = new HostBuilder()
                .ConfigureServices((hostContext, services) =>
                {
                    services.AddTransient<NmeaStreamReader>();
                    services.AddHostedService<GpsDemoService>();
                })
                .ConfigureLogging((hostingContext, logging) =>
                {
                    logging.AddConsole();
                    logging.SetMinimumLevel(LogLevel.Information);
                });

            if (useConsole)
            {
                await builder.RunConsoleAsync().ConfigureAwait(false);
            }
            else
            {
                await builder.RunAsServiceAsync().ConfigureAwait(false);
            }
        }
    }
}
