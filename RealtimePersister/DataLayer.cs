using RealtimePersister.Models.Streams;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace RealtimePersister
{
    public class DataLayer
    {
        private ConcurrentDictionary<string, StreamMarket> _markets = new ConcurrentDictionary<string, StreamMarket>();
        private ConcurrentDictionary<string, StreamSubmarket> _submarkets = new ConcurrentDictionary<string, StreamSubmarket>();
        private ConcurrentDictionary<string, StreamInstrument> _instruments = new ConcurrentDictionary<string, StreamInstrument>();
        private ConcurrentDictionary<string, StreamPortfolio> _portfolios = new ConcurrentDictionary<string, StreamPortfolio>();
        private ConcurrentDictionary<string, StreamPosition> _positions = new ConcurrentDictionary<string, StreamPosition>();
        private ConcurrentDictionary<string, StreamOrder> _orders = new ConcurrentDictionary<string, StreamOrder>();
        private ConcurrentDictionary<string, StreamRule> _rules = new ConcurrentDictionary<string, StreamRule>();
        private ConcurrentDictionary<string, StreamPrice> _prices = new ConcurrentDictionary<string, StreamPrice>();
        private ConcurrentDictionary<string, StreamTrade> _trades = new ConcurrentDictionary<string, StreamTrade>();

        private PersistenceLayer _persistenceLayer = null;

        public async Task Initialize(CancellationToken cancellationToken, PersistenceLayer persistenceLayer)
        {
            _persistenceLayer = persistenceLayer;

            if (persistenceLayer != null)
            {
                var markets = await persistenceLayer.GetAllMarkets();
                if (markets != null)
                    foreach (var market in markets)
                    {
                        _markets[market.Id] = market;
                    }

                var submarkets = await persistenceLayer.GetAllSubmarkets();
                if (submarkets != null)
                    foreach (var submarket in submarkets)
                    {
                        _submarkets[submarket.Id] = submarket;
                    }

                var instruments = await persistenceLayer.GetAllInstruments();
                if (instruments != null)
                    foreach (var instrument in instruments)
                    {
                        _instruments[instrument.Id] = instrument;
                    }

                var portfolios = await persistenceLayer.GetAllPortfolios();
                if (portfolios != null)
                    foreach (var portfolio in portfolios)
                    {
                        _portfolios[portfolio.Id] = portfolio;
                    }

                var positions = await persistenceLayer.GetAllPositions();
                if (positions != null)
                    foreach (var position in positions)
                    {
                        _positions[position.Id] = position;
                    }

                var orders = await persistenceLayer.GetAllOrders();
                if (orders != null)
                    foreach (var order in orders)
                    {
                        _orders[order.Id] = order;
                    }

                var rules = await persistenceLayer.GetAllRules();
                if (rules != null)
                    foreach (var rule in rules)
                    {
                        _rules[rule.Id] = rule;
                    }

                var prices = await persistenceLayer.GetAllPrices();
                if (prices != null)
                    foreach (var price in prices)
                    {
                        _prices[price.Id] = price;
                    }

                var trades = await persistenceLayer.GetAllTrades();
                if (trades != null)
                    foreach (var trade in trades)
                    {
                        _trades[trade.Id] = trade;
                    }
            }
        }

        public Task ProcessStreamItem(StreamEntityBase streamItem)
        {
            switch (streamItem.EntityType)
            {
                case StreamEntityType.Command:
                    return ProcessStreamItem(streamItem as StreamCommand);
                case StreamEntityType.Market:
                    return ProcessStreamItem(streamItem as StreamMarket);
                case StreamEntityType.Submarket:
                    return ProcessStreamItem(streamItem as StreamSubmarket);
                case StreamEntityType.Instrument:
                    return ProcessStreamItem(streamItem as StreamInstrument);
                case StreamEntityType.Portfolio:
                    return ProcessStreamItem(streamItem as StreamPortfolio);
                case StreamEntityType.Position:
                    return ProcessStreamItem(streamItem as StreamPosition);
                case StreamEntityType.Order:
                    return ProcessStreamItem(streamItem as StreamOrder);
                case StreamEntityType.Rule:
                    return ProcessStreamItem(streamItem as StreamRule);
                case StreamEntityType.Price:
                    return ProcessStreamItem(streamItem as StreamPrice);
                case StreamEntityType.Trade:
                    return ProcessStreamItem(streamItem as StreamTrade);
                default:
                    return Task.CompletedTask;
            }
        }

        public Task ProcessStreamItem(StreamCommand command)
        {
            switch (command.Command)
            {
                case StreamCommandType.StartSnapshot:
                    // TODO
                    break;
                case StreamCommandType.EndSnapshot:
                    // TODO
                    break;
                case StreamCommandType.Clear:
                    // TODO
                    break;
            }
            return Task.CompletedTask;
        }

        public async Task ProcessStreamItem(StreamMarket market)
        {
            StreamMarket currentMarket = null;
            bool found = _markets.TryGetValue(market.Id, out currentMarket);

            switch (market.Operation)
            {
                case StreamOperation.Insert:
                case StreamOperation.Update:
                    if (!found || currentMarket.Compare(market) != 0)
                    {
                        if (_persistenceLayer != null)
                            await _persistenceLayer.ProcessStreamItem(market);
                    }
                    _markets[market.Id] = market;
                    break;

                case StreamOperation.Delete:
                    if (found)
                    {
                        if (_persistenceLayer != null)
                            await _persistenceLayer.ProcessStreamItem(market);
                        _markets.TryRemove(market.Id, out currentMarket);
                    }
                    break;

                default:
                    break;
            }
        }

        public async Task ProcessStreamItem(StreamSubmarket submarket)
        {
            StreamSubmarket currentSubmarket = null;
            bool found = _submarkets.TryGetValue(submarket.Id, out currentSubmarket);

            switch (submarket.Operation)
            {
                case StreamOperation.Insert:
                case StreamOperation.Update:
                    if (!found || currentSubmarket.Compare(submarket) != 0)
                    {
                        if (_persistenceLayer != null)
                            await _persistenceLayer.ProcessStreamItem(submarket);
                    }
                    _submarkets[submarket.Id] = submarket;
                    break;

                case StreamOperation.Delete:
                    if (found)
                    {
                        if (_persistenceLayer != null)
                            await _persistenceLayer.ProcessStreamItem(submarket);
                        _submarkets.TryRemove(submarket.Id, out currentSubmarket);
                    }
                    break;

                default:
                    break;
            }
        }

        public async Task ProcessStreamItem(StreamInstrument instrument)
        {
            StreamInstrument currentInstrument = null;
            bool found = _instruments.TryGetValue(instrument.Id, out currentInstrument);

            switch (instrument.Operation)
            {
                case StreamOperation.Insert:
                case StreamOperation.Update:
                    if (!found || currentInstrument.Compare(instrument) != 0)
                    {
                        if (_persistenceLayer != null)
                            await _persistenceLayer.ProcessStreamItem(instrument);
                    }
                    _instruments[instrument.Id] = instrument;
                    break;

                case StreamOperation.Delete:
                    if (found)
                    {
                        if (_persistenceLayer != null)
                            await _persistenceLayer.ProcessStreamItem(instrument);
                        _instruments.TryRemove(instrument.Id, out currentInstrument);
                    }
                    break;

                default:
                    break;
            }
        }

        public async Task ProcessStreamItem(StreamPortfolio portfolio)
        {
            StreamPortfolio currentPortfolio = null;
            bool found = _portfolios.TryGetValue(portfolio.Id, out currentPortfolio);

            switch (portfolio.Operation)
            {
                case StreamOperation.Insert:
                case StreamOperation.Update:
                    if (!found || currentPortfolio.Compare(portfolio) != 0)
                    {
                        if (_persistenceLayer != null)
                            await _persistenceLayer.ProcessStreamItem(portfolio);
                    }
                    _portfolios[portfolio.Id] = portfolio;
                    break;

                case StreamOperation.Delete:
                    if (found)
                    {
                        if (_persistenceLayer != null)
                            await _persistenceLayer.ProcessStreamItem(portfolio);
                        _portfolios.TryRemove(portfolio.Id, out currentPortfolio);
                    }
                    break;

                default:
                    break;
            }
        }

        public async Task ProcessStreamItem(StreamPosition position)
        {
            StreamPosition currentPosition = null;
            bool found = _positions.TryGetValue(position.Id, out currentPosition);

            switch (position.Operation)
            {
                case StreamOperation.Insert:
                case StreamOperation.Update:
                    if (!found || currentPosition.Compare(position) != 0)
                    {
                        if (_persistenceLayer != null)
                            await _persistenceLayer.ProcessStreamItem(position);
                    }
                    _positions[position.Id] = position;
                    break;

                case StreamOperation.Delete:
                    if (found)
                    {
                        if (_persistenceLayer != null)
                            await _persistenceLayer.ProcessStreamItem(position);
                        _positions.TryRemove(position.Id, out currentPosition);
                    }
                    break;

                default:
                    break;
            }
        }

        public async Task ProcessStreamItem(StreamOrder order)
        {
            StreamOrder currentOrder = null;
            bool found = _orders.TryGetValue(order.Id, out currentOrder);

            switch (order.Operation)
            {
                case StreamOperation.Insert:
                case StreamOperation.Update:
                    if (!found || currentOrder.Compare(order) != 0)
                    {
                        if (_persistenceLayer != null)
                            await _persistenceLayer.ProcessStreamItem(order);
                    }
                    _orders[order.Id] = order;
                    break;

                case StreamOperation.Delete:
                    if (found)
                    {
                        if (_persistenceLayer != null)
                            await _persistenceLayer.ProcessStreamItem(order);
                        _orders.TryRemove(order.Id, out currentOrder);
                    }
                    break;

                default:
                    break;
            }
        }

        public async Task ProcessStreamItem(StreamRule rule)
        {
            StreamRule currentRule = null;
            bool found = _rules.TryGetValue(rule.Id, out currentRule);

            switch (rule.Operation)
            {
                case StreamOperation.Insert:
                case StreamOperation.Update:
                    if (!found || currentRule.Compare(rule) != 0)
                    {
                        if (_persistenceLayer != null)
                            await _persistenceLayer.ProcessStreamItem(rule);
                    }
                    _rules[rule.Id] = rule;
                    break;

                case StreamOperation.Delete:
                    if (found)
                    {
                        if (_persistenceLayer != null)
                            await _persistenceLayer.ProcessStreamItem(rule);
                        _rules.TryRemove(rule.Id, out currentRule);
                    }
                    break;

                default:
                    break;
            }
        }

        public async Task ProcessStreamItem(StreamPrice price)
        {
            StreamPrice currentPrice = null;
            bool found = _prices.TryGetValue(price.Id, out currentPrice);
            switch (price.Operation)
            {
                case StreamOperation.Insert:
                case StreamOperation.Update:
                    if (!found || currentPrice.Compare(price) != 0)
                    {
                        if (_persistenceLayer != null)
                            await _persistenceLayer.ProcessStreamItem(price);
                    }
                    _prices[price.Id] = price;
                    break;

                case StreamOperation.Delete:
                    if (found)
                    {
                        if (_persistenceLayer != null)
                            await _persistenceLayer.ProcessStreamItem(price);
                        _prices.TryRemove(price.Id, out currentPrice);
                    }
                    break;

                default:
                    break;
            }
        }

        public async Task ProcessStreamItem(StreamTrade trade)
        {
            StreamTrade currentTrade = null;
            bool found = _trades.TryGetValue(trade.Id, out currentTrade);

            switch (trade.Operation)
            {
                case StreamOperation.Insert:
                case StreamOperation.Update:
                    if (!found || currentTrade.Compare(trade) != 0)
                    {
                        if (_persistenceLayer != null)
                            await _persistenceLayer.ProcessStreamItem(trade);
                    }
                    _trades[trade.Id] = trade;
                    break;

                case StreamOperation.Delete:
                    if (found)
                    {
                        if (_persistenceLayer != null)
                            await _persistenceLayer.ProcessStreamItem(trade);
                        _trades.TryRemove(trade.Id, out currentTrade);
                    }
                    break;

                default:
                    break;
            }
        }
    }
}
