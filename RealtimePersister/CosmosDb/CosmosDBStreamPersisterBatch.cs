using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RealtimePersister.CosmosDb
{
    public class CosmosDBStreamPersisterBatch : IStreamPersisterBatch
    {
        protected List<Task> Tasks { get; } = new List<Task>();


        public async Task Commit()
        {
            await Task.WhenAll(Tasks);
        }

        public void AddTask(Task task)
        {
            lock (this)
            {
                Tasks.Add(task);
            }
        }
    }
}
