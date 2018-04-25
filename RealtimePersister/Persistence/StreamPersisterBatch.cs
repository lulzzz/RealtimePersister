using SaxoRiskPOC.StreamPersister.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace SaxoRiskPOC.StreamPersister.CosmosDB
{
    public class StreamPersisterBatch : IStreamPersisterBatch
    {
        public List<Task> Tasks { get; } = new List<Task>();

        public async Task Commit()
        {
            await Task.WhenAll(Tasks);
            Tasks.Clear();
        }
    }
}
