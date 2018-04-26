using System;
using System.Linq;
using System.Threading;

namespace RealtimePersister
{
    class Program
    {
        static private DataIngesterRunner _dataIngesterRunner;

        static void Main(string[] args)
        {
            _dataIngesterRunner = new DataIngesterRunner();
            CancellationTokenSource tokenSource = new CancellationTokenSource();

            int startMarketNo = 0;
            if (args.Count() > 0)
                startMarketNo = Int32.Parse(args[0]);

            var mainLoop = _dataIngesterRunner.RunSimulationAsync(tokenSource.Token, startMarketNo);

            System.Console.WriteLine("Press Enter to stop running");
            System.Console.ReadLine();

            tokenSource.Cancel();
            mainLoop.Wait();
        }
    }
}
