using RealtimePersister.Models.Streams;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace RealtimePersister
{
    public class StreamEntityPersisterItem
    {
        public StreamEntityBase UpsertItem { get; set; }
        public StreamEntityBase DeleteItem { get; set; }
    }

    public abstract class StreamEntityPersisterQueue
    {
        protected StreamEntityType _entityType;
        protected int _partitionKey;
        protected int _holdOffBusy;
        protected int _holdOffIdle;

        public StreamEntityPersisterQueue(StreamEntityType entityType, int partitionKey, int holdOffBusy, int holdOffIdle)
        {
            _entityType = entityType;
            _partitionKey = partitionKey;
            _holdOffBusy = holdOffBusy;
            _holdOffIdle = holdOffIdle;
        }


        public abstract Task ProcessStreamItem(StreamEntityBase item);
        public abstract Task ProcessPendingItems(IStreamPersister persister, CancellationToken cancellationToken, int maxItems = 50);
    }
}
