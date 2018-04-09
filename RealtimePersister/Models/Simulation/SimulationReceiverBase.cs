using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using RealtimePersister.Models.Streams;

namespace RealtimePersister.Models.Simulation
{
    public class SimulationReceiverBase : ISimulationReceiver
    {
        public virtual Task InstrumentAdded(StreamInstrument streamItem)
        {
            return Task.CompletedTask;
        }

        public virtual Task InstrumentDeleted(StreamInstrument streamItem)
        {
            return Task.CompletedTask;
        }

        public virtual Task InstrumentUpdated(StreamInstrument streamItem)
        {
            return Task.CompletedTask;
        }

        public virtual Task MarketAdded(StreamMarket streamItem)
        {
            return Task.CompletedTask;
        }

        public virtual Task MarketDeleted(StreamMarket streamItem)
        {
            return Task.CompletedTask;
        }

        public virtual Task MarketUpdated(StreamMarket streamItem)
        {
            return Task.CompletedTask;
        }

        public virtual Task OrderAdded(StreamOrder streamItem)
        {
            return Task.CompletedTask;
        }

        public virtual Task OrderDeleted(StreamOrder streamItem)
        {
            return Task.CompletedTask;
        }

        public virtual Task OrderUpdated(StreamOrder streamItem)
        {
            return Task.CompletedTask;
        }

        public virtual Task PortfolioAdded(StreamPortfolio streamItem)
        {
            return Task.CompletedTask;
        }

        public virtual Task PortfolioDeleted(StreamPortfolio streamItem)
        {
            return Task.CompletedTask;
        }

        public virtual Task PortfolioUpdated(StreamPortfolio streamItem)
        {
            return Task.CompletedTask;
        }

        public virtual Task PositionAdded(StreamPosition streamItem)
        {
            return Task.CompletedTask;
        }

        public virtual Task PositionDeleted(StreamPosition streamItem)
        {
            return Task.CompletedTask;
        }

        public virtual Task PositionUpdated(StreamPosition streamItem)
        {
            return Task.CompletedTask;
        }

        public virtual Task PriceAdded(StreamPrice streamItem)
        {
            return Task.CompletedTask;
        }

        public virtual Task PriceDeleted(StreamPrice streamItem)
        {
            return Task.CompletedTask;
        }

        public virtual Task PriceUpdated(StreamPrice streamItem)
        {
            return Task.CompletedTask;
        }

        public virtual Task RuleAdded(StreamRule streamItem)
        {
            return Task.CompletedTask;
        }

        public virtual Task RuleDeleted(StreamRule streamItem)
        {
            return Task.CompletedTask;
        }

        public virtual Task RuleUpdated(StreamRule streamItem)
        {
            return Task.CompletedTask;
        }

        public virtual Task SubmarketAdded(StreamSubmarket streamItem)
        {
            return Task.CompletedTask;
        }

        public virtual Task SubmarketDeleted(StreamSubmarket streamItem)
        {
            return Task.CompletedTask;
        }

        public virtual Task SubmarketUpdated(StreamSubmarket streamItem)
        {
            return Task.CompletedTask;
        }

        public virtual Task TradeAdded(StreamTrade streamItem)
        {
            return Task.CompletedTask;
        }

        public virtual Task TradeDeleted(StreamTrade streamItem)
        {
            return Task.CompletedTask;
        }

        public virtual Task TradeUpdated(StreamTrade streamItem)
        {
            return Task.CompletedTask;
        }
    }
}
