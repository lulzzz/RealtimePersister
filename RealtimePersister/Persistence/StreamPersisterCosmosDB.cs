using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.Documents.Linq;
//using SaxoRiskPOC.Models.Streams;
//using SaxoRiskPOC.StreamPersister.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RealtimePersister.Models.Streams;

namespace RealtimePersister
{
    public class StreamPersister : IStreamPersister
    {
        private string _uri;
        private string _key;
        private string _database;
        private string _collection;
        private DocumentClient _client;

        public bool SupportsBatches => true;

        public StreamPersister(string uri, string key, string database, string collection)
        {
            _uri = uri;
            _key = key;
            _database = database;
            _collection = collection;
        }

        public async Task<bool> Connect()
        {
            ConnectionPolicy connectionPolicy = new ConnectionPolicy
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
            await CreateDatabaseIfNotExistsAsync();
            await CreateCollectionIfNotExistsAsync();
            return (_client != null ? true : false);
        }

        public Task Disconnect()
        {
            _client = null;
            return Task.CompletedTask;
        }

        public Task<IStreamPersisterBatch> CreateBatch(StreamEntityType type)
        {
            return Task.FromResult<IStreamPersisterBatch>(new StreamPersisterBatch());
        }

        public async Task Upsert(StreamEntityBase item, IStreamPersisterBatch batch = null)
        {
            var collectionUri = UriFactory.CreateDocumentCollectionUri(_database, _collection);
            try
            {
                var task = _client.UpsertDocumentAsync(collectionUri, item);
                if (batch != null)
                {
                    var stx = batch as StreamPersisterBatch;
                    stx.Tasks.Add(task);
                }
                else
                    await task;
            }
            catch (DocumentClientException e)
            {
                if (e.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    try
                    {
                        var task =  _client.CreateDocumentAsync(collectionUri, item);
                        if (batch != null)
                        {
                            var stx = batch as StreamPersisterBatch;
                            stx.Tasks.Add(task);
                        }
                        else
                            await task;
                    }
                    catch (Exception ex)
                    {

                    }
                }
                else
                {
                    throw;
                }
            }
        }

        public async Task Delete(StreamEntityBase item, IStreamPersisterBatch batch = null)
        {
            var task = _client.DeleteDocumentAsync(UriFactory.CreateDocumentUri(_database, _collection, item.Id));
            if (batch != null)
            {
                var stx = batch as StreamPersisterBatch;
                stx.Tasks.Add(task);
            }
            else
                await task;
        }

        public async Task<IEnumerable<T>> GetAll<T>(StreamEntityType entityType) where T : StreamEntityBase
        {
            IDocumentQuery<T> query = _client.CreateDocumentQuery<T>(
                UriFactory.CreateDocumentCollectionUri(_database, _collection),
                new FeedOptions { MaxItemCount = -1 })
                .Where(s => s.EntityType == entityType)
                .AsDocumentQuery();

            List<T> items = new List<T>();
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
                Document document = await _client.ReadDocumentAsync(UriFactory.CreateDocumentUri(_database, _collection, id));
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
            IDocumentQuery<T> query = _client.CreateDocumentQuery<T>(
                UriFactory.CreateDocumentCollectionUri(_database, _collection),
                new FeedOptions { MaxItemCount = -1 })
                .Where(s => s.EntityType == entityType && s.SequenceNumber >= sequenceNumberStart && s.SequenceNumber <= sequenceNumberEnd)
                .AsDocumentQuery();

            List<T> items = new List<T>();
            while (query.HasMoreResults)
            {
                items.AddRange(await query.ExecuteNextAsync<T>());
            }

            return items;
        }

        private async Task CreateDatabaseIfNotExistsAsync()
        {
            try
            {
                await _client.ReadDatabaseAsync(UriFactory.CreateDatabaseUri(_database));
            }
            catch (DocumentClientException e)
            {
                if (e.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    await _client.CreateDatabaseAsync(new Database { Id = _database });
                }
                else
                {
                    throw;
                }
            }
        }

        private async Task CreateCollectionIfNotExistsAsync()
        {
            try
            {
                await _client.ReadDocumentCollectionAsync(UriFactory.CreateDocumentCollectionUri(_database, _collection));
            }
            catch (DocumentClientException e)
            {
                if (e.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    await _client.CreateDocumentCollectionAsync(
                        UriFactory.CreateDatabaseUri(_database),
                        new DocumentCollection { Id = _collection },
                        new RequestOptions { OfferThroughput = 1000 });
                }
                else
                {
                    throw;
                }
            }
        }
    }
}
