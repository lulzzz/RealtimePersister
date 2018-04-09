using System.Collections.Generic;

namespace RealtimePersister.Models.Streams
{
    public class StreamBatch : StreamEntityBase
    {
        public StreamBatch() :
            base("SetInProperty", StreamEntityType.Batch)
        {
        }

        public List<StreamEntityBase> Items { get; } = new List<StreamEntityBase>();
    }
}
