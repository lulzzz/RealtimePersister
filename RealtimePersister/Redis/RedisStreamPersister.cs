using RealtimePersister;
using RealtimePersister.Models.Streams;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace RealtimePersister.Redis
{
    public class RedisStreamPersister : IStreamPersister
    {
        private string _databaseName;
        private string _adress;
        private ConnectionMultiplexer _redis;
        private IDatabase _db = null;

        static private int _numUpserts;
        static private int _numDeletes;
        static private int _numBatches;
        static private DateTime _lastReported = DateTime.UtcNow;

        public bool SupportsBatches => true;

        public RedisStreamPersister(string databaseName, string adress)
        {
            _databaseName = databaseName;
            _adress = adress;
        }

        public async Task<bool> Connect()
        {
            _redis = await ConnectionMultiplexer.ConnectAsync(_adress);
            if (_redis != null)
            {
                _db = _redis.GetDatabase();
            }

            ReportMetrics();

            return (_db != null ? true : false);
        }

        public Task Disconnect()
        {
            // TODO ?
            return Task.CompletedTask;
        }


        public Task<IStreamPersisterBatch> CreateBatch(StreamEntityType type)
        {
            Interlocked.Increment(ref _numBatches);
            return Task.FromResult<IStreamPersisterBatch>(new RedisStreamPersisterBatch(_db));
        }

        public async Task<StoredLatency> Upsert(StreamEntityBase item, IStreamPersisterBatch batch = null)
        {
            double storedLatency = 0.0;
            var data = item.ToProtoBufByteArray();
            if (batch != null)
            {
                RedisStreamPersisterBatch stx = batch as RedisStreamPersisterBatch;
                stx.AddItem(item, stx.Batch.HashSetAsync($"{_databaseName}-{item.EntityType}", item.Id, data));
                storedLatency = 0.0;
            }
            else
            {
                await _db.HashSetAsync($"{_databaseName}-{item.EntityType}", item.Id, data);
                storedLatency = StreamEntityPersisterPartition.GetStoredLatency(item);
            }
            Interlocked.Increment(ref _numUpserts);
            return new StoredLatency { NumItems = (batch == null ? 1 : 0), Time = storedLatency };
        }

        public async Task<StoredLatency> Delete(StreamEntityBase item, IStreamPersisterBatch batch = null)
        {
            double storedLatency = 0.0;
            if (batch != null)
            {
                RedisStreamPersisterBatch stx = batch as RedisStreamPersisterBatch;
                stx.AddItem(item, stx.Batch.HashDeleteAsync($"{_databaseName}-{item.EntityType}", item.Id));
                storedLatency = 0.0;
            }
            else
            {
                await _db.HashDeleteAsync($"{_databaseName}-{item.EntityType}", item.Id);
                storedLatency = StreamEntityPersisterPartition.GetStoredLatency(item);
            }
            return new StoredLatency { NumItems = (batch == null ? 1 : 0), Time = storedLatency };
        }

        public Task<IEnumerable<T>> GetAll<T>(StreamEntityType entityType) where T : StreamEntityBase
        {
            var entries = _db.HashGetAll($"{_databaseName}-{entityType}");
            if (entries != null)
            {
                var items = new List<T>();
                foreach (var entry in entries)
                {
                    try
                    {
                        items.Add(StreamEntityBase.FromProtoBufByteArray(entry.Value) as T);
                    }
                    catch (Exception ex)
                    {
                    }
                }
                return Task.FromResult(items.AsEnumerable());
            }
            else
                return Task.FromResult<IEnumerable<T>>(null);
        }

        public Task<T> GetById<T>(StreamEntityType entityType, string id) where T : StreamEntityBase
        {
            var data = _db.HashGet($"{_databaseName}-{entityType}", id);
            if (data.HasValue)
            {
                try
                {
                    return Task.FromResult(StreamEntityBase.FromProtoBufByteArray(data) as T);
                }
                catch (Exception ex)
                {
                    return Task.FromResult<T>(null);
                }
            }
            else
                return Task.FromResult<T>(null);
        }

        public Task<IEnumerable<T>> GetFromSequenceNumber<T>(StreamEntityType entityType, ulong sequenceNumberStart = 0, ulong sequenceNumberEnd = ulong.MaxValue) where T : StreamEntityBase
        {
            var entries = _db.HashGetAll($"{_databaseName}-{entityType}");
            if (entries != null)
            {
                var items = new List<T>();
                foreach (var entry in entries)
                {
                    try
                    {
                        var item = StreamEntityBase.FromProtoBufByteArray(entry.Value) as T;
                        if (item.SequenceNumber >= sequenceNumberStart && item.SequenceNumber <= sequenceNumberEnd)
                            items.Add(item);
                    }
                    catch (Exception ex)
                    {

                    }
                }
                return Task.FromResult(items.AsEnumerable());
            }
            else
                return Task.FromResult<IEnumerable<T>>(null);
        }

        private Task ReportMetrics()
        {
            return Task.Run(() =>
            {
                while (true)
                {
                    DateTime now = DateTime.UtcNow;

                    if (now > (_lastReported + TimeSpan.FromSeconds(10)))
                    {
                        var numBatches = Interlocked.Exchange(ref _numBatches, 0);
                        var numUpserts = Interlocked.Exchange(ref _numUpserts, 0);
                        var numDeletes = Interlocked.Exchange(ref _numDeletes, 0);

                        Console.WriteLine($"Redis Persister: {numBatches / 10} batches /sec, {numUpserts / 10} upserts /sec, {numDeletes / 10} deletes /sec, ");
                        _lastReported = now;
                    }

                    Task.Delay(100).Wait();
                }
            });
        }
    }
}
