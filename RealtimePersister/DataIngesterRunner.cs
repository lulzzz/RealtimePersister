using RealtimePersister.CosmosDb;
using RealtimePersister.Models.Simulation;
using System.Threading;
using System.Threading.Tasks;

namespace RealtimePersister
{
    public class DataIngesterRunner
    {
        private DataLayer _dataLayer = new DataLayer();
        private PersistenceLayer _persistenceLayer = new PersistenceLayer();
        private SimulationLayer _simulationLayer = null;

        public async Task RunSimulationAsync(CancellationToken cancellationToken, int startMarketNo = 0)
        {
            SimulationReceiver simulationReceiver = new SimulationReceiver(_dataLayer);

            int numThreads = 8;
            int numSubmarketsPerMarket = 4;
            int numInstrumentsPerMarket = 1000;
            int numPortfolios = 1000;
            int maxPositionsPerPortfolio = 50;
            int maxRulesPerPortfolio = 20;
            int numPriceUpdatesPerSecond = 0; // this is per market (or thread). 0 means as fast as possible

            _simulationLayer = new SimulationLayer(simulationReceiver);

            // Persister Factory
            IStreamPersisterFactory persisterFactory = new SqlStreamPersisterFactory();
            //IStreamPersisterFactory persisterFactory = new CosmosDbStreamPersisterFactory();
            //IStreamPersisterFactory persisterFactory = new RealtimePersister.Redis.StreamPersisterFactory("pb-syncweek-redis.redis.cache.windows.net:6380,password=IG1aBMjxzo0uE106LJT+Ceigc1AZldzwd9HYDDKIdBc=,ssl=True,abortConnect=False");
            IStreamPersister persister = null;

            if (persisterFactory != null)
                persister = persisterFactory.CreatePersister("DataIngester");

            // initialize the persistence layer
            var ret = await _persistenceLayer.Initialize(persister, cancellationToken);
            // initialize the data layer
            await _dataLayer.Initialize(cancellationToken, _persistenceLayer);

            for (int marketNo = startMarketNo; marketNo < (startMarketNo + numThreads); marketNo++)
            {
                await _simulationLayer.GenerateData(marketNo, numSubmarketsPerMarket, numInstrumentsPerMarket,
                        (marketNo == 0 ? numPortfolios : 0), (marketNo == 0 ? maxPositionsPerPortfolio : 0), (marketNo == 0 ? maxRulesPerPortfolio : 0));
            }

            var marketTasks = new Task[numThreads];
            for (int marketNo = startMarketNo; marketNo < (startMarketNo + numThreads); marketNo++)
            {
                var marketNoCopy = marketNo;
                marketTasks[marketNoCopy - startMarketNo] = Task.Run(async () =>
                {
                });
            }
            await Task.WhenAll(marketTasks);
            
            for (int marketNo = 0; marketNo < numThreads; marketNo++)
            {
                var marketNoCopy = marketNo;
                marketTasks[marketNoCopy] = Task.Run(async () =>
                {
                    await _simulationLayer.SimulatePrices(cancellationToken, marketNoCopy, numPriceUpdatesPerSecond, persister);
                });
            }
            await Task.WhenAll(marketTasks);
            
        }
    }
}
