using Newtonsoft.Json;
using ProtoBuf;
using System.Runtime.Serialization;

namespace RealtimePersister.Models.Streams
{
    [DataContract]
    [ProtoContract]
    public class StreamRule : StreamEntityBase
    {
        public StreamRule() :
            base("Rule", StreamEntityType.Rule)
        {
        }

        [JsonProperty(PropertyName = "portfolioid")]
        [DataMember]
        [ProtoMember(7)]
        public string PortfolioId { get; set; }
        [JsonProperty(PropertyName = "expression")]
        [DataMember]
        [ProtoMember(8)]
        public string Expression { get; set; } // this is very vague by intention - we won't execute any roles

        public int Compare(StreamRule other)
        {
            int ret = 0;
            ret = Id.CompareTo(other.Id);
            if (ret != 0)
                return ret;
            ret = PortfolioId.CompareTo(other.PortfolioId);
            if (ret != 0)
                return ret;
            ret = Expression.CompareTo(other.Expression);
            return ret;
        }
    }
}
