using RealtimePersister.Models.Streams;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace RealtimePersister
{
    public class StreamEntityPersisterQueueInMemory : StreamEntityPersisterQueue
    {
        private Dictionary<string, StreamEntityPersisterItem> _dict1 =
            new Dictionary<string, StreamEntityPersisterItem>();
        private Dictionary<string, StreamEntityPersisterItem> _dict2 =
            new Dictionary<string, StreamEntityPersisterItem>();
        private Dictionary<string, StreamEntityPersisterItem> _pendingItems;
        private Dictionary<string, StreamEntityPersisterItem> _processItems;
        private int _lockState = 0; // Idle = 0, Switching = 1, Adding = 2

        public StreamEntityPersisterQueueInMemory(StreamEntityType entityType, int partitionKey, int holdOffBusy = 0, int holdOffIdle = 100)
            : base(entityType, partitionKey, holdOffBusy, holdOffIdle)
        {
            _pendingItems = _dict1;
            _processItems = _dict2;
        }

        public override Task ProcessStreamItem(StreamEntityBase item)
        {
            if (item != null)
            {
                var lockState = Interlocked.CompareExchange(ref _lockState, 2, 0);
                while (lockState != 0)
                {
                    Thread.SpinWait(10);
                    lockState = Interlocked.CompareExchange(ref _lockState, 2, 0);
                }

                if (!_pendingItems.TryGetValue(item.Id, out StreamEntityPersisterItem persiterItem))
                {
                    _pendingItems.Add(item.Id, new StreamEntityPersisterItem()
                    {
                        UpsertItem = (item.Operation == StreamOperation.Insert ||
                                        item.Operation == StreamOperation.Update ? item : null),
                        DeleteItem = (item.Operation == StreamOperation.Delete ? item : null)
                    });
                }
                else if (item.Operation == StreamOperation.Insert ||
                                        item.Operation == StreamOperation.Update)
                    persiterItem.UpsertItem = item;
                else if (item.Operation == StreamOperation.Delete)
                    persiterItem.DeleteItem = item;

                Interlocked.Exchange(ref _lockState, 0);
            }
            return Task.CompletedTask;
        }

        public override async Task ProcessPendingItems(IStreamPersister persister, CancellationToken cancellationToken, int maxItems = 50)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                bool anyDataProcessed = false;

                SwitchDictionaries();
                while (_processItems.Values.Any())
                {
                    int storedItems = 0;
                    IStreamPersisterBatch batch = null;

                    while (_processItems.Values.Any() && storedItems <= maxItems && !cancellationToken.IsCancellationRequested)
                    {
                        if (persister != null)
                        {
                            if (persister.SupportsBatches && batch == null)
                                batch = await persister.CreateBatch(_entityType);

                            var processItemPair = _processItems.First();
                            if (_processItems.Remove(processItemPair.Key))
                            {
                                if (processItemPair.Value.DeleteItem != null)
                                    await persister.Delete(processItemPair.Value.DeleteItem, batch);
                                else if (processItemPair.Value.UpsertItem != null)
                                    await persister.Upsert(processItemPair.Value.UpsertItem, batch);
                                storedItems++;
                                anyDataProcessed = true;
                            }
                        }
                    }

                    if (batch != null)
                        await batch.Commit();
                }

                await Task.Delay(anyDataProcessed ? _holdOffBusy : _holdOffIdle);
            }
        }

        private void SwitchDictionaries()
        {
            var lockState = Interlocked.CompareExchange(ref _lockState, 1, 0);
            while (lockState != 0)
            {
                Thread.SpinWait(10);
                lockState = Interlocked.CompareExchange(ref _lockState, 1, 0);
            }

            if (_pendingItems == _dict1)
            {
                _pendingItems = _dict2;
                _processItems = _dict1;
            }
            else
            {
                _pendingItems = _dict1;
                _processItems = _dict2;
            }
            Interlocked.Exchange(ref _lockState, 0);
        }
    }
}
