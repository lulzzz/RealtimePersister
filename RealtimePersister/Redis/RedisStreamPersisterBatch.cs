using StackExchange.Redis;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RealtimePersister.Redis
{
    public class RedisStreamPersisterBatch : IStreamPersisterBatch
    {
        public IBatch Batch { get; private set; }
        public List<Task> Tasks { get; } = new List<Task>();

        public RedisStreamPersisterBatch(IDatabase db)
        {
            Batch = db.CreateBatch();
        }

        public async Task Commit()
        {
            Batch.Execute();
            await Task.WhenAll(Tasks);
        }
    }
}
