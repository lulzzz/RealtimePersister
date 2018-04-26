using RealtimePersister.Models.Streams;
using StackExchange.Redis;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RealtimePersister.Redis
{
    public class RedisStreamPersisterBatch : IStreamPersisterBatch
    {
        public IBatch Batch { get; private set; }
        protected List<Task> Tasks { get; } = new List<Task>();
        protected List<StreamEntityBase> Items { get; } = new List<StreamEntityBase>();

        public RedisStreamPersisterBatch(IDatabase db)
        {
            Batch = db.CreateBatch();
        }

        public async Task<StoredLatency> Commit()
        {
            Batch.Execute();
            await Task.WhenAll(Tasks);
            return new StoredLatency() { NumItems = Items.Count, Time = StreamEntityPersisterPartition.GetStoredLatency(Items) };
        }

        public void AddItem(StreamEntityBase item, Task task)
        {
            if (item != null && task != null)
            {
                lock (this)
                {
                    Items.Add(item);
                    Tasks.Add(task);
                }
            }
        }
    }
}
