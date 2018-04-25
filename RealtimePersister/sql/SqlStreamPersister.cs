using System;
using System.Collections.Generic;
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
        private string database;

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
        }
    }
}
