using RealtimePersister.Models.Streams;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace RealtimePersister.Models.Simulation
{
    public interface ISimulationReceiver
    {
        Task MarketAdded(StreamMarket streamItem);
        Task MarketUpdated(StreamMarket streamItem);
        Task MarketDeleted(StreamMarket streamItem);

        Task SubmarketAdded(StreamSubmarket streamItem);
        Task SubmarketUpdated(StreamSubmarket streamItem);
        Task SubmarketDeleted(StreamSubmarket streamItem);

        Task InstrumentAdded(StreamInstrument streamItem);
        Task InstrumentUpdated(StreamInstrument streamItem);
        Task InstrumentDeleted(StreamInstrument streamItem);

        Task PortfolioAdded(StreamPortfolio streamItem);
        Task PortfolioUpdated(StreamPortfolio streamItem);
        Task PortfolioDeleted(StreamPortfolio streamItem);

        Task PositionAdded(StreamPosition streamItem);
        Task PositionUpdated(StreamPosition streamItem);
        Task PositionDeleted(StreamPosition streamItem);

        Task OrderAdded(StreamOrder streamItem);
        Task OrderUpdated(StreamOrder streamItem);
        Task OrderDeleted(StreamOrder streamItem);

        Task TradeAdded(StreamTrade streamItem);
        Task TradeUpdated(StreamTrade streamItem);
        Task TradeDeleted(StreamTrade streamItem);

        Task RuleAdded(StreamRule streamItem);
        Task RuleUpdated(StreamRule streamItem);
        Task RuleDeleted(StreamRule streamItem);

        Task PriceAdded(StreamPrice streamItem);
        Task PriceUpdated(StreamPrice streamItem);
        Task PriceDeleted(StreamPrice streamItem);
    }
}
