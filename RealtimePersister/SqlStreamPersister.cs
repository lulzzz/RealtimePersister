using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RealtimePersister.Models.Streams;

namespace RealtimePersister
{
    public class SqlStreamPersisterFactory : IStreamPersisterFactory
    {
        public IStreamPersister CreatePersister(string database)
        {
            return new SqlStreamPersister(database);
        }
    }

    public class SqlStreamPersister : IStreamPersister
    {
        string connectionString = "Server=andersth-sql-wus2.database.windows.net;Database=StreamEntityPersist;Integrated Security=False;User Id=andersth;Password=P@ssword.666";
        //string connectionString = "Server=10.0.30.179;Database=StreamEntityPersist;Integrated Security=True";
        string database;

        public SqlStreamPersister(string database)
        {
            this.database = database;
        }

        public bool SupportsBatches => false;

        public async Task<bool> Connect()
        {
            return true;
        }

        public async Task<IStreamPersisterBatch> CreateBatch(StreamEntityType type)
        {
            return null;
        }

        public Task Delete(StreamEntityBase item, IStreamPersisterBatch tx = null)
        {
            return Task.CompletedTask;
        }

        public Task Disconnect()
        {
            return Task.CompletedTask;
        }

        public async Task<IEnumerable<T>> GetAll<T>(StreamEntityType entityType) where T : StreamEntityBase
        {
            return null;
        }

        public async Task<T> GetById<T>(StreamEntityType entityType, string id) where T : StreamEntityBase
        {
            return null;
        }

        public async Task<IEnumerable<T>> GetFromSequenceNumber<T>(StreamEntityType entityType, ulong sequenceNumberStart = 0, ulong sequenceNumberEnd = ulong.MaxValue) where T : StreamEntityBase
        {
            return null;
        }

        public async Task Upsert(StreamEntityBase item, IStreamPersisterBatch tx = null)
        {
            if (item.EntityType == StreamEntityType.Market) {
                if (item.Operation == StreamOperation.Insert) {
                    await InsertMarket(item as StreamMarket);
                }
            } else if (item.EntityType == StreamEntityType.Submarket) {
                if (item.Operation == StreamOperation.Insert) {
                    await InsertSubmarket(item as StreamSubmarket);
                }
            } else if (item.EntityType == StreamEntityType.Instrument) {
                if (item.Operation == StreamOperation.Insert) {
                    await InsertInstrument(item as StreamInstrument);
                }
            } else if (item.EntityType == StreamEntityType.Portfolio) {
                if (item.Operation == StreamOperation.Insert) {
                    await InsertPortfolio(item as StreamPortfolio);
                }
            } else if (item.EntityType == StreamEntityType.Position) {
                if (item.Operation == StreamOperation.Insert) {
                    await InsertPosition(item as StreamPosition);
                }
            } else if (item.EntityType == StreamEntityType.Rule) {
                if (item.Operation == StreamOperation.Insert) {
                    await InsertRule(item as StreamRule);
                }
            } else {
                //Debugger.Break();
            }
        }

        private async Task InsertRule(StreamRule rule)
        {
            var InsertCmd = new SqlCommand();
            await Insert(InsertCmd, () =>
            {
                var Id = int.Parse(rule.Id.Split('-')[1]);
                var PortfolioId = int.Parse(rule.PortfolioId.Split(':')[1]);
                InsertCmd.CommandText = "exec InsertRule @Id, @PortfolioId, @Expression, @Timestamp";
                InsertCmd.Parameters.Add("@Id", SqlDbType.Int).Value = PortfolioId * 1000000+Id;
                InsertCmd.Parameters.Add("@PortfolioId", SqlDbType.Int).Value = PortfolioId;
                InsertCmd.Parameters.Add("@Expression", SqlDbType.NVarChar).Value = rule.Expression;
                InsertCmd.Parameters.Add("@Timestamp", SqlDbType.DateTime).Value = rule.Date;
            });
        }

        private async Task InsertPosition(StreamPosition position)
        {
            var InsertCmd = new SqlCommand();
            await Insert(InsertCmd, () =>
            {
                var Id = int.Parse(position.Id.Split('-')[1]);
                var InstrumentId = int.Parse(position.InstrumentId.Split('-')[2]);
                var PortfolioId = int.Parse(position.PortfolioId.Split(':')[1]);
                InsertCmd.CommandText = "exec InsertPosition @Id, @InstrumentId, @PortfolioId, @Volume, @Price, @Timestamp";
                InsertCmd.Parameters.Add("@Id", SqlDbType.Int).Value = PortfolioId*1000000+Id;
                InsertCmd.Parameters.Add("@InstrumentId", SqlDbType.Int).Value = InstrumentId;
                InsertCmd.Parameters.Add("@PortfolioId", SqlDbType.Int).Value = PortfolioId;
                InsertCmd.Parameters.Add("@Volume", SqlDbType.Int).Value = position.Volume;
                InsertCmd.Parameters.Add("@Price", SqlDbType.Int).Value = position.Price;
                InsertCmd.Parameters.Add("@Timestamp", SqlDbType.DateTime).Value = position.Date;
            });
        }

        private async Task InsertPortfolio(StreamPortfolio portfolio)
        {
            var InsertCmd = new SqlCommand();
            await Insert(InsertCmd, () =>
            {
                var Id = int.Parse(portfolio.Id.Split(':')[1]);
                InsertCmd.CommandText = "exec InsertPortfolio @Id, @Name, @Balance, @CheckRules, @Timestamp";
                InsertCmd.Parameters.Add("@Id", SqlDbType.Int).Value = Id;
                InsertCmd.Parameters.Add("@Name", SqlDbType.NVarChar).Value = portfolio.Name;
                InsertCmd.Parameters.Add("@Balance", SqlDbType.Float).Value = portfolio.Balance;
                InsertCmd.Parameters.Add("@CheckRules", SqlDbType.Bit).Value = portfolio.CheckRules;
                InsertCmd.Parameters.Add("@Timestamp", SqlDbType.DateTime).Value = portfolio.Date;
            });
        }

        private async Task InsertInstrument(StreamInstrument instrument)
        {
            var InsertCmd = new SqlCommand();
            await Insert(InsertCmd, () =>
            {
                var Id = int.Parse(instrument.Id.Split('-')[2]);
                var SubmarketId = int.Parse(instrument.SubmarketId.Split('-')[1]);
                InsertCmd.CommandText = "exec InsertInstrument @Id, @MarketId, @Name, @Timestamp";
                InsertCmd.Parameters.Add("@Id", SqlDbType.Int).Value = Id;
                InsertCmd.Parameters.Add("@MarketId", SqlDbType.Int).Value = SubmarketId;
                InsertCmd.Parameters.Add("@Name", SqlDbType.NVarChar).Value = instrument.Name;
                InsertCmd.Parameters.Add("@Timestamp", SqlDbType.DateTime).Value = instrument.Date;
            });
        }

        private async Task InsertSubmarket(StreamSubmarket submarket)
        {
            var InsertCmd = new SqlCommand();
            await Insert(InsertCmd, () =>
            {
                var Id = int.Parse(submarket.Id.Split('-')[1]);
                var MarketId = int.Parse(submarket.MarketId.Split(':')[1]);
                InsertCmd.CommandText = "exec InsertSubmarket @Id, @MarketId, @Name, @Timestamp";
                InsertCmd.Parameters.Add("@Id", SqlDbType.Int).Value = Id;
                InsertCmd.Parameters.Add("@MarketId", SqlDbType.Int).Value = MarketId;
                InsertCmd.Parameters.Add("@Name", SqlDbType.NVarChar).Value = submarket.Name;
                InsertCmd.Parameters.Add("@Timestamp", SqlDbType.DateTime).Value = submarket.Date;
            });
        }

        private async Task InsertMarket(StreamMarket market)
        {
            var InsertCmd = new SqlCommand();
            await Insert(InsertCmd, () =>
            {
                var Id = int.Parse(market.Id.Split(':')[1]);
                InsertCmd.CommandText = "exec InsertMarket @Id, @Name, @Timestamp";
                InsertCmd.Parameters.Add("@Id", SqlDbType.Int).Value = Id;
                InsertCmd.Parameters.Add("@Name", SqlDbType.NVarChar).Value = market.Name;
                InsertCmd.Parameters.Add("@Timestamp", SqlDbType.DateTime).Value = market.Date;
            });
        }

        private async Task Insert(SqlCommand InsertCmd, Action setupParameters)
        {
            using (var conn = new SqlConnection(connectionString)) {
                await conn.OpenAsync();
                InsertCmd.Connection = conn;
                setupParameters();
                await InsertCmd.ExecuteNonQueryAsync();
            }
        }
    }
}
