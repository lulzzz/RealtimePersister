using Microsoft.Azure.Documents.Client;
using RealtimePersister.Models.Streams;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RealtimePersister.CosmosDb
{
    public class CosmosDBStreamPersisterBatch : IStreamPersisterBatch
    {
        private List<Dictionary<string, object>> _items { get; } = new List<Dictionary<string, object>>();
        private readonly DocumentClient _client;
        private readonly Uri _sprocLink;

        public CosmosDBStreamPersisterBatch(DocumentClient client, Uri sprocLink)
        {
            _client = client;
            _sprocLink = sprocLink;
        }

        public async Task Commit()
        {
            var partitionKey = _items.First()[nameof(StreamEntityBase.PartitionKey)];

            var response = await _client.ExecuteStoredProcedureAsync<string>(_sprocLink, 
                new RequestOptions { PartitionKey = new Microsoft.Azure.Documents.PartitionKey(partitionKey) }, 
                new { docs = _items });

            //var ru = response.RequestCharge;
        }

        public void AddItem(Dictionary<string, object> item)
        {
            _items.Add(item);
        }
    }
}
