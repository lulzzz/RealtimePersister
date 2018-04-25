using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.Documents.Linq;
using RealtimePersister.Models.Streams;
using System;
using System.Collections.Generic;
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
            EnableCrossPartitionQuery = true,
            EnableScanInQuery = true
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
                ConnectionProtocol = Protocol.Https,
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
            throw new NotImplementedException();
        }

        public async Task Upsert(StreamEntityBase item, IStreamPersisterBatch batch = null)
        {
            var collectionUri = UriFactory.CreateDocumentCollectionUri(_database, _collection);

            var response = await _client.UpsertDocumentAsync(collectionUri, item,
                    new RequestOptions { PartitionKey = new PartitionKey(item.Id) });

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
                IndexingPolicy = new IndexingPolicy { IndexingMode = IndexingMode.None, Automatic = false }
            };

            collectionInfo.PartitionKey.Paths.Add($"/id");

            await _client.CreateDocumentCollectionAsync(
                UriFactory.CreateDatabaseUri(_database),
                collectionInfo,
                new RequestOptions
                {
                    OfferThroughput = _offerThroughput,
                    ConsistencyLevel = ConsistencyLevel.Eventual
                });
        }
    }
}
