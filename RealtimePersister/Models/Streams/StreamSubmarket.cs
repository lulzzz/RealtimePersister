using Newtonsoft.Json;
using ProtoBuf;
using System.Runtime.Serialization;


namespace RealtimePersister.Models.Streams
{
    [DataContract]
    [ProtoContract]
    public class StreamSubmarket : StreamEntityBase
    {
        public StreamSubmarket() :
            base("ReferenceData", StreamEntityType.Submarket)
        {
        }

        [JsonProperty(PropertyName = "marketid")]
        [DataMember]
        [ProtoMember(7)]
        public string MarketId { get; set; }
        [JsonProperty(PropertyName = "name")]
        [DataMember]
        [ProtoMember(8)]
        public string Name { get; set; }

        public int Compare(StreamSubmarket other)
        {
            int ret = 0;
            ret = MarketId.CompareTo(other.MarketId);
            if (ret != 0)
                return ret;
            ret = Name.CompareTo(other.Name);
            return ret;
        }
    }
}
