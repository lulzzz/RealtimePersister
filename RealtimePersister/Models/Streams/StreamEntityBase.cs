using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace RealtimePersister.Models.Streams
{
    public class StreamEntityBase
    {
        public StreamEntityBase(string streamName, StreamEntityType entityType)
        {
            StreamName = streamName;
            EntityType = entityType;
        }

        public virtual int GetPartitionKey(int partitionCount)
        {
            return -1;
        }

        public string StreamName { get; set; }
        public UInt64 SequenceNumber { get; set; }
        public StreamEntityType EntityType { get; private set; }
        public StreamOperation Operation { get; set; }

        [JsonProperty("id")]
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public DateTime Date { get; set; } = DateTime.UtcNow;
    }
}
