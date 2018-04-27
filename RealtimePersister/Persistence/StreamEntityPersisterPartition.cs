using RealtimePersister.Models.Streams;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RealtimePersister
{
    public abstract class StreamEntityPersisterPartition
    {
        protected IStreamPersister _persister;
        protected StreamEntityType _entityType;
        protected int _partitionKey;

        static private double _storedLatencyTime = 0.0;
        static private int _storedLatencyCount = 0;
        static private object _lockObject = new object();
        static private DateTime _lastReported = DateTime.UtcNow;

        public StreamEntityPersisterPartition(IStreamPersister persister, StreamEntityType entityType, int partitionKey)
        {
            _persister = persister;
            _entityType = entityType;
            _partitionKey = partitionKey;
        }

        public abstract Task ProcessStreamItem(StreamEntityBase item);

        static public double GetStoredLatency(StreamEntityBase item)
        {
            return (DateTime.UtcNow - item.Date).TotalMilliseconds; 
        }

        static public double GetStoredLatency(IEnumerable<StreamEntityBase> items)
        {
            var now = DateTime.UtcNow;
            double totalLatency = 0.0;

            foreach (var item in items)
            {
                totalLatency += (now - item.Date).TotalMilliseconds;
            }
            return totalLatency;
        }

        public void ReportLatency(StoredLatency storedLatency)
        {
            lock (_lockObject)
            {
                _storedLatencyCount += storedLatency.NumItems;
                _storedLatencyTime += storedLatency.Time;
            }
        }

        public void DisplayLatency()
        {
            lock (_lockObject)
            {
                if (DateTime.UtcNow > (_lastReported + TimeSpan.FromSeconds(10)))
                {
                    if (_storedLatencyCount > 0)
                        Console.WriteLine($"Stored latency {_storedLatencyTime / _storedLatencyCount } ms average stored latency");
                    _lastReported = DateTime.UtcNow;
                    _storedLatencyTime = 0;
                    _storedLatencyCount = 0;
                }
            }
        }
    }
}
