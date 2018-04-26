using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.Documents.Linq;
using RealtimePersister.Models.Streams;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace RealtimePersister.CosmosDb
{
    public class CosmosDbStreamPersister : IStreamPersister
    {
        private string _uri;
        private string _key;
        private string _database;
        private string _collection;
        private int _offerThroughput;
        private DocumentClient _client;

        private static readonly FeedOptions FeedOptions = new FeedOptions
        {
            MaxItemCount = -1,
            EnableCrossPartitionQuery = true/*,
            EnableScanInQuery = true*/
        };

        public bool SupportsBatches => false;

        public CosmosDbStreamPersister(string uri, string key, string database, string collection, int offerThroughput)
        {
            _uri = uri;
            _key = key;
            _database = database;
            _collection = collection;
            _offerThroughput = offerThroughput;
        }

        public async Task<bool> Connect()
        {
            var connectionPolicy = new ConnectionPolicy
            {
                ConnectionMode = ConnectionMode.Direct,
                ConnectionProtocol = Protocol.Tcp,
                RequestTimeout = new TimeSpan(1, 0, 0),
                MaxConnectionLimit = 1000,
                RetryOptions = new RetryOptions
                {
                    MaxRetryAttemptsOnThrottledRequests = 10,
                    MaxRetryWaitTimeInSeconds = 60
                }
            };

            _client = new DocumentClient(new Uri(_uri), _key, connectionPolicy);
            await CreateDatabaseAsync();
            await CreateCollectionAsync();
            return (_client != null ? true : false);
        }

        public Task Disconnect()
        {
            _client = null;
            return Task.CompletedTask;
        }

        public Task<IStreamPersisterBatch> CreateBatch(StreamEntityType type)
        {
            return Task.FromResult<IStreamPersisterBatch>(new CosmosDBStreamPersisterBatch());
        }

        private int _numUpserts;
        private double _timeSpentUpsert;
        private DateTime _lastReported = DateTime.UtcNow;

        private Dictionary<string, object> _cachedData = null;

        public async Task Upsert(StreamEntityBase item, IStreamPersisterBatch batch = null)
        {
            var sw = new Stopwatch();
            sw.Start();
            var collectionUri = UriFactory.CreateDocumentCollectionUri(_database, _collection);

#if true
            var task = _client.UpsertDocumentAsync(collectionUri, item.ToKeyValueDictionary(),
                    new RequestOptions { PartitionKey = new PartitionKey(item.Id) });
#else
            if (_cachedData == null)
                _cachedData = item.ToKeyValueDictionary();
            else
                item.ToKeyValueDictionary();
            
            _cachedData["id"] = Guid.NewGuid().ToString();
            var task = _client.CreateDocumentAsync(collectionUri, _cachedData, new RequestOptions() { });
#endif
            if (batch != null)
            {
                var cosmosDbBatch = batch as CosmosDBStreamPersisterBatch;
                cosmosDbBatch.AddTask(task);
            }
            else
                await task;
            sw.Stop();

            lock (this)
            {
                _numUpserts++;
                _timeSpentUpsert += sw.ElapsedMilliseconds;
                var now = DateTime.UtcNow;

                if (now > (_lastReported + TimeSpan.FromSeconds(10)))
                {
                    if (_numUpserts > 0)
                        Console.WriteLine($"CosmosDB Persister; Num upserts {_numUpserts / 10} / sec, Avg time per call {_timeSpentUpsert / _numUpserts} ms.");
                    _numUpserts = 0;
                    _timeSpentUpsert = 0;
                    _lastReported = now;
                }
            }
        }

        public async Task Delete(StreamEntityBase item, IStreamPersisterBatch batch = null)
        {
            await _client.DeleteDocumentAsync(UriFactory.CreateDocumentUri(_database, _collection, item.Id));
        }

        public async Task<IEnumerable<T>> GetAll<T>(StreamEntityType entityType) where T : StreamEntityBase
        {
            var query = _client.CreateDocumentQuery<T>(
                UriFactory.CreateDocumentCollectionUri(_database, _collection), FeedOptions)
                .Where(s => s.EntityType == entityType)
                .AsDocumentQuery();

            var items = new List<T>();
            while (query.HasMoreResults)
            {
                items.AddRange(await query.ExecuteNextAsync<T>());
            }

            return items;
        }

        public async Task<T> GetById<T>(StreamEntityType entityType, string id) where T : StreamEntityBase
        {
            try
            {
                var document = await _client.ReadDocumentAsync(UriFactory.CreateDocumentUri(_database, _collection, id));
                return (T)(dynamic)document;
            }
            catch (DocumentClientException e)
            {
                if (e.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    return null;
                }
                else
                {
                    throw;
                }
            }
        }

        public async Task<IEnumerable<T>> GetFromSequenceNumber<T>(StreamEntityType entityType, ulong sequenceNumberStart = 0, ulong sequenceNumberEnd = ulong.MaxValue) where T : StreamEntityBase
        {
            var query = _client.CreateDocumentQuery<T>(
                UriFactory.CreateDocumentCollectionUri(_database, _collection), FeedOptions)
                .Where(s => s.EntityType == entityType && s.SequenceNumber >= sequenceNumberStart && s.SequenceNumber <= sequenceNumberEnd)
                .AsDocumentQuery();

            var items = new List<T>();
            while (query.HasMoreResults)
            {
                items.AddRange(await query.ExecuteNextAsync<T>());
            }

            return items;
        }

        private async Task CreateDatabaseAsync()
        {
            var database = _client.CreateDatabaseQuery().Where(d => d.Id == _database).AsEnumerable().FirstOrDefault();
            if (database != null)
            {
                await _client.DeleteDatabaseAsync(database.SelfLink);
            }

            database = await _client.CreateDatabaseAsync(new Database { Id = _database });
        }

        private async Task CreateCollectionAsync()
        {
            var collectionInfo = new DocumentCollection
            {
                Id = _collection,
                //IndexingPolicy = new IndexingPolicy { IndexingMode = IndexingMode.None, Automatic = false }
            };

            collectionInfo.PartitionKey.Paths.Add($"/id");

            await _client.CreateDocumentCollectionAsync(
                UriFactory.CreateDatabaseUri(_database),
                collectionInfo,
                new RequestOptions
                {
                    OfferThroughput = _offerThroughput/*,
                    ConsistencyLevel = ConsistencyLevel.Eventual*/
                });
        }
    }
}
