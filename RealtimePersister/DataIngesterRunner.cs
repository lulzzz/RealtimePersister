﻿using RealtimePersister.Models.Simulation;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace RealtimePersister.DataIngester.Impl
{
    public class DataIngesterRunner
    {
        private DataLayer _dataLayer = new DataLayer();
        private PersistenceLayer _persistenceLayer = new PersistenceLayer();
        private SimulationLayer _simulationLayer = null;

        public async Task RunSimulationAsync(CancellationToken cancellationToken)
        {
            SimulationReceiver simulationReceiver = new SimulationReceiver(_dataLayer);

            int numMarkets = 8;
            int numSubmarketsPerMarket = 4;
            int numInstrumentsPerMarket = 1000;
            int numPortfolios = 1000;
            int maxPositionsPerPortfolio = 50;
            int maxRulesPerPortfolio = 20;
            int numPriceUpdatesPerSecond = 0; // this is per market (or thread). 0 means as fast as possible

            _simulationLayer = new SimulationLayer(simulationReceiver);

            // Persister Factory
            IStreamPersisterFactory persisterFactory = null; /* TODO */
            IStreamPersister persister = null;

            if (persisterFactory != null)
                persister = persisterFactory.CreatePersister("DataIngester");

            // initialize the persistence layer
            var ret = await _persistenceLayer.Initialize(persister, cancellationToken);
            // initialize the data layer
            await _dataLayer.Initialize(cancellationToken, _persistenceLayer);

            var marketTasks = new Task[numMarkets];
            for (int marketNo = 0; marketNo < numMarkets; marketNo++)
            {
                marketTasks[marketNo] = Task.Run(async () =>
                {
                    await _simulationLayer.LoadData(marketNo);
                    bool addedData = await _simulationLayer.GenerateData(marketNo, numSubmarketsPerMarket, numInstrumentsPerMarket, 
                        (marketNo == 0 ? numPortfolios : 0), (marketNo == 0 ? maxPositionsPerPortfolio : 0), (marketNo == 0 ? maxRulesPerPortfolio : 0));
                    if (addedData)
                    {
                        await _simulationLayer.SaveData(marketNo);
                    }
                    await _simulationLayer.SimulatePrices(cancellationToken, marketNo, numPriceUpdatesPerSecond);

                    await _simulationLayer.SaveData(marketNo);
                });
            }
        }
    }
}