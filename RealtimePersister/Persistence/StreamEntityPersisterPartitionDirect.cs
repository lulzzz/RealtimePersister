using RealtimePersister.Models.Streams;
using System.Threading.Tasks;

namespace RealtimePersister
{
    public class StreamEntityPersisterPartitionDirect : StreamEntityPersisterPartition
    {
        private IStreamPersisterBatch _batch;
        private int _numItemsPerBatch;
        private bool _waitForCompletion;
        private int _currentNumItemsInBatch;

        public StreamEntityPersisterPartitionDirect(IStreamPersister persister, StreamEntityType entityType, int partitionKey, int numItemsPerBatch = 0, bool waitForCompletion = true)
            : base(persister, entityType, partitionKey)
        {
            _numItemsPerBatch = numItemsPerBatch;
            _waitForCompletion = waitForCompletion;
        }
        public override async Task ProcessStreamItem(StreamEntityBase item)
        {
            if (_persister != null && item != null)
            {
                if (_numItemsPerBatch > 0 && _persister.SupportsBatches && _batch == null)
                {
                    _batch = await _persister.CreateBatch(_entityType);
                    _currentNumItemsInBatch = 0;
                }

                item.PartitionKey = _partitionKey;
                var task = (item.Operation == StreamOperation.Insert || item.Operation == StreamOperation.Update ? _persister.Upsert(item, _batch) : _persister.Delete(item, _batch));
                if (_batch != null)
                {
                    await task;
                    _currentNumItemsInBatch++;
                    if (_currentNumItemsInBatch >= _numItemsPerBatch)
                    {
                        if (_waitForCompletion)
                            await _batch.Commit();
                        _batch = null;
                    }
                }
                else if (_waitForCompletion)
                    await task;
            }
        }
    }
}
