using System;
using Newtonsoft.Json;
using ProtoBuf;
using System.Runtime.Serialization;

namespace RealtimePersister.Models.Streams
{
    [DataContract]
    [ProtoContract]
    public class StreamInstrument : StreamEntityBase
    {
        public StreamInstrument() :
            base("ReferenceData", StreamEntityType.Instrument)
        {
        }

        [JsonProperty(PropertyName = "submarketid")]
        [DataMember]
        [ProtoMember(7)]
        public string SubmarketId { get; set; }
        [JsonProperty(PropertyName = "name")]
        [DataMember]
        [ProtoMember(8)]
        public string Name { get; set; }
        // Certainly needs at least instrument type here and much more (but not now)

        public override int GetPartitionKey(int partitionCount)
        {
            return (partitionCount > 0 ? Math.Abs(Id.GetHashCode()) % partitionCount : -1);
        }

        public int Compare(StreamInstrument other)
        {
            int ret = 0;
            ret = SubmarketId.CompareTo(other.SubmarketId);
            if (ret != 0)
                return ret;
            ret = Name.CompareTo(other.Name);
            return ret;
        }
    }
}
