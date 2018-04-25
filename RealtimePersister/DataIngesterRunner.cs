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

        public async Task RunSimulationAsync(CancellationToken cancellationToken)
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
            IStreamPersisterFactory persisterFactory = new CosmosDbStreamPersisterFactory();
            IStreamPersister persister = null;

            if (persisterFactory != null)
                persister = persisterFactory.CreatePersister("DataIngester");

            // initialize the persistence layer
            var ret = await _persistenceLayer.Initialize(persister, cancellationToken);
            // initialize the data layer
            await _dataLayer.Initialize(cancellationToken, _persistenceLayer);

            var marketTasks = new Task[numThreads];
            for (int marketNo = 0; marketNo < numThreads; marketNo++) {
                var marketNoCopy = marketNo;
                marketTasks[marketNoCopy] = Task.Run(async () =>
                {
                    await _simulationLayer.LoadData(marketNoCopy);
                    bool addedData = await _simulationLayer.GenerateData(marketNoCopy, numSubmarketsPerMarket, numInstrumentsPerMarket,
                        (marketNoCopy == 0 ? numPortfolios : 0), (marketNoCopy == 0 ? maxPositionsPerPortfolio : 0), (marketNoCopy == 0 ? maxRulesPerPortfolio : 0));
                    if (addedData) {
                        await _simulationLayer.SaveData(marketNoCopy);
                    }
                    await _simulationLayer.SimulatePrices(cancellationToken, marketNoCopy, numPriceUpdatesPerSecond);
                    await _simulationLayer.SaveData(marketNoCopy);
                });
            }
            await Task.WhenAll(marketTasks);
        }
    }
}
