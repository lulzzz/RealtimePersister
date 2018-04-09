using RealtimePersister.Models.Streams;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace RealtimePersister
{
    public class StreamItemPersisterInMemory<T> : StreamItemPersister<T> where T : StreamEntityBase
    {
        private Dictionary<string, StreamItemPersisterState<T>> _dict1 =
            new Dictionary<string, StreamItemPersisterState<T>>();
        private Dictionary<string, StreamItemPersisterState<T>> _dict2 =
            new Dictionary<string, StreamItemPersisterState<T>>();
        private Dictionary<string, StreamItemPersisterState<T>> _pendingItems;
        private Dictionary<string, StreamItemPersisterState<T>> _processItems;
        private int _lockState = 0; // Idle = 0, Switching = 1, Adding = 2

        public StreamItemPersisterInMemory(StreamEntityType entityType)
            : base(entityType)
        {
            _pendingItems = _dict1;
            _processItems = _dict2;
        }

        public override Task ProcessStreamItem(T item)
        {
            if (item != null)
            {
                var lockState = Interlocked.CompareExchange(ref _lockState, 2, 0);
                while (lockState != 0)
                {
                    Thread.SpinWait(10);
                    lockState = Interlocked.CompareExchange(ref _lockState, 2, 0);
                }

                if (!_pendingItems.TryGetValue(item.Id, out StreamItemPersisterState<T> state))
                {
                    _pendingItems.Add(item.Id, new StreamItemPersisterState<T>()
                    {
                        UpsertItem = (item.Operation == StreamOperation.Insert ||
                                        item.Operation == StreamOperation.Update ? item : null),
                        DeleteItem = (item.Operation == StreamOperation.Delete ? item : null)
                    });
                }
                else if (item.Operation == StreamOperation.Insert ||
                                        item.Operation == StreamOperation.Update)
                    state.UpsertItem = item;
                else if (item.Operation == StreamOperation.Delete)
                    state.DeleteItem = item;

                Interlocked.Exchange(ref _lockState, 0);
            }
            return Task.CompletedTask;
        }

        public override async Task<bool> ProcessPendingItems(IStreamPersister persister, CancellationToken cancellationToken, int maxItems = 50)
        {
            bool anyDataProcessed = false;

            if (persister != null)
            {
                SwitchDictionaries();
                while (_processItems.Values.Any())
                {
                    int storedItems = 0;
                    IStreamPersisterBatch batch = null;

                    while (_processItems.Values.Any() && storedItems <= maxItems && !cancellationToken.IsCancellationRequested)
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

                    if (batch != null)
                        await batch.Commit();
                }
            }
            return anyDataProcessed;
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
