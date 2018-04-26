﻿using System;
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

    public class SqlStreamPersister : IStreamPersister, IStreamPersisterBatch
    {
        string connectionString = "Server=andersth-sql-wus2.database.windows.net;Database=StreamEntityPersist;Integrated Security=False;User Id=andersth;Password=P@ssword.666";
        //string connectionString = "Server=10.0.30.179;Database=StreamEntityPersist;Integrated Security=True";
        string database;

        DataTable currentBatch;

        public SqlStreamPersister(string database)
        {
            this.database = database;
        }

        public bool SupportsBatches => true;

        public async Task<bool> Connect()
        {
            return true;
        }

        public async Task<IStreamPersisterBatch> CreateBatch(StreamEntityType type)
        {
            currentBatch = new DataTable();
            // CREATE TYPE [dbo].[PriceTableType] AS TABLE
            // (
            //	 InstrumentId INT, Price FLOAT, PriceDate DATETIME, Timestamp DATETIME, SequenceNumber bigint
            // )
            currentBatch.Columns.Add("InstrumentId", typeof(int));
            currentBatch.Columns.Add("Price", typeof(double));
            currentBatch.Columns.Add("PriceDate", typeof(DateTime));
            currentBatch.Columns.Add("Timestamp", typeof(DateTime));
            currentBatch.Columns.Add("SequenceNumber", typeof(long));
            return this;
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
            } else if (item.EntityType == StreamEntityType.Price) {
                if (tx != null) {
                    await UpsertPrice(item as StreamPrice);
                } else {
                    UpsertPriceBatch(item as StreamPrice);
                }
            } else {
                Debugger.Break();
            }
        }

        private void UpsertPriceBatch(StreamPrice price)
        {
            var IntrumentTmp = price.Id.Split(':')[1];
            var InstrumentIdSplit = IntrumentTmp.Split('-');
            var MarketId = int.Parse(InstrumentIdSplit[0]);
            var SubmarketId = int.Parse(InstrumentIdSplit[1]);
            var Id = int.Parse(InstrumentIdSplit[2]);

            currentBatch.Rows.Add((MarketId * 10 + SubmarketId) * 1000000 + Id, price.PriceLatest, price.PriceDate, price.Date, price.SequenceNumber);
        }

        public async Task Commit()
        {
            var CallSPCmd = new SqlCommand();
            await DoSqlCmd(CallSPCmd, () =>
            {
                CallSPCmd.CommandText = "exec UpsertPriceBatch @PriceTable";
                CallSPCmd.Parameters.Add("@PriceTable", SqlDbType.Structured).Value = currentBatch;
            });
        }

        private async Task UpsertPrice(StreamPrice price)
        {
            var CallSPCmd = new SqlCommand();
            await DoSqlCmd(CallSPCmd, () =>
            {
                var IntrumentTmp = price.Id.Split(':')[1];
                var InstrumentIdSplit = IntrumentTmp.Split('-');
                var MarketId = int.Parse(InstrumentIdSplit[0]);
                var SubmarketId = int.Parse(InstrumentIdSplit[1]);
                var Id = int.Parse(InstrumentIdSplit[2]);

                CallSPCmd.CommandText = "exec UpsertPrice @InstrumentId, @Price, @PriceLatest, @Timestamp, @SequenceNumber";
                CallSPCmd.Parameters.Add("@InstrumentId", SqlDbType.Int).Value = (MarketId * 10 + SubmarketId) * 1000000 + Id;
                CallSPCmd.Parameters.Add("@Price", SqlDbType.Float).Value = price.PriceLatest;
                CallSPCmd.Parameters.Add("@Pricelatest", SqlDbType.DateTime).Value = price.PriceDate;
                CallSPCmd.Parameters.Add("@Timestamp", SqlDbType.DateTime).Value = price.Date;
                CallSPCmd.Parameters.Add("@SequenceNumber", SqlDbType.BigInt).Value = price.SequenceNumber;
            });
        }

        private async Task InsertRule(StreamRule rule)
        {
            var CallSPCmd = new SqlCommand();
            await DoSqlCmd(CallSPCmd, () =>
            {
                var Id = int.Parse(rule.Id.Split('-')[1]);
                var PortfolioId = int.Parse(rule.PortfolioId.Split(':')[1]);
                CallSPCmd.CommandText = "exec InsertRule @Id, @PortfolioId, @Expression, @Timestamp";
                CallSPCmd.Parameters.Add("@Id", SqlDbType.Int).Value = PortfolioId * 1000000+Id;
                CallSPCmd.Parameters.Add("@PortfolioId", SqlDbType.Int).Value = PortfolioId;
                CallSPCmd.Parameters.Add("@Expression", SqlDbType.NVarChar).Value = rule.Expression;
                CallSPCmd.Parameters.Add("@Timestamp", SqlDbType.DateTime).Value = rule.Date;
            });
        }

        private async Task InsertPosition(StreamPosition position)
        {
            var CallSPCmd = new SqlCommand();
            await DoSqlCmd(CallSPCmd, () =>
            {
                var Id = int.Parse(position.Id.Split('-')[1]);
                var InstrumentId = int.Parse(position.InstrumentId.Split('-')[2]);
                var PortfolioId = int.Parse(position.PortfolioId.Split(':')[1]);
                CallSPCmd.CommandText = "exec InsertPosition @Id, @InstrumentId, @PortfolioId, @Volume, @Price, @Timestamp";
                CallSPCmd.Parameters.Add("@Id", SqlDbType.Int).Value = PortfolioId*1000000+Id;
                CallSPCmd.Parameters.Add("@InstrumentId", SqlDbType.Int).Value = InstrumentId;
                CallSPCmd.Parameters.Add("@PortfolioId", SqlDbType.Int).Value = PortfolioId;
                CallSPCmd.Parameters.Add("@Volume", SqlDbType.Int).Value = position.Volume;
                CallSPCmd.Parameters.Add("@Price", SqlDbType.Int).Value = position.Price;
                CallSPCmd.Parameters.Add("@Timestamp", SqlDbType.DateTime).Value = position.Date;
            });
        }

        private async Task InsertPortfolio(StreamPortfolio portfolio)
        {
            var CallSPCmd = new SqlCommand();
            await DoSqlCmd(CallSPCmd, () =>
            {
                var Id = int.Parse(portfolio.Id.Split(':')[1]);
                CallSPCmd.CommandText = "exec InsertPortfolio @Id, @Name, @Balance, @CheckRules, @Timestamp";
                CallSPCmd.Parameters.Add("@Id", SqlDbType.Int).Value = Id;
                CallSPCmd.Parameters.Add("@Name", SqlDbType.NVarChar).Value = portfolio.Name;
                CallSPCmd.Parameters.Add("@Balance", SqlDbType.Float).Value = portfolio.Balance;
                CallSPCmd.Parameters.Add("@CheckRules", SqlDbType.Bit).Value = portfolio.CheckRules;
                CallSPCmd.Parameters.Add("@Timestamp", SqlDbType.DateTime).Value = portfolio.Date;
            });
        }

        private async Task InsertInstrument(StreamInstrument instrument)
        {
            var CallSPCmd = new SqlCommand();
            await DoSqlCmd(CallSPCmd, () =>
            {
                var InstrumentIdTmp = instrument.Id.Split(':')[1];
                var InstrumentIdSplit = InstrumentIdTmp.Split('-');
                var MarketId = int.Parse(InstrumentIdSplit[0]);
                var SubmarketId = int.Parse(InstrumentIdSplit[1]);
                var Id = int.Parse(InstrumentIdSplit[2]);
                CallSPCmd.CommandText = "exec InsertInstrument @Id, @SubmarketId, @Name, @Timestamp";
                CallSPCmd.Parameters.Add("@Id", SqlDbType.Int).Value = (MarketId*10+SubmarketId)*1000000+Id;
                CallSPCmd.Parameters.Add("@SubmarketId", SqlDbType.Int).Value = SubmarketId;
                CallSPCmd.Parameters.Add("@Name", SqlDbType.NVarChar).Value = instrument.Name;
                CallSPCmd.Parameters.Add("@Timestamp", SqlDbType.DateTime).Value = instrument.Date;
            });
        }

        private async Task InsertSubmarket(StreamSubmarket submarket)
        {
            var CallSPCmd = new SqlCommand();
            await DoSqlCmd(CallSPCmd, () =>
            {
                var Id = int.Parse(submarket.Id.Split('-')[1]);
                var MarketId = int.Parse(submarket.MarketId.Split(':')[1]);
                CallSPCmd.CommandText = "exec InsertSubmarket @Id, @MarketId, @Name, @Timestamp";
                CallSPCmd.Parameters.Add("@Id", SqlDbType.Int).Value = Id;
                CallSPCmd.Parameters.Add("@MarketId", SqlDbType.Int).Value = MarketId;
                CallSPCmd.Parameters.Add("@Name", SqlDbType.NVarChar).Value = submarket.Name;
                CallSPCmd.Parameters.Add("@Timestamp", SqlDbType.DateTime).Value = submarket.Date;
            });
        }

        private async Task InsertMarket(StreamMarket market)
        {
            var CallSPCmd = new SqlCommand();
            await DoSqlCmd(CallSPCmd, () =>
            {
                var Id = int.Parse(market.Id.Split(':')[1]);
                CallSPCmd.CommandText = "exec InsertMarket @Id, @Name, @Timestamp";
                CallSPCmd.Parameters.Add("@Id", SqlDbType.Int).Value = Id;
                CallSPCmd.Parameters.Add("@Name", SqlDbType.NVarChar).Value = market.Name;
                CallSPCmd.Parameters.Add("@Timestamp", SqlDbType.DateTime).Value = market.Date;
            });
        }

        private async Task DoSqlCmd(SqlCommand SqlCmd, Action setupParameters)
        {
            using (var conn = new SqlConnection(connectionString)) {
                await conn.OpenAsync();
                SqlCmd.Connection = conn;
                setupParameters();
                await SqlCmd.ExecuteNonQueryAsync();
            }
        }
    }
}
