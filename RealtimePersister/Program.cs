using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace RealtimePersister
{
    class Program
    {
        static private DataIngesterRunner _dataIngesterRunnerCosmos;
        static private DataIngesterRunner _dataIngesterRunnerRedis;
        static private DataIngesterRunner _dataIngesterRunnerSQL;
        static void Main(string[] args)
        {

            var tasks =new List<Task>();
            int numThreads = 8;
            int numSubmarketsPerMarket = 4;
            int numInstrumentsPerMarket = 1000;
            int numPortFolios = 1000;
            int maxPositionsPerPortfolio = 50;
            int maxRulesPerPortfolio = 20;
            int numPriceUpdatesPerSecond = 0;

            //int typeOfPersister = 0; //0=Cosmos DB, 1 =Redis, 2= SQL
            _dataIngesterRunnerCosmos = new DataIngesterRunner(numThreads, numSubmarketsPerMarket, numInstrumentsPerMarket, numPortFolios, maxPositionsPerPortfolio, maxRulesPerPortfolio, numPriceUpdatesPerSecond,null);
            CancellationTokenSource tokenSource = new CancellationTokenSource();
            //var mainLoop1 = _dataIngesterRunnerCosmos.RunSimulationAsync(tokenSource.Token);
            tasks.Add(_dataIngesterRunnerCosmos.RunSimulationAsync(tokenSource.Token));

            _dataIngesterRunnerRedis = new DataIngesterRunner(numThreads, numSubmarketsPerMarket, numInstrumentsPerMarket, numPortFolios, maxPositionsPerPortfolio, maxRulesPerPortfolio, numPriceUpdatesPerSecond,new RedisStreamPersisterFactory());
            //var mainLoop2 =_dataIngesterRunnerRedis.RunSimulationAsync(tokenSource.Token);
            tasks.Add(_dataIngesterRunnerRedis.RunSimulationAsync(tokenSource.Token));

            _dataIngesterRunnerSQL = new DataIngesterRunner(numThreads, numSubmarketsPerMarket, numInstrumentsPerMarket, numPortFolios, maxPositionsPerPortfolio, maxRulesPerPortfolio, numPriceUpdatesPerSecond, null);
            tasks.Add(_dataIngesterRunnerSQL.RunSimulationAsync(tokenSource.Token));

            System.Console.WriteLine("Press Enter to stop running");
            System.Console.ReadLine();

            tokenSource.Cancel();
            Task.WhenAll(tasks).Wait();
        }
    }
}
