using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
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
            }
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
