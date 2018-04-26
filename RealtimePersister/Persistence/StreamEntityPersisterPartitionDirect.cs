using RealtimePersister.Models.Streams;
using System;
using System.Threading.Tasks;

namespace RealtimePersister
{
    public class StreamEntityPersisterPartitionDirect : StreamEntityPersisterPartition
    {
        private IStreamPersisterBatch _batch;
        private int _numItemsPerBatch;
        private int _currentNumItemsInBatch;

        public StreamEntityPersisterPartitionDirect(IStreamPersister persister, StreamEntityType entityType, int partitionKey, int numItemsPerBatch = 0)
            : base(persister, entityType, partitionKey)
        {
            _numItemsPerBatch = numItemsPerBatch;
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

                var result = (item.Operation == StreamOperation.Insert || item.Operation == StreamOperation.Update ? _persister.Upsert(item, _batch) : _persister.Delete(item, _batch));
                if (_batch != null)
                {
                    _currentNumItemsInBatch++;
                    if (_currentNumItemsInBatch >= _numItemsPerBatch)
                    {
                        var storedLatency = await _batch.Commit();
                        ReportLatency(storedLatency);
                        _batch = null;
                    }
                }
                else
                {
                    var storedLatency = await result;
                    ReportLatency(storedLatency);
                }
            }

            DisplayLatency();
        }
    }
}
