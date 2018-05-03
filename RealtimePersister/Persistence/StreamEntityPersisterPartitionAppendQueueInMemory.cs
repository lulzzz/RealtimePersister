using RealtimePersister.Models.Streams;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace RealtimePersister.Persistence
{
    public class StreamEntityPersisterPartitionAppendQueueInMemory : StreamEntityPersisterPartitionQueue
    {
        private List<StreamEntityBase> _queue1 = new List<StreamEntityBase>();
        private List<StreamEntityBase> _queue2 = new List<StreamEntityBase>();
        private bool _queue1IsPending = true;
        private List<StreamEntityBase> PendingItems { get { return (_queue1IsPending ? _queue1 : _queue2); } }
        private List<StreamEntityBase> ProcessItems { get { return (_queue1IsPending ? _queue2 : _queue1); } }
        private int _lockState = 0; // Idle = 0, Switching = 1, Adding = 2

        static private int _numUpserts;
        static private int _numDeletes;
        static private int _numProcessedItems;
        static private int _numSwitchDirectories;
        static long _reportingRunning;
        static private DateTime _lastReported = DateTime.UtcNow;

        public StreamEntityPersisterPartitionAppendQueueInMemory(IStreamPersister persister, StreamEntityType entityType, int partitionKey, int holdOffBusy = 0, int holdOffIdle = 100)
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
                    pendingItems.Add(item);
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
                while (processItems.Any())
                {
                    int storedItems = 0;
                    IStreamPersisterBatch batch = null;

                    if (_persister != null)
                    {
                        foreach (var item in processItems)
                        {
                            if (_persister.SupportsBatches && batch == null)
                                batch = await _persister.CreateBatch(_entityType);

                            StoredLatency storedLatency;
                            if (item.Operation == StreamOperation.Insert || item.Operation == StreamOperation.Update)
                                storedLatency = await _persister.Upsert(item, batch);
                            else
                                storedLatency = await _persister.Delete(item, batch);
                            ReportLatency(storedLatency);
                            storedItems++;
                            anyDataProcessed = true;

                            Interlocked.Increment(ref _numProcessedItems);
                        }
                    }
                    else
                        Interlocked.Add(ref _numProcessedItems, processItems.Count);

                    if (batch != null)
                    {
                        var storedLatency = await batch.Commit();
                        ReportLatency(storedLatency);
                    }
                    processItems.Clear();
                }

                DisplayLatency();
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

            _queue1IsPending = !_queue1IsPending;
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

                        Console.WriteLine($"In-memory aggregate persister queue: {numProcessedItems / 10} processed items /sec, {numUpserts / 10} upserts /sec, {numDeletes / 10} deletes /sec, {numSwitchedDirectories / 10} switch directories / sec");
                        _lastReported = now;
                    }

                    Task.Delay(100).Wait();
                }
            });
        }

    }
}
