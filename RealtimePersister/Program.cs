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

            var _tasks =new List<Task>();
            int _numThreads = 8;
            int _numSubmarketsPerMarket = 4;
            int _numInstrumentsPerMarket = 1000;
            int _numPortFolios = 1000;
            int _maxPositionsPerPortfolio = 50;
            int _maxRulesPerPortfolio = 20;
            int _numPriceUpdatesPerSecond = 0;
            bool _isCosmosDBPersistenceOn = false;
            bool _isRedisPersistenceOn = true;
            bool _isSqlPersistenceOn = false;
            IStreamPersisterFactory _persisterFactoryCosmos = null;
            IStreamPersisterFactory _persisterFactoryRedis = null;
            IStreamPersisterFactory _persisterFactorySQL = null;

            CancellationTokenSource tokenSource = new CancellationTokenSource();

            if (_isCosmosDBPersistenceOn) {
                //TODO
                _persisterFactoryCosmos = null;
                _dataIngesterRunnerCosmos = new DataIngesterRunner(_numThreads, _numSubmarketsPerMarket, _numInstrumentsPerMarket, _numPortFolios, _maxPositionsPerPortfolio, _maxRulesPerPortfolio, _numPriceUpdatesPerSecond, _persisterFactoryCosmos);
                _tasks.Add(_dataIngesterRunnerCosmos.RunSimulationAsync(tokenSource.Token));

            }

            if (_isRedisPersistenceOn)
            {
                _persisterFactoryRedis = new RealtimePersister.Redis.StreamPersisterFactory("pb-syncweek-redis.redis.cache.windows.net:6380,password=IG1aBMjxzo0uE106LJT+Ceigc1AZldzwd9HYDDKIdBc=,ssl=True,abortConnect=False");
                _dataIngesterRunnerRedis = new DataIngesterRunner(_numThreads, _numSubmarketsPerMarket, _numInstrumentsPerMarket, _numPortFolios, _maxPositionsPerPortfolio, _maxRulesPerPortfolio, _numPriceUpdatesPerSecond, _persisterFactoryRedis);
                _tasks.Add(_dataIngesterRunnerRedis.RunSimulationAsync(tokenSource.Token));
            }

            if (_isSqlPersistenceOn)
            {
                _persisterFactorySQL = null;
                _dataIngesterRunnerSQL = new DataIngesterRunner(_numThreads, _numSubmarketsPerMarket, _numInstrumentsPerMarket, _numPortFolios, _maxPositionsPerPortfolio, _maxRulesPerPortfolio, _numPriceUpdatesPerSecond, _persisterFactorySQL);
                _tasks.Add(_dataIngesterRunnerSQL.RunSimulationAsync(tokenSource.Token));
            }
            //IStreamPersisterFactory persisterFactory = new RealtimePersister.Redis.StreamPersisterFactory("pb-syncweek-redis.redis.cache.windows.net:6380,password=IG1aBMjxzo0uE106LJT+Ceigc1AZldzwd9HYDDKIdBc=,ssl=True,abortConnect=False");
            //IStreamPersisterFactory persisterFactory = new RealtimePersister.Redis.StreamPersisterFactory("localhost");

              

            System.Console.WriteLine("Press Enter to stop running");
            System.Console.ReadLine();

            tokenSource.Cancel();
            System.Console.WriteLine("Task size" + _tasks.Count);
            Task.WhenAll(_tasks).Wait();
            
           
        }
    }
}
