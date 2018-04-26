using Newtonsoft.Json;
using ProtoBuf;
using System.Runtime.Serialization;

namespace RealtimePersister.Models.Streams
{
    [DataContract]
    [ProtoContract]
    public class StreamMarket : StreamEntityBase
    {
        public StreamMarket() :
            base("ReferenceData", StreamEntityType.Market)
        {
        }

        [JsonProperty(PropertyName = "name")]
        [DataMember]
        [ProtoMember(7)]
        public string Name { get; set; }

        public int Compare(StreamMarket other)
        {
            int ret = 0;
            ret = Name.CompareTo(other.Name);
            return ret;
        }
    }
}
