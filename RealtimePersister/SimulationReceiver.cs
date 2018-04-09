using RealtimePersister.Models.Simulation;
using RealtimePersister.Models.Streams;
using System.Threading.Tasks;

namespace RealtimePersister
{
    public class SimulationReceiver : SimulationReceiverBase
    {
        private DataLayer _dataLayer;

        public SimulationReceiver(DataLayer dataLayer)
        {
            _dataLayer = dataLayer;
        }

        public override Task InstrumentAdded(StreamInstrument streamItem)
        {
            return _dataLayer.ProcessStreamItem(streamItem);
        }

        public override Task InstrumentDeleted(StreamInstrument streamItem)
        {
            return _dataLayer.ProcessStreamItem(streamItem);
        }

        public override Task InstrumentUpdated(StreamInstrument streamItem)
        {
            return _dataLayer.ProcessStreamItem(streamItem);
        }

        public override Task MarketAdded(StreamMarket streamItem)
        {
            return _dataLayer.ProcessStreamItem(streamItem);
        }

        public override Task MarketDeleted(StreamMarket streamItem)
        {
            return _dataLayer.ProcessStreamItem(streamItem);
        }

        public override Task MarketUpdated(StreamMarket streamItem)
        {
            return _dataLayer.ProcessStreamItem(streamItem);
        }

        public override Task OrderAdded(StreamOrder streamItem)
        {
            return _dataLayer.ProcessStreamItem(streamItem);
        }

        public override Task OrderDeleted(StreamOrder streamItem)
        {
            return _dataLayer.ProcessStreamItem(streamItem);
        }

        public override Task OrderUpdated(StreamOrder streamItem)
        {
            return _dataLayer.ProcessStreamItem(streamItem);
        }

        public override Task PortfolioAdded(StreamPortfolio streamItem)
        {
            return _dataLayer.ProcessStreamItem(streamItem);
        }

        public override Task PortfolioDeleted(StreamPortfolio streamItem)
        {
            return _dataLayer.ProcessStreamItem(streamItem);
        }

        public override Task PortfolioUpdated(StreamPortfolio streamItem)
        {
            return _dataLayer.ProcessStreamItem(streamItem);
        }

        public override Task PositionAdded(StreamPosition streamItem)
        {
            return _dataLayer.ProcessStreamItem(streamItem);
        }

        public override Task PositionDeleted(StreamPosition streamItem)
        {
            return _dataLayer.ProcessStreamItem(streamItem);
        }

        public override Task PositionUpdated(StreamPosition streamItem)
        {
            return _dataLayer.ProcessStreamItem(streamItem);
        }

        public override Task PriceAdded(StreamPrice streamItem)
        {
            return _dataLayer.ProcessStreamItem(streamItem);
        }

        public override Task PriceDeleted(StreamPrice streamItem)
        {
            return _dataLayer.ProcessStreamItem(streamItem);
        }

        public override Task PriceUpdated(StreamPrice streamItem)
        {
            return _dataLayer.ProcessStreamItem(streamItem);
        }

        public override Task RuleAdded(StreamRule streamItem)
        {
            return _dataLayer.ProcessStreamItem(streamItem);
        }

        public override Task RuleDeleted(StreamRule streamItem)
        {
            return _dataLayer.ProcessStreamItem(streamItem);
        }

        public override Task RuleUpdated(StreamRule streamItem)
        {
            return _dataLayer.ProcessStreamItem(streamItem);
        }

        public override Task SubmarketAdded(StreamSubmarket streamItem)
        {
            return _dataLayer.ProcessStreamItem(streamItem);
        }

        public override Task SubmarketDeleted(StreamSubmarket streamItem)
        {
            return _dataLayer.ProcessStreamItem(streamItem);
        }

        public override Task SubmarketUpdated(StreamSubmarket streamItem)
        {
            return _dataLayer.ProcessStreamItem(streamItem);
        }

        public override Task TradeAdded(StreamTrade streamItem)
        {
            return _dataLayer.ProcessStreamItem(streamItem);
        }

        public override Task TradeDeleted(StreamTrade streamItem)
        {
            return _dataLayer.ProcessStreamItem(streamItem);
        }

        public override Task TradeUpdated(StreamTrade streamItem)
        {
            return _dataLayer.ProcessStreamItem(streamItem);
        }
    }
}
