using Newtonsoft.Json;
using RealtimePersister.Models.Streams;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace RealtimePersister.Models.Simulation
{
    public class SimulationLayer
    {
        public ConcurrentDictionary<string, Market> MarketsById { get; private set; } = new ConcurrentDictionary<string, Market>();
        public ConcurrentDictionary<string, string> MarketsByName { get; } = new ConcurrentDictionary<string, string>();
        public ConcurrentDictionary<string, Portfolio> PortfoliosById { get; private set; } = new ConcurrentDictionary<string, Portfolio>();
        public ConcurrentDictionary<string, string> PortfoliosByName { get; } = new ConcurrentDictionary<string, string>();

        private ISimulationReceiver _simulationReceiver = null;

        public SimulationLayer(ISimulationReceiver simulationReceiver)
        {
            _simulationReceiver = simulationReceiver;
        }

        public async Task LoadData(int marketNo)
        {
            await LoadMarkets($"market-{marketNo}.json");
            if (marketNo == 0)
                await LoadPortfolios("portfolios.json");
        }

        public async Task SaveData(int marketNo)
        {
            await SaveMarkets($"market-{marketNo}.json");
            if (marketNo == 0)
                await SavePortfolios("portfolios.json");
        }

        public async Task<bool> GenerateData(int marketNo, int numSubmarketsPerMarket = 4, int numInstrumentsToGenerate = 1000,
            int numPortfoliosToGenerate = 1000, int maxPositionsPerPortfolio = 50, int maxRulesPerPortfolio = 20)
        {
            int numInstruments = 0;
            int numPortfolios = 0;
            bool addedData = false;

            // check to see if we need generate more data. We use numInstruments and numPortfolios to do this
            foreach (var market in MarketsById.Values)
            {
                foreach (var submarket in market.SubmarketsById.Values)
                {
                    numInstruments += submarket.NumInstruments;
                }
            }
            numPortfolios = PortfoliosById.Count;

            // TODO: Let's go nuts here and generate a ton of random (or semi random shit)
            // If we use the same seed we should alwyays end up with the same result given that the parameters
            // are the same.
            var rand = new Random(25839);

            if (numInstruments < numInstrumentsToGenerate)
            {
                addedData = true;
                // generate markets, submarkets and instruments
                if (numInstruments == 0)
                {
                    var market = await GenerateMarket(marketNo, numSubmarketsPerMarket, numInstrumentsToGenerate, rand);
                    MarketsById[market.Id] = market;
                    MarketsByName[market.Name] = market.Id;
                }
                else
                {
                    // How manu more instruments to generate
                    int numInstrumentsToAdd = numInstrumentsToGenerate - numInstruments;
                    // how many submarkets do we have?
                    int numSubmarkets = 0;
                    foreach (var market in MarketsById.Values)
                    {
                        numSubmarkets += market.SubmarketsById.Count;
                    }

                    int numInstrumentsToAddPerSubmarket = (numInstrumentsToAdd / numSubmarkets) + 1;

                    int marketNoLoop = 0;

                    foreach (var market in MarketsById.Values)
                    {
                        int submarketNo = 0;

                        foreach (var submarket in market.SubmarketsById.Values)
                        {
                            int instrumentNoBase = submarket.NumInstruments;

                            for (int instrumentNo = 0; instrumentNo < numInstrumentsToAddPerSubmarket; instrumentNo++)
                            {
                                var instrument = await GenerateInstrument(submarket.Id, marketNoLoop, submarketNo, instrumentNoBase + instrumentNo, rand);
                                submarket.InstrumentsById[instrument.Id] = instrument;
                                submarket.InstrumentsByName[instrument.Name] = instrument.Id;
                                submarket.NumInstruments++;
                            }
                            submarketNo++;
                        }
                        marketNoLoop++;
                    }
                }
            }

            if (numPortfolios < numPortfoliosToGenerate)
            {
                addedData = true;
                // generate portfolios and positions
                for (int portfolioNo = numPortfolios; portfolioNo < numPortfoliosToGenerate; portfolioNo++)
                {
                    var portfolio = await GeneratePortfolio(portfolioNo, maxPositionsPerPortfolio, maxRulesPerPortfolio, rand);
                    PortfoliosById[portfolio.Id] = portfolio;
                    PortfoliosByName[portfolio.Name] = portfolio.Id;
                }
            }

            return addedData;
        }

        protected async Task<Market> GenerateMarket(int marketNo, int numSubmarketsPerMarket, int numInstruments, Random rand)
        {
            var market = new Market()
            {
                Id = $"Market:{marketNo}",
                Name = $"Market{marketNo}"
            };

            if (_simulationReceiver != null)
                await _simulationReceiver.MarketAdded(market.ToStream(StreamOperation.Insert));

            for (int submarketNo = 0; submarketNo < numSubmarketsPerMarket; submarketNo++)
            {
                var submarket = await GenerateSubmarket(market.Id, marketNo, submarketNo, numInstruments / numSubmarketsPerMarket, rand);
                market.SubmarketsById[submarket.Id] = submarket;
                market.SubmarketsByName[submarket.Name] = submarket.Id;
            }

            return market;
        }

        protected async Task<Submarket> GenerateSubmarket(string marketId, int marketNo, int submarketNo, int numInstruments, Random rand)
        {
            var submarket = new Submarket()
            {
                Id = $"Submarket:{marketNo}-{submarketNo}",
                MarketId = marketId,
                Name = $"Submarket{marketNo}-{submarketNo}"
            };

            if (_simulationReceiver != null)
                await _simulationReceiver.SubmarketAdded(submarket.ToStream(StreamOperation.Insert));

            for (int instrumentNo = 0; instrumentNo < numInstruments; instrumentNo++)
            {
                var instrument = await GenerateInstrument(submarket.Id, marketNo, submarketNo, instrumentNo, rand);
                submarket.InstrumentsById[instrument.Id] = instrument;
                submarket.InstrumentsByName[instrument.Name] = instrument.Id;
                submarket.NumInstruments++;
            }

            return submarket;
        }

        protected async Task<Instrument> GenerateInstrument(string submarketId, int marketNo, int submarketNo, int instrumentNo, Random rand)
        {
            var instrument = new Instrument()
            {
                Id = $"Instrument:{marketNo}-{submarketNo}-{instrumentNo}",
                SubmarketId = submarketId,
                Name = $"Instrument{marketNo}-{submarketNo}-{instrumentNo}",
                PriceLatest = rand.Next(500),
                PriceDate = DateTime.UtcNow
            };

            if (_simulationReceiver != null)
                await _simulationReceiver.InstrumentAdded(instrument.ToStream(StreamOperation.Insert));

            return instrument;
        }

        protected async Task<Portfolio> GeneratePortfolio(int portfolioNo, int maxPositionsPerPortfolio, int maxRulesPerPortfolio, Random rand)
        {
            var portfolio = new Portfolio()
            {
                Id = $"Portfolio:{portfolioNo}",
                Name = $"Portfolio-{portfolioNo}",
                Balance = rand.Next(1000000)
            };

            if (_simulationReceiver != null)
                await _simulationReceiver.PortfolioAdded(portfolio.ToStream(StreamOperation.Insert));

            await GeneratePositions(portfolio, portfolioNo, maxPositionsPerPortfolio, rand);
            await GenerateRules(portfolio, portfolioNo, maxRulesPerPortfolio, rand);

            return portfolio;
        }

        protected async Task GeneratePositions(Portfolio portfolio, int portfolioNo, int maxPositionsPerPortfolio, Random rand)
        {
            int numPositions = Math.Max(rand.Next(maxPositionsPerPortfolio), 1);
            int marketNo = 0;
            int submarketNo = 0;
            int positionNo = 0;

            while (positionNo < numPositions)
            {
                foreach (var market in MarketsById.Values)
                {
                    foreach (var submarket in market.SubmarketsById.Values)
                    {
                        var instrument = GetRandomInstrumentId(rand, marketNo, submarketNo, submarket);
                        int numInstruments = submarket.NumInstruments;
                        var slotSeed = rand.Next(41);
                        int slot = (slotSeed > 17 ? 0 : slotSeed > 5 ? 1 : slotSeed > 1 ? 2 : 3);
                        int instrumentNo = rand.Next(numInstruments / 4) + (slot * numInstruments / 4);
                        string instrumentName = $"Instrument{marketNo}-{submarketNo}-{instrumentNo}";

                        // Find the instrument
                        string instrumentId = null;
                        if (submarket.InstrumentsByName.TryGetValue(instrumentName, out instrumentId))
                        {
                            // create position
                            var position = new Position()
                            {
                                Id = $"Position:{portfolioNo}-{positionNo}",
                                PortfolioId = portfolio.Id,
                                InstrumentId = instrumentId,
                                Price = rand.Next(500),
                                Volume = rand.Next(2000)
                            };

                            if (_simulationReceiver != null)
                                await _simulationReceiver.PositionAdded(position.ToStream(StreamOperation.Insert));

                            portfolio.Positions[position.InstrumentId] = position;

                            submarketNo++;
                            if (submarketNo >= market.SubmarketsById.Count)
                                submarketNo = 0;
                            positionNo++;
                        }
                    }

                    marketNo++;
                    if (marketNo >= MarketsById.Count)
                        marketNo = 0;
                }
            }
        }

        protected async Task GenerateRules(Portfolio portfolio, int portfolioNo, int maxRulesPerPortfolio, Random rand)
        {
            int numRules = Math.Max(rand.Next(maxRulesPerPortfolio), 1);

            for (int ruleNo = 0; ruleNo < numRules; ruleNo++)
            {
                var rule = new Rule()
                {
                    Id = $"Rule:{portfolioNo}-{ruleNo}",
                    PortfolioId = portfolio.Id,
                    Expression = "TODO"
                };

                if (_simulationReceiver != null)
                    await _simulationReceiver.RuleAdded(rule.ToStream(StreamOperation.Insert));

                portfolio.Rules.Add(rule);
            }
        }

        public class SubmarketInfo
        {
            public int MarketNo { get; set; }
            public int SubmarketNo { get; set; }
            public Submarket Submarket { get; set; }
        }

        public Task SimulatePrices(CancellationToken cancellationToken, int marketNo, int priceUpdatesPerSecond)
        {
            return Task.Run(async () =>
            {
                var rand = new Random();
                int numSlotsPerSecond = (priceUpdatesPerSecond > 1000 ? 40 : 20) + 1;
                int priceUpdatesPerSlot = (priceUpdatesPerSecond / numSlotsPerSecond) + 1;

                // generate a flat list of submarkets to generate prices for
                List<SubmarketInfo> submarkets = new List<SubmarketInfo>();

                foreach (var market in MarketsById.Values)
                {
                    int marketNoFromMarket = Int32.Parse(market.Name.Substring(6));
                    if (marketNoFromMarket == marketNo)
                    {
                        foreach (var submarket in market.SubmarketsById.Values)
                        {
                            int submarketNo = Int32.Parse(submarket.Name.Substring(11));
                            submarkets.Add(new SubmarketInfo()
                            {
                                MarketNo = marketNo,
                                SubmarketNo = submarketNo,
                                Submarket = submarket
                            });
                        }
                    }
                }

                // this loop repeats every second
                do
                {
                    int numSubmarkets = submarkets.Count;
                    int submarketNo = 0;
                    var swSecond = new Stopwatch();
                    swSecond.Start();
                    for (int currentSlotInSecond = 0; currentSlotInSecond < numSlotsPerSecond; currentSlotInSecond++)
                    {
                        var swSlot = new Stopwatch();
                        swSlot.Start();
                        for (int numUpdatesInSlot = 0; numUpdatesInSlot < priceUpdatesPerSlot; numUpdatesInSlot++)
                        {
                            var submarketInfo = submarkets[submarketNo];
                            var instrument = GetRandomInstrument(rand, submarketInfo.MarketNo, submarketInfo.SubmarketNo, submarketInfo.Submarket);
                            instrument.Id = instrument.Id.Replace("Instrument:", "Price:");
                            instrument.PriceLatest += (rand.Next(10) > 4 ? 0.05 : -0.05);
                            instrument.PriceDate = DateTime.UtcNow;
                            if (_simulationReceiver != null)
                                await _simulationReceiver.PriceUpdated(instrument.ToPriceStream(StreamOperation.Update));
                            ReportSend();
                            submarketNo++;
                            if (submarketNo >= numSubmarkets)
                                submarketNo = 0;
                        }
                        swSlot.Stop();
                        if (priceUpdatesPerSecond > 0)
                        {
                            int timeLeftBeforeNextSlot = (1000 / numSlotsPerSecond) - (int)swSlot.ElapsedMilliseconds - 2 /* need som extra time I think */;
                            if (timeLeftBeforeNextSlot > 0)
                                Thread.Sleep(timeLeftBeforeNextSlot);
                        }
                    }
                    swSecond.Stop();
                    if (priceUpdatesPerSecond > 0)
                    {
                        int timeLeftBeforeNextSecond = 1000 - (int)swSecond.ElapsedMilliseconds - 10 /* need some extra time I think */;
                        if (timeLeftBeforeNextSecond > 0)
                            Thread.Sleep(timeLeftBeforeNextSecond);
                    }
                } while (!cancellationToken.IsCancellationRequested);
            });
        }

        static DateTime _timeLastStatusReport = DateTime.UtcNow;
        static int _numUpdatesSent = 0;
        static object _locker = new object();
        static TimeSpan _tenSeconds = TimeSpan.FromSeconds(10);

        static protected void ReportSend()
        {
            Interlocked.Increment(ref _numUpdatesSent);

            var timeLastStatusReportSaved = _timeLastStatusReport;
            var timePassed = (DateTime.UtcNow - _timeLastStatusReport);
            if (timePassed > _tenSeconds)
            {
                lock (_locker)
                {
                    if (_timeLastStatusReport == timeLastStatusReportSaved)
                    {
                        Console.WriteLine($"Currently sending {_numUpdatesSent / timePassed.TotalSeconds} price updates per second.");
                        _timeLastStatusReport = DateTime.UtcNow;
                        Interlocked.Exchange(ref _numUpdatesSent, 0);
                    }
                }
            }
        }

        protected string GetRandomInstrumentId(Random rand, int marketNo, int submarketNo, Submarket submarket)
        {
            int numInstruments = submarket.NumInstruments;
            var slotSeed = rand.Next(41);
            int slot = (slotSeed > 17 ? 0 : slotSeed > 5 ? 1 : slotSeed > 1 ? 2 : 3);
            int instrumentNo = rand.Next(numInstruments / 4) + (slot * numInstruments / 4);
            string instrumentName = "Instrument" + marketNo.ToString() + "-" + submarketNo.ToString() + "-" + instrumentNo.ToString();

            // Find the instrument
            string instrumentId = null;
            return (submarket.InstrumentsByName.TryGetValue(instrumentName, out instrumentId) ? instrumentId : null);
        }

        protected Instrument GetRandomInstrument(Random rand, int marketNo, int submarketNo, Submarket submarket)
        {
            var instrumentId = GetRandomInstrumentId(rand, marketNo, submarketNo, submarket);
            if (!String.IsNullOrEmpty(instrumentId))
            {
                Instrument instrument = null;
                return (submarket.InstrumentsById.TryGetValue(instrumentId, out instrument) ? instrument : null);
            }
            else
                return null;
        }

        private async Task LoadMarkets(string filename)
        {
            try
            {
                var json = File.ReadAllText(filename);

                MarketsById = JsonConvert.DeserializeObject<ConcurrentDictionary<string, Market>>(json);
                foreach (var market in MarketsById.Values)
                {
                    MarketsByName[market.Name] = market.Id;
                }

                if (_simulationReceiver != null)
                {
                    // let's tell the simulation receiver about all of this
                    foreach (var market in MarketsById.Values)
                    {
                        await _simulationReceiver.MarketAdded(market.ToStream(StreamOperation.Insert));
                        foreach (var submarket in market.SubmarketsById.Values)
                        {
                            await _simulationReceiver.SubmarketAdded(submarket.ToStream(StreamOperation.Insert));
                            foreach (var instrument in submarket.InstrumentsById.Values)
                            {
                                await _simulationReceiver.InstrumentAdded(instrument.ToStream(StreamOperation.Insert));
                            }
                        }
                    }
                }
            }
            catch (Exception)
            {
            }
        }

        private Task SaveMarkets(string filename)
        {
            try
            {
                var json = JsonConvert.SerializeObject(MarketsById);
                File.WriteAllText(filename, json);
            }
            catch (Exception)
            {
            }
            return Task.CompletedTask;
        }

        private async Task LoadPortfolios(string filename)
        {
            try
            {
                var json = File.ReadAllText(filename);
                PortfoliosById = JsonConvert.DeserializeObject<ConcurrentDictionary<string, Portfolio>>(json);
                foreach (var portfolio in PortfoliosById.Values)
                {
                    PortfoliosByName[portfolio.Name] = portfolio.Id;
                }

                if (_simulationReceiver != null)
                {
                    foreach (var portfolio in PortfoliosById.Values)
                    {
                        await _simulationReceiver.PortfolioAdded(portfolio.ToStream(StreamOperation.Insert));
                        foreach (var position in portfolio.Positions.Values)
                        {
                            await _simulationReceiver.PositionAdded(position.ToStream(StreamOperation.Insert));
                        }
                        foreach (var rule in portfolio.Rules)
                        {
                            await _simulationReceiver.RuleAdded(rule.ToStream(StreamOperation.Insert));
                        }
                    }
                }
            }
            catch (Exception)
            {
            }
        }

        private Task SavePortfolios(string filename)
        {
            try
            {
                var json = JsonConvert.SerializeObject(PortfoliosById);
                File.WriteAllText(filename, json);
            }
            catch (Exception)
            {
            }
            return Task.CompletedTask;
        }

    }
}
