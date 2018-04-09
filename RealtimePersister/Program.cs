using RealtimePersister.DataIngester.Impl;
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

            var mainLoop = _dataIngesterRunner.RunSimulationAsync(tokenSource.Token);

            System.Console.WriteLine("Press Enter to stop running");
            System.Console.ReadLine();

            tokenSource.Cancel();
            mainLoop.Wait();
        }
    }
}
