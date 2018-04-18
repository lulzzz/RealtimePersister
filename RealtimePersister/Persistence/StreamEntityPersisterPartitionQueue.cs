using RealtimePersister.Models.Streams;
using System.Threading;
using System.Threading.Tasks;

namespace RealtimePersister
{
    public class StreamEntityPersisterItem
    {
        public StreamEntityBase UpsertItem { get; set; }
        public StreamEntityBase DeleteItem { get; set; }
    }

    public abstract class StreamEntityPersisterPartitionQueue : StreamEntityPersisterPartition
    {
        protected int _holdOffBusy;
        protected int _holdOffIdle;

        public StreamEntityPersisterPartitionQueue(IStreamPersister persister, StreamEntityType entityType, int partitionKey, int holdOffBusy, int holdOffIdle)
            : base(persister, entityType, partitionKey)
        {
            _holdOffBusy = holdOffBusy;
            _holdOffIdle = holdOffIdle;
        }

        public abstract Task ProcessPendingItems(CancellationToken cancellationToken);
    }
}
