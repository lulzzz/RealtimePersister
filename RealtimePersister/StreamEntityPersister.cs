using RealtimePersister.Models.Streams;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace RealtimePersister
{
    public class StreamEntityPersister
    {
        protected StreamEntityPersisterQueue[] _queues = null;
        protected int _numPartitions;

        public StreamEntityPersister(StreamEntityType entityType, IStreamPersister persister, CancellationToken cancellationToken, int numPartitions, int holdOffBusy = 0, int holdOffIdle = 100)
        {
            _numPartitions = numPartitions;
            _queues = new StreamEntityPersisterQueue[Math.Max(numPartitions, 1)];
            for (int partitionKey = 0; partitionKey < _queues.Count(); partitionKey++)
            {
                _queues[partitionKey] = new StreamEntityPersisterQueueInMemory(entityType, partitionKey, holdOffBusy, holdOffIdle);
                _queues[partitionKey].ProcessPendingItems(persister, cancellationToken/* , numItems TODO */);
            }
        }

        public Task ProcessStreamItem(StreamEntityBase streamItem)
        {
            int partitionKey = (_numPartitions > 0 ? Math.Max(streamItem.GetPartitionKey(_numPartitions), 0) : 0);
            return _queues[partitionKey].ProcessStreamItem(streamItem);
        }
    }
}
