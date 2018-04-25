using Newtonsoft.Json;
using ProtoBuf;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace RealtimePersister.Models.Streams
{
    [DataContract]
    [ProtoContract]
    public class StreamBatch : StreamEntityBase
    {
        public StreamBatch() :
            base("SetInProperty", StreamEntityType.Batch)
        {
        }

        [JsonProperty(PropertyName = "items")]
        [DataMember]
        [ProtoMember(7)]
        public List<StreamEntityBase> Items { get; } = new List<StreamEntityBase>();
    }
}
