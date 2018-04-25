using System;
using Newtonsoft.Json;
using ProtoBuf;
using System.Runtime.Serialization;

namespace RealtimePersister.Models.Streams
{
    [DataContract]
    [ProtoContract]
    public class StreamPortfolio : StreamEntityBase
    {
        public StreamPortfolio() :
            base("FrontOffice", StreamEntityType.Portfolio)
        {
        }

        [JsonProperty(PropertyName = "name")]
        [DataMember]
        [ProtoMember(7)]
        public string Name { get; set; }
        [JsonProperty(PropertyName = "balance")]
        [DataMember]
        [ProtoMember(8)]
        public double Balance { get; set; }
        [JsonProperty(PropertyName = "checkrules")]
        [DataMember]
        [ProtoMember(9)]
        public bool CheckRules { get; set; }

        public override int GetPartitionKey(int partitionCount)
        {
            return (partitionCount > 0 ? Math.Abs(Id.GetHashCode()) % partitionCount : -1);
        }

        public int Compare(StreamPortfolio other)
        {
            int ret = 0;
            ret = Id.CompareTo(other.Id);
            if (ret != 0)
                return ret;
            ret = Name.CompareTo(other.Name);
            if (ret != 0)
                return ret;
            ret = (Balance != other.Balance ? Balance > other.Balance ? 1 : -1 : 0);
            if (ret != 0)
                return ret;
            ret = (CheckRules != other.CheckRules ? CheckRules ? 1 : -1 : 0);
            return ret;
        }
    }
}
