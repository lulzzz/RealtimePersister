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

        private int numThreads ;
        private int numSubmarketsPerMarket ;
        private int numInstrumentsPerMarket ;
        private int numPortfolios;
        private int maxPositionsPerPortfolio;
        private int maxRulesPerPortfolio;
        private int numPriceUpdatesPerSecond;
        private IStreamPersisterFactory streamPersisterFactory;
        //private int typeOfPersister;

        public DataIngesterRunner(int numThreads, int numSubmarketsPerMarket, int numInstrumentsPerMarket, int numPortfolios, int maxPositionsPerPortfolio, int maxRulesPerPortfolio, int numPriceUpdatesPerSecond, IStreamPersisterFactory streamPersisterFactory)
        {
            this.numThreads = numThreads;
            this.numSubmarketsPerMarket = numSubmarketsPerMarket;
            this.numInstrumentsPerMarket = numInstrumentsPerMarket;
            this.numPortfolios = numPortfolios;
            this.maxPositionsPerPortfolio = maxPositionsPerPortfolio;
            this.maxRulesPerPortfolio = maxRulesPerPortfolio;
            this.numPriceUpdatesPerSecond = numPriceUpdatesPerSecond;
            this.streamPersisterFactory = streamPersisterFactory;
        }

        public async Task RunSimulationAsync(CancellationToken cancellationToken)
        {
            SimulationReceiver simulationReceiver = new SimulationReceiver(_dataLayer);
            System.Diagnostics.Debug.WriteLine("Started Simulator");
            
            _simulationLayer = new SimulationLayer(simulationReceiver);

            // Persister Factory
            IStreamPersister persister = null;

            if (streamPersisterFactory != null)
            {
                persister = streamPersisterFactory.CreatePersister("DataIngester");
                System.Diagnostics.Debug.WriteLine("Creating Data Persister");
            }  

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
