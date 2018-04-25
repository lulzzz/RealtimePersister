using Newtonsoft.Json;
using ProtoBuf;
using System.Runtime.Serialization;

namespace RealtimePersister.Models.Streams
{
    [DataContract]
    [ProtoContract]
    public class StreamPosition : StreamEntityBase
    {
        public StreamPosition() :
            base("FrontOffice", StreamEntityType.Position)
        {
        }

        [JsonProperty(PropertyName = "portfolioid")]
        [DataMember]
        [ProtoMember(7)]
        public string PortfolioId { get; set; }
        [JsonProperty(PropertyName = "instrumentid")]
        [DataMember]
        [ProtoMember(8)]
        public string InstrumentId { get; set; }
        [JsonProperty(PropertyName = "price")]
        [DataMember]
        [ProtoMember(9)]
        public double Price { get; set; }
        [JsonProperty(PropertyName = "volume")]
        [DataMember]
        [ProtoMember(10)]
        public double Volume { get; set; }

        public int Compare(StreamPosition other)
        {
            int ret = 0;
            ret = PortfolioId.CompareTo(other.PortfolioId);
            if (ret != 0)
                return ret;
            ret = InstrumentId.CompareTo(other.InstrumentId);
            if (ret != 0)
                return ret;
            ret = (Price != other.Price ? Price > other.Price ? 1 : -1 : 0);
            if (ret != 0)
                return ret;
            ret = (Volume != other.Volume ? Volume > other.Volume ? 1 : -1 : 0);
            return ret;
        }
    }
}
