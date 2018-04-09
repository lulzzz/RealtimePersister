using RealtimePersister.Models.Streams;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace RealtimePersister
{
    public class StreamItemPersisterState<T> where T : StreamEntityBase
    {
        public T UpsertItem { get; set; }
        public T DeleteItem { get; set; }
    }

    public abstract class StreamItemPersister<T> where T : StreamEntityBase
    {
        protected StreamEntityType _entityType;

        public StreamItemPersister(StreamEntityType entityType)
        {
            _entityType = entityType;
        }

        public abstract Task ProcessStreamItem(T item);
        public abstract Task<bool> ProcessPendingItems(IStreamPersister persister, CancellationToken cancellationToken, int maxItems = 50);

#region Get functions
        public Task<IEnumerable<T>> GetAll(IStreamPersister persister, StreamEntityType entityType)
        {
            return (persister != null ? persister.GetAll<T>(entityType) : Task.FromResult<IEnumerable<T>>(null));
        }

        public Task<T> GetById(IStreamPersister persister, StreamEntityType entityType, string id)
        {
            return (persister != null ? persister.GetById<T>(entityType, id) : Task.FromResult<T>(null));
        }

        public Task<IEnumerable<T>> GetFromSequenceNumber(IStreamPersister persister, StreamEntityType entityType,
                UInt64 sequenceNumberStart = UInt64.MinValue, UInt64 sequenceNumberEnd = UInt64.MaxValue)
        {
            return (persister != null ? persister.GetFromSequenceNumber<T>(entityType, sequenceNumberStart, sequenceNumberEnd) : Task.FromResult<IEnumerable<T>>(null));
        }
#endregion
    }
}
