using RealtimePersister.Models.Streams;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace RealtimePersister
{
    public class PersistenceLayer
    {
        private IStreamPersister _persister;

        private StreamItemPersister<StreamMarket> _streamItemPersisterMarket;
        private StreamItemPersister<StreamSubmarket> _streamItemPersisterSubmarket;
        private StreamItemPersister<StreamInstrument> _streamItemPersisterInstrument;
        private StreamItemPersister<StreamPortfolio> _streamItemPersisterPortfolio;
        private StreamItemPersister<StreamPosition> _streamItemPersisterPosition;
        private StreamItemPersister<StreamOrder> _streamItemPersisterOrder;
        private StreamItemPersister<StreamRule> _streamItemPersisterRule;
        private StreamItemPersister<StreamPrice> _streamItemPersisterPrice;
        private StreamItemPersister<StreamTrade> _streamItemPersisterTrade;

        public async Task<bool> Initialize(IStreamPersister persister, CancellationToken cancellationToken)
        {
            _persister = persister;
            bool ret = false;
            if (_persister != null) 
                ret = await _persister.Connect();

            if (ret)
            {
                _streamItemPersisterMarket = new StreamItemPersisterQueue<StreamMarket>(StreamEntityType.Market);
                _streamItemPersisterSubmarket = new StreamItemPersisterQueue<StreamSubmarket>(StreamEntityType.Submarket);
                _streamItemPersisterInstrument = new StreamItemPersisterQueue<StreamInstrument>(StreamEntityType.Instrument);
                _streamItemPersisterPortfolio = new StreamItemPersisterQueue<StreamPortfolio>(StreamEntityType.Portfolio);
                _streamItemPersisterPosition = new StreamItemPersisterQueue<StreamPosition>(StreamEntityType.Position);
                _streamItemPersisterOrder = new StreamItemPersisterQueue<StreamOrder>(StreamEntityType.Order);
                _streamItemPersisterRule = new StreamItemPersisterQueue<StreamRule>(StreamEntityType.Rule);
                _streamItemPersisterPrice = new StreamItemPersisterQueue<StreamPrice>(StreamEntityType.Price);
                _streamItemPersisterTrade = new StreamItemPersisterQueue<StreamTrade>(StreamEntityType.Trade);
                Task.Run(async () =>
                        {
                            await ProcessPendingItems(cancellationToken);
                        });
            }

            return ret;
        }

        public Task ProcessStreamItem(StreamEntityBase streamItem)
        {
            switch (streamItem.EntityType)
            {
                case StreamEntityType.Market:
                    return (_streamItemPersisterMarket != null ? _streamItemPersisterMarket.ProcessStreamItem(streamItem as StreamMarket) : Task.CompletedTask);
                case StreamEntityType.Submarket:
                    return (_streamItemPersisterSubmarket != null ? _streamItemPersisterSubmarket.ProcessStreamItem(streamItem as StreamSubmarket) : Task.CompletedTask);
                case StreamEntityType.Instrument:
                    return (_streamItemPersisterInstrument != null ? _streamItemPersisterInstrument.ProcessStreamItem(streamItem as StreamInstrument) : Task.CompletedTask);
                case StreamEntityType.Portfolio:
                    return (_streamItemPersisterPortfolio != null ? _streamItemPersisterPortfolio.ProcessStreamItem(streamItem as StreamPortfolio) : Task.CompletedTask);
                case StreamEntityType.Position:
                    return (_streamItemPersisterPosition != null ? _streamItemPersisterPosition.ProcessStreamItem(streamItem as StreamPosition) : Task.CompletedTask);
                case StreamEntityType.Order:
                    return (_streamItemPersisterOrder != null ? _streamItemPersisterOrder.ProcessStreamItem(streamItem as StreamOrder) : Task.CompletedTask);
                case StreamEntityType.Rule:
                    return (_streamItemPersisterRule != null ? _streamItemPersisterRule.ProcessStreamItem(streamItem as StreamRule) : Task.CompletedTask);
                case StreamEntityType.Price:
                    return (_streamItemPersisterPrice != null ? _streamItemPersisterPrice.ProcessStreamItem(streamItem as StreamPrice) : Task.CompletedTask);
                case StreamEntityType.Trade:
                    return (_streamItemPersisterTrade != null ? _streamItemPersisterTrade.ProcessStreamItem(streamItem as StreamTrade) : Task.CompletedTask);
                default:
                    return Task.CompletedTask;
            }
        }

        #region Get functions
        public Task<IEnumerable<StreamMarket>> GetAllMarkets()
        {
            return (_streamItemPersisterMarket != null ? _streamItemPersisterMarket.GetAll(_persister, StreamEntityType.Market) : Task.FromResult<IEnumerable<StreamMarket>>(null));
        }

        public Task<StreamMarket> GetMarketById(string id)
        {
            return (_streamItemPersisterMarket != null ? _streamItemPersisterMarket.GetById(_persister, StreamEntityType.Market, id) : Task.FromResult<StreamMarket>(null));
        }

        public Task<IEnumerable<StreamMarket>> GetMarketsFromSequenceNumber(UInt64 sequenceNumberStart = UInt64.MinValue,
                        UInt64 sequenceNumberEnd = UInt64.MaxValue)
        {
            return (_streamItemPersisterMarket != null ? _streamItemPersisterMarket.GetFromSequenceNumber(_persister, StreamEntityType.Market,
                sequenceNumberStart, sequenceNumberEnd) : Task.FromResult<IEnumerable<StreamMarket>>(null));
        }

        public Task<IEnumerable<StreamSubmarket>> GetAllSubmarkets()
        {
            return (_streamItemPersisterSubmarket != null ? _streamItemPersisterSubmarket.GetAll(_persister, StreamEntityType.Submarket) : Task.FromResult<IEnumerable<StreamSubmarket>>(null));
        }

        public Task<StreamSubmarket> GetSubmarketById(string id)
        {
            return (_streamItemPersisterSubmarket != null ? _streamItemPersisterSubmarket.GetById(_persister, StreamEntityType.Submarket, id) : Task.FromResult<StreamSubmarket>(null));
        }

        public Task<IEnumerable<StreamSubmarket>> GetSubmarketsFromSequenceNumber(UInt64 sequenceNumberStart = UInt64.MinValue,
                        UInt64 sequenceNumberEnd = UInt64.MaxValue)
        {
            return (_streamItemPersisterSubmarket != null ? _streamItemPersisterSubmarket.GetFromSequenceNumber(_persister, StreamEntityType.Submarket,
                sequenceNumberStart, sequenceNumberEnd) : Task.FromResult<IEnumerable<StreamSubmarket>>(null));
        }

        public Task<IEnumerable<StreamInstrument>> GetAllInstruments()
        {
            return (_streamItemPersisterInstrument != null ? _streamItemPersisterInstrument.GetAll(_persister, StreamEntityType.Instrument) : Task.FromResult<IEnumerable<StreamInstrument>>(null));
        }

        public Task<StreamInstrument> GetInstrumentById(string id)
        {
            return (_streamItemPersisterInstrument != null ? _streamItemPersisterInstrument.GetById(_persister, StreamEntityType.Instrument, id) : Task.FromResult<StreamInstrument>(null));
        }

        public Task<IEnumerable<StreamInstrument>> GetInstrumentsFromSequenceNumber(UInt64 sequenceNumberStart = UInt64.MinValue,
                        UInt64 sequenceNumberEnd = UInt64.MaxValue)
        {
            return (_streamItemPersisterInstrument != null ? _streamItemPersisterInstrument.GetFromSequenceNumber(_persister, StreamEntityType.Instrument,
                sequenceNumberStart, sequenceNumberEnd) : Task.FromResult<IEnumerable<StreamInstrument>>(null));
        }

        public Task<IEnumerable<StreamPortfolio>> GetAllPortfolios()
        {
            return (_streamItemPersisterPortfolio != null ? _streamItemPersisterPortfolio.GetAll(_persister, StreamEntityType.Portfolio) : Task.FromResult<IEnumerable<StreamPortfolio>>(null));
        }

        public Task<StreamPortfolio> GetPortfolioById(string id)
        {
            return (_streamItemPersisterPortfolio != null ? _streamItemPersisterPortfolio.GetById(_persister, StreamEntityType.Portfolio, id) : Task.FromResult<StreamPortfolio>(null));
        }

        public Task<IEnumerable<StreamPortfolio>> GetPortfoliosFromSequenceNumber(UInt64 sequenceNumberStart = UInt64.MinValue,
                        UInt64 sequenceNumberEnd = UInt64.MaxValue)
        {
            return (_streamItemPersisterPortfolio != null ? _streamItemPersisterPortfolio.GetFromSequenceNumber(_persister, StreamEntityType.Portfolio,
                sequenceNumberStart, sequenceNumberEnd) : Task.FromResult<IEnumerable<StreamPortfolio>>(null));
        }

        public Task<IEnumerable<StreamPosition>> GetAllPositions()
        {
            return (_streamItemPersisterPosition != null ? _streamItemPersisterPosition.GetAll(_persister, StreamEntityType.Position) : Task.FromResult<IEnumerable<StreamPosition>>(null));
        }

        public Task<StreamPosition> GetPositionById(string id)
        {
            return (_streamItemPersisterPosition != null ? _streamItemPersisterPosition.GetById(_persister, StreamEntityType.Position, id) : Task.FromResult<StreamPosition>(null));
        }

        public Task<IEnumerable<StreamPosition>> GetPositionsFromSequenceNumber(UInt64 sequenceNumberStart = UInt64.MinValue,
                        UInt64 sequenceNumberEnd = UInt64.MaxValue)
        {
            return (_streamItemPersisterPosition != null ? _streamItemPersisterPosition.GetFromSequenceNumber(_persister, StreamEntityType.Position,
                sequenceNumberStart, sequenceNumberEnd) : Task.FromResult<IEnumerable<StreamPosition>>(null));
        }

        public Task<IEnumerable<StreamOrder>> GetAllOrders()
        {
            return (_streamItemPersisterOrder != null ? _streamItemPersisterOrder.GetAll(_persister, StreamEntityType.Order) : Task.FromResult<IEnumerable<StreamOrder>>(null));
        }

        public Task<StreamOrder> GetOrderById(string id)
        {
            return (_streamItemPersisterOrder != null ? _streamItemPersisterOrder.GetById(_persister, StreamEntityType.Order, id) : Task.FromResult<StreamOrder>(null));
        }

        public Task<IEnumerable<StreamOrder>> GetOrdersFromSequenceNumber(UInt64 sequenceNumberStart = UInt64.MinValue,
                        UInt64 sequenceNumberEnd = UInt64.MaxValue)
        {
            return (_streamItemPersisterOrder != null ?_streamItemPersisterOrder.GetFromSequenceNumber(_persister, StreamEntityType.Order,
                sequenceNumberStart, sequenceNumberEnd) : Task.FromResult<IEnumerable<StreamOrder>>(null));
        }

        public Task<IEnumerable<StreamRule>> GetAllRules()
        {
            return (_streamItemPersisterRule != null ? _streamItemPersisterRule.GetAll(_persister, StreamEntityType.Rule) : Task.FromResult<IEnumerable<StreamRule>>(null));
        }

        public Task<StreamRule> GetRuleById(string id)
        {
            return (_streamItemPersisterRule != null ? _streamItemPersisterRule.GetById(_persister, StreamEntityType.Rule, id) : Task.FromResult<StreamRule>(null));
        }

        public Task<IEnumerable<StreamRule>> GetRulesFromSequenceNumber(UInt64 sequenceNumberStart = UInt64.MinValue,
                        UInt64 sequenceNumberEnd = UInt64.MaxValue)
        {
            return (_streamItemPersisterRule != null ? _streamItemPersisterRule.GetFromSequenceNumber(_persister, StreamEntityType.Rule,
                sequenceNumberStart, sequenceNumberEnd) : Task.FromResult<IEnumerable<StreamRule>>(null));
        }

        public Task<IEnumerable<StreamPrice>> GetAllPrices()
        {
            return (_streamItemPersisterPrice != null ? _streamItemPersisterPrice.GetAll(_persister, StreamEntityType.Price) : Task.FromResult<IEnumerable<StreamPrice>>(null));
        }

        public Task<StreamPrice> GetPriceById(string id)
        {
            return (_streamItemPersisterPrice != null ? _streamItemPersisterPrice.GetById(_persister, StreamEntityType.Price, id) : Task.FromResult<StreamPrice>(null));
        }

        public Task<IEnumerable<StreamPrice>> GetPricesFromSequenceNumber(UInt64 sequenceNumberStart = UInt64.MinValue,
                        UInt64 sequenceNumberEnd = UInt64.MaxValue)
        {
            return (_streamItemPersisterPrice != null ? _streamItemPersisterPrice.GetFromSequenceNumber(_persister, StreamEntityType.Price,
                sequenceNumberStart, sequenceNumberEnd) : Task.FromResult<IEnumerable<StreamPrice>>(null));
        }

        public Task<IEnumerable<StreamTrade>> GetAllTrades()
        {
            return (_streamItemPersisterTrade != null ? _streamItemPersisterTrade.GetAll(_persister, StreamEntityType.Trade) : Task.FromResult<IEnumerable<StreamTrade>>(null));
        }

        public Task<StreamTrade> GetTradeById(string id)
        {
            return (_streamItemPersisterTrade != null ? _streamItemPersisterTrade.GetById(_persister, StreamEntityType.Trade, id) : Task.FromResult<StreamTrade>(null));
        }

        public Task<IEnumerable<StreamTrade>> GetTradesFromSequenceNumber(UInt64 sequenceNumberStart = UInt64.MinValue,
                        UInt64 sequenceNumberEnd = UInt64.MaxValue)
        {
            return (_streamItemPersisterTrade != null ? _streamItemPersisterTrade.GetFromSequenceNumber(_persister, StreamEntityType.Trade,
                sequenceNumberStart, sequenceNumberEnd) : Task.FromResult<IEnumerable<StreamTrade>>(null));
        }
        #endregion

        private async Task ProcessPendingItems(CancellationToken cancellationToken, int maxItems = 50)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                bool anyItems = false;
                Task[] tasks = new Task[9];
                int numTasks = 0;
                if (_streamItemPersisterMarket != null)
                    tasks[numTasks++] = _streamItemPersisterMarket.ProcessPendingItems(_persister, cancellationToken, maxItems);
                if (_streamItemPersisterSubmarket != null)
                    tasks[numTasks++] = _streamItemPersisterSubmarket.ProcessPendingItems(_persister, cancellationToken, maxItems);
                if (_streamItemPersisterInstrument != null)
                    tasks[numTasks++] = _streamItemPersisterInstrument.ProcessPendingItems(_persister, cancellationToken, maxItems);
                if (_streamItemPersisterPortfolio != null)
                    tasks[numTasks++] = _streamItemPersisterPortfolio.ProcessPendingItems(_persister, cancellationToken, maxItems);
                if (_streamItemPersisterPosition != null)
                    tasks[numTasks++] = _streamItemPersisterPosition.ProcessPendingItems(_persister, cancellationToken, maxItems);
                if (_streamItemPersisterOrder != null)
                    tasks[numTasks++] = _streamItemPersisterOrder.ProcessPendingItems(_persister, cancellationToken, maxItems);
                if (_streamItemPersisterRule != null)
                    tasks[numTasks++] = _streamItemPersisterRule.ProcessPendingItems(_persister, cancellationToken, maxItems);
                if (_streamItemPersisterPrice != null)
                    tasks[numTasks++] = _streamItemPersisterPrice.ProcessPendingItems(_persister, cancellationToken, maxItems);
                if (_streamItemPersisterTrade != null)
                    tasks[numTasks++] = _streamItemPersisterTrade.ProcessPendingItems(_persister, cancellationToken, maxItems);

                await Task.WhenAll(tasks);
                // TODO: should we set anyItems here?
                if (!anyItems)
                    await Task.Delay(/*1*/60000);
            }
        }
    }
}
