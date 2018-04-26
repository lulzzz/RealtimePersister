using RealtimePersister.Models.Streams;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace RealtimePersister
{
    public class StreamEntityPersisterPartitionQueueInMemory : StreamEntityPersisterPartitionQueue
    {
        private Dictionary<string, StreamEntityPersisterItem> _dict1 =
            new Dictionary<string, StreamEntityPersisterItem>();
        private Dictionary<string, StreamEntityPersisterItem> _dict2 =
            new Dictionary<string, StreamEntityPersisterItem>();
        private bool _dict1IsPending = true;
        private Dictionary<string, StreamEntityPersisterItem> PendingItems { get { return (_dict1IsPending ? _dict1 : _dict2); } }
        private Dictionary<string, StreamEntityPersisterItem> ProcessItems { get { return (_dict1IsPending ? _dict2 : _dict1); } }
        private int _lockState = 0; // Idle = 0, Switching = 1, Adding = 2

        static private int _numUpserts;
        static private int _numDeletes;
        static private int _numProcessedItems;
        static private int _numSwitchDirectories;
        static long _reportingRunning;
        static private DateTime _lastReported = DateTime.UtcNow;

        public StreamEntityPersisterPartitionQueueInMemory(IStreamPersister persister, StreamEntityType entityType, int partitionKey, int holdOffBusy = 0, int holdOffIdle = 100)
            : base(persister, entityType, partitionKey, holdOffBusy, holdOffIdle)
        {
            var reportingRunning = Interlocked.Exchange(ref _reportingRunning, 1);
            if (reportingRunning == 0)
                ReportMetrics();
        }

        public override Task ProcessStreamItem(StreamEntityBase item)
        {
            if (item != null)
            {
                var lockState = Interlocked.CompareExchange(ref _lockState, 2, 0);
                while (lockState == 1)
                {
                    Thread.SpinWait(10);
                    lockState = Interlocked.CompareExchange(ref _lockState, 2, 0);
                }

                item.PartitionKey = _partitionKey;
                var pendingItems = PendingItems;
                lock (this)
                {
                    pendingItems[item.Id] = new StreamEntityPersisterItem()
                    {
                        UpsertItem = (item.Operation == StreamOperation.Insert ||
                                        item.Operation == StreamOperation.Update ? item : null),
                        DeleteItem = (item.Operation == StreamOperation.Delete ? item : null)
                    };
                }

                if (item.Operation == StreamOperation.Insert || item.Operation == StreamOperation.Update)
                    Interlocked.Increment(ref _numUpserts);
                else
                    Interlocked.Increment(ref _numDeletes);

                Interlocked.Exchange(ref _lockState, 0);
            }
            return Task.CompletedTask;
        }

        public override async Task ProcessPendingItems(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                bool anyDataProcessed = false;

                SwitchDictionaries();
                Interlocked.Increment(ref _numSwitchDirectories);
                var processItems = ProcessItems;
                while (processItems.Values.Any())
                {
                    int storedItems = 0;
                    IStreamPersisterBatch batch = null;

                    while (processItems.Values.Any() && !cancellationToken.IsCancellationRequested)
                    {
                        if (_persister != null)
                        {
                            if (_persister.SupportsBatches && batch == null)
                                batch = await _persister.CreateBatch(_entityType);

                            var processItemPair = processItems.First();
                            if (processItems.Remove(processItemPair.Key))
                            {
                                if (processItemPair.Value.DeleteItem != null)
                                    await _persister.Delete(processItemPair.Value.DeleteItem, batch);
                                else if (processItemPair.Value.UpsertItem != null)
                                    await _persister.Upsert(processItemPair.Value.UpsertItem, batch);
                                storedItems++;
                                anyDataProcessed = true;

                                Interlocked.Increment(ref _numProcessedItems);
                            }
                        }
                        else
                        {
                            Interlocked.Add(ref _numProcessedItems, processItems.Count);
                            processItems.Clear();
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
            while (lockState == 2)
            {
                Thread.SpinWait(10);
                lockState = Interlocked.CompareExchange(ref _lockState, 1, 0);
            }

            _dict1IsPending = !_dict1IsPending;
            Interlocked.Exchange(ref _lockState, 0);
        }

        private Task ReportMetrics()
        {
            return Task.Run(() =>
            {
                while (true)
                {
                    DateTime now = DateTime.UtcNow;

                    if (now > (_lastReported + TimeSpan.FromSeconds(10)))
                    {
                        var numUpserts = Interlocked.Exchange(ref _numUpserts, 0);
                        var numDeletes = Interlocked.Exchange(ref _numDeletes, 0);
                        var numProcessedItems = Interlocked.Exchange(ref _numProcessedItems, 0);
                        var numSwitchedDirectories = Interlocked.Exchange(ref _numSwitchDirectories, 0);

                        Console.WriteLine($"In-memory persister queue: {numProcessedItems / 10} processed items /sec, {numUpserts / 10} upserts /sec, {numDeletes / 10} deletes /sec, {numSwitchedDirectories / 10} switch directories / sec");
                        _lastReported = now;
                    }

                    Task.Delay(100).Wait();
                }
            });
        }
    }
}
