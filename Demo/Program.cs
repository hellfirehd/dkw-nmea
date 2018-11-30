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

namespace TestApp
{
    using System;
    using System.Diagnostics;
    using System.Threading;
    using System.Threading.Tasks;
    using DKW.NMEA;

    internal class Program
    {
        private static readonly CancellationTokenSource _cancelationTokenSource = new CancellationTokenSource();

        private static async Task Main(String[] args)
        {
            Console.CancelKeyPress += new ConsoleCancelEventHandler(OnExitRequested);

            var count = 0L;

            using (var nr = new NmeaReader(RH.GetResourceStream("track2.nmea")))
            {
                while (true)
                {
                    var message = nr.ReadNext(_cancelationTokenSource.Token);
                    if (message == null)
                    {
                        break;
                    }
                    Console.WriteLine($"Stage 1 {++count}: {message}");
                }
            }

            using (var nr = new NmeaReader(RH.GetResourceStream("track2.nmea")))
            {
                while (true)
                {
                    var message = await nr.ReadNextAsync(_cancelationTokenSource.Token).ConfigureAwait(false);
                    if (message == null)
                    {
                        break;
                    }
                    Console.WriteLine($"State 2 {++count}: {message}");
                }
            }

            var nsr = NmeaStreamReader.Create();
            using (var reader = RH.GetResourceStream("track1.nmea"))
            {
                await nsr.ParseStreamAsync(reader, (s) =>
                {
                    Console.WriteLine($"Stage 3 {++count}: {s}");
                }, _cancelationTokenSource.Token).ConfigureAwait(false);
            }

            if (Debugger.IsAttached)
            {
                Console.WriteLine("Press ENTER to EXIT . . .");
                Console.ReadLine();
            }
        }

        private static void OnExitRequested(Object sender, ConsoleCancelEventArgs e)
        {
            Console.WriteLine("Requesting EXIT . . .");
            _cancelationTokenSource.Cancel();
            Console.WriteLine("EXIT Requested . . .");
            e.Cancel = true;
        }
    }
}
