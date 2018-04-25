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
        private StreamEntityPersister[] _streamEntityPersisters = new StreamEntityPersister[(int)StreamEntityType.Max];

        public async Task<bool> Initialize(IStreamPersister persister, CancellationToken cancellationToken)
        {
            _persister = persister;
            bool ret = false;
            if (_persister != null)
                ret = await _persister.Connect();

            bool persistSynchronously = false;

            _streamEntityPersisters[(int)StreamEntityType.Market] = new StreamEntityPersister(StreamEntityType.Market, persister, cancellationToken, 1, persistSynchronously, 50, 100);
            _streamEntityPersisters[(int)StreamEntityType.Submarket] = new StreamEntityPersister(StreamEntityType.Submarket, persister, cancellationToken, 1, persistSynchronously, 50, 100);
            _streamEntityPersisters[(int)StreamEntityType.Instrument] = new StreamEntityPersister(StreamEntityType.Instrument, persister, cancellationToken, 1, persistSynchronously, 50, 100);
            _streamEntityPersisters[(int)StreamEntityType.Portfolio] = new StreamEntityPersister(StreamEntityType.Portfolio, persister, cancellationToken, 1, persistSynchronously, 50, 100);
            _streamEntityPersisters[(int)StreamEntityType.Position] = new StreamEntityPersister(StreamEntityType.Position, persister, cancellationToken, 1, persistSynchronously, 50, 100);
            _streamEntityPersisters[(int)StreamEntityType.Order] = new StreamEntityPersister(StreamEntityType.Order, persister, cancellationToken, 1, persistSynchronously, 50, 100);
            _streamEntityPersisters[(int)StreamEntityType.Rule] = new StreamEntityPersister(StreamEntityType.Rule, persister, cancellationToken, 1, persistSynchronously, 50, 100);
            _streamEntityPersisters[(int)StreamEntityType.Price] = new StreamEntityPersister(StreamEntityType.Price, persister, cancellationToken, 100, persistSynchronously, 50, 100);
            _streamEntityPersisters[(int)StreamEntityType.Trade] = new StreamEntityPersister(StreamEntityType.Trade, persister, cancellationToken, 1, persistSynchronously, 50, 100);

            return ret;
        }

        public Task ProcessStreamItem(StreamEntityBase streamItem)
        {
            return (_streamEntityPersisters[(int)streamItem.EntityType] != null ?
                _streamEntityPersisters[(int)streamItem.EntityType].ProcessStreamItem(streamItem) :
                Task.CompletedTask);
        }

        #region Get functions
        public Task<IEnumerable<StreamMarket>> GetAllMarkets()
        {
            return (_persister != null ? _persister.GetAll<StreamMarket>(StreamEntityType.Market) : Task.FromResult<IEnumerable<StreamMarket>>(null));
        }

        public Task<StreamMarket> GetMarketById(string id)
        {
            return (_persister != null ? _persister.GetById<StreamMarket>(StreamEntityType.Market, id) : Task.FromResult<StreamMarket>(null));
        }

        public Task<IEnumerable<StreamMarket>> GetMarketsFromSequenceNumber(UInt64 sequenceNumberStart = UInt64.MinValue,
                        UInt64 sequenceNumberEnd = UInt64.MaxValue)
        {
            return (_persister != null ? _persister.GetFromSequenceNumber<StreamMarket>(StreamEntityType.Market,
                sequenceNumberStart, sequenceNumberEnd) : Task.FromResult<IEnumerable<StreamMarket>>(null));
        }

        public Task<IEnumerable<StreamSubmarket>> GetAllSubmarkets()
        {
            return (_persister != null ? _persister.GetAll<StreamSubmarket>(StreamEntityType.Submarket) : Task.FromResult<IEnumerable<StreamSubmarket>>(null));
        }

        public Task<StreamSubmarket> GetSubmarketById(string id)
        {
            return (_persister != null ? _persister.GetById<StreamSubmarket>(StreamEntityType.Submarket, id) : Task.FromResult<StreamSubmarket>(null));
        }

        public Task<IEnumerable<StreamSubmarket>> GetSubmarketsFromSequenceNumber(UInt64 sequenceNumberStart = UInt64.MinValue,
                        UInt64 sequenceNumberEnd = UInt64.MaxValue)
        {
            return (_persister != null ? _persister.GetFromSequenceNumber<StreamSubmarket>(StreamEntityType.Submarket,
                sequenceNumberStart, sequenceNumberEnd) : Task.FromResult<IEnumerable<StreamSubmarket>>(null));
        }

        public Task<IEnumerable<StreamInstrument>> GetAllInstruments()
        {
            return (_persister != null ? _persister.GetAll<StreamInstrument>(StreamEntityType.Instrument) : Task.FromResult<IEnumerable<StreamInstrument>>(null));
        }

        public Task<StreamInstrument> GetInstrumentById(string id)
        {
            return (_persister != null ? _persister.GetById<StreamInstrument>(StreamEntityType.Instrument, id) : Task.FromResult<StreamInstrument>(null));
        }

        public Task<IEnumerable<StreamInstrument>> GetInstrumentsFromSequenceNumber(UInt64 sequenceNumberStart = UInt64.MinValue,
                        UInt64 sequenceNumberEnd = UInt64.MaxValue)
        {
            return (_persister != null ? _persister.GetFromSequenceNumber<StreamInstrument>(StreamEntityType.Instrument,
                sequenceNumberStart, sequenceNumberEnd) : Task.FromResult<IEnumerable<StreamInstrument>>(null));
        }

        public Task<IEnumerable<StreamPortfolio>> GetAllPortfolios()
        {
            return (_persister != null ? _persister.GetAll<StreamPortfolio>(StreamEntityType.Portfolio) : Task.FromResult<IEnumerable<StreamPortfolio>>(null));
        }

        public Task<StreamPortfolio> GetPortfolioById(string id)
        {
            return (_persister != null ? _persister.GetById<StreamPortfolio>(StreamEntityType.Portfolio, id) : Task.FromResult<StreamPortfolio>(null));
        }

        public Task<IEnumerable<StreamPortfolio>> GetPortfoliosFromSequenceNumber(UInt64 sequenceNumberStart = UInt64.MinValue,
                        UInt64 sequenceNumberEnd = UInt64.MaxValue)
        {
            return (_persister != null ? _persister.GetFromSequenceNumber<StreamPortfolio>(StreamEntityType.Portfolio,
                sequenceNumberStart, sequenceNumberEnd) : Task.FromResult<IEnumerable<StreamPortfolio>>(null));
        }

        public Task<IEnumerable<StreamPosition>> GetAllPositions()
        {
            return (_persister != null ? _persister.GetAll<StreamPosition>(StreamEntityType.Position) : Task.FromResult<IEnumerable<StreamPosition>>(null));
        }

        public Task<StreamPosition> GetPositionById(string id)
        {
            return (_persister != null ? _persister.GetById<StreamPosition>(StreamEntityType.Position, id) : Task.FromResult<StreamPosition>(null));
        }

        public Task<IEnumerable<StreamPosition>> GetPositionsFromSequenceNumber(UInt64 sequenceNumberStart = UInt64.MinValue,
                        UInt64 sequenceNumberEnd = UInt64.MaxValue)
        {
            return (_persister != null ? _persister.GetFromSequenceNumber<StreamPosition>(StreamEntityType.Position,
                sequenceNumberStart, sequenceNumberEnd) : Task.FromResult<IEnumerable<StreamPosition>>(null));
        }

        public Task<IEnumerable<StreamOrder>> GetAllOrders()
        {
            return (_persister != null ? _persister.GetAll<StreamOrder>(StreamEntityType.Order) : Task.FromResult<IEnumerable<StreamOrder>>(null));
        }

        public Task<StreamOrder> GetOrderById(string id)
        {
            return (_persister != null ? _persister.GetById<StreamOrder>(StreamEntityType.Order, id) : Task.FromResult<StreamOrder>(null));
        }

        public Task<IEnumerable<StreamOrder>> GetOrdersFromSequenceNumber(UInt64 sequenceNumberStart = UInt64.MinValue,
                        UInt64 sequenceNumberEnd = UInt64.MaxValue)
        {
            return (_persister != null ? _persister.GetFromSequenceNumber<StreamOrder>(StreamEntityType.Order,
                sequenceNumberStart, sequenceNumberEnd) : Task.FromResult<IEnumerable<StreamOrder>>(null));
        }

        public Task<IEnumerable<StreamRule>> GetAllRules()
        {
            return (_persister != null ? _persister.GetAll<StreamRule>(StreamEntityType.Rule) : Task.FromResult<IEnumerable<StreamRule>>(null));
        }

        public Task<StreamRule> GetRuleById(string id)
        {
            return (_persister != null ? _persister.GetById<StreamRule>(StreamEntityType.Rule, id) : Task.FromResult<StreamRule>(null));
        }

        public Task<IEnumerable<StreamRule>> GetRulesFromSequenceNumber(UInt64 sequenceNumberStart = UInt64.MinValue,
                        UInt64 sequenceNumberEnd = UInt64.MaxValue)
        {
            return (_persister != null ? _persister.GetFromSequenceNumber<StreamRule>(StreamEntityType.Rule,
                sequenceNumberStart, sequenceNumberEnd) : Task.FromResult<IEnumerable<StreamRule>>(null));
        }

        public Task<IEnumerable<StreamPrice>> GetAllPrices()
        {
            return (_persister != null ? _persister.GetAll<StreamPrice>(StreamEntityType.Price) : Task.FromResult<IEnumerable<StreamPrice>>(null));
        }

        public Task<StreamPrice> GetPriceById(string id)
        {
            return (_persister != null ? _persister.GetById<StreamPrice>(StreamEntityType.Price, id) : Task.FromResult<StreamPrice>(null));
        }

        public Task<IEnumerable<StreamPrice>> GetPricesFromSequenceNumber(UInt64 sequenceNumberStart = UInt64.MinValue,
                        UInt64 sequenceNumberEnd = UInt64.MaxValue)
        {
            return (_persister != null ? _persister.GetFromSequenceNumber<StreamPrice>(StreamEntityType.Price,
                sequenceNumberStart, sequenceNumberEnd) : Task.FromResult<IEnumerable<StreamPrice>>(null));
        }

        public Task<IEnumerable<StreamTrade>> GetAllTrades()
        {
            return (_persister != null ? _persister.GetAll<StreamTrade>(StreamEntityType.Trade) : Task.FromResult<IEnumerable<StreamTrade>>(null));
        }

        public Task<StreamTrade> GetTradeById(string id)
        {
            return (_persister != null ? _persister.GetById<StreamTrade>(StreamEntityType.Trade, id) : Task.FromResult<StreamTrade>(null));
        }

        public Task<IEnumerable<StreamTrade>> GetTradesFromSequenceNumber(UInt64 sequenceNumberStart = UInt64.MinValue,
                        UInt64 sequenceNumberEnd = UInt64.MaxValue)
        {
            return (_persister != null ? _persister.GetFromSequenceNumber<StreamTrade>(StreamEntityType.Trade,
                sequenceNumberStart, sequenceNumberEnd) : Task.FromResult<IEnumerable<StreamTrade>>(null));
        }
#endregion
    }
}
