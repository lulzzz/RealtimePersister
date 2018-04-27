using Cassandra;
using Cassandra.Mapping;
using Microsoft.Azure.Documents.Client;
using RealtimePersister.Models.Streams;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RealtimePersister.CosmosDb
{
    public class CosmosDBSCassandratreamPersisterBatch : IStreamPersisterBatch
    {
        private readonly ISession _session;
        private readonly PreparedStatement _itemstmt;
        private readonly BatchStatement _batchStmt = new BatchStatement();
        private List<Dictionary<string, object>> _items { get; } = new List<Dictionary<string, object>>();

        public CosmosDBSCassandratreamPersisterBatch(ISession session)
        {
            _session = session;
            _itemstmt = _session.Prepare("insert into streamentity(id, streamname, sequencenumber, entitytype, operation, date) values(?, ?, ?, ?, ?, ?)");
        }

        public async Task<StoredLatency> Commit()
        {
            await _session.ExecuteAsync(_batchStmt);
            return new StoredLatency() { NumItems = _items.Count, Time = StreamEntityPersisterPartition.GetStoredLatency(_items) };
        }

        public void AddItem(StreamEntityBase item)
        {
            _batchStmt.Add(_itemstmt.Bind(item.Id, item.StreamName, (long)item.SequenceNumber, item.EntityType.ToString(), item.Operation.ToString(), item.Date));
            _items.Add(item.ToKeyValueDictionary()); 
        }
    }
}
