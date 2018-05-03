using RealtimePersister.Models.Streams;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace RealtimePersister
{
    public class StreamEntityPersister
    {
        protected StreamEntityPersisterPartition[] _partitions = null;
        protected int _numPartitions;

        public StreamEntityPersister(StreamEntityType entityType, IStreamPersister persister, CancellationToken cancellationToken, int numPartitions, bool direct = false, int holdOffBusy = 0, int holdOffIdle = 100)
        {
            _numPartitions = numPartitions;
            _partitions = new StreamEntityPersisterPartition[Math.Max(numPartitions, 1)];
            for (int partitionKey = 0; partitionKey < _partitions.Count(); partitionKey++)
            {
                if (direct)
                    _partitions[partitionKey] = new StreamEntityPersisterPartitionDirect(persister, entityType, partitionKey, 100);
                else
                {
                    var persisterPartition = new StreamEntityPersisterPartitionAggregateQueueInMemory(persister, entityType, partitionKey, holdOffBusy, holdOffIdle);
                    _partitions[partitionKey] = persisterPartition;
                    persisterPartition.ProcessPendingItems(cancellationToken);
                }
            }
        }

        public Task ProcessStreamItem(StreamEntityBase streamItem)
        {
            int partitionKey = (_numPartitions > 0 ? Math.Max(streamItem.GetPartitionKey(_numPartitions), 0) : 0);
            return _partitions[partitionKey].ProcessStreamItem(streamItem);
        }
    }
}
