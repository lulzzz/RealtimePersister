using RealtimePersister.Models.Streams;
using System.Threading.Tasks;

namespace RealtimePersister
{
    public abstract class StreamEntityPersisterPartition
    {
        protected IStreamPersister _persister;
        protected StreamEntityType _entityType;
        protected int _partitionKey;

        public StreamEntityPersisterPartition(IStreamPersister persister, StreamEntityType entityType, int partitionKey)
        {
            _persister = persister;
            _entityType = entityType;
            _partitionKey = partitionKey;
        }

        public abstract Task ProcessStreamItem(StreamEntityBase item);
    }
}
