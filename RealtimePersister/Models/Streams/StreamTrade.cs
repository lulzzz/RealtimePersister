using System;
using Newtonsoft.Json;
using ProtoBuf;
using System.Runtime.Serialization;

namespace RealtimePersister.Models.Streams
{
    [DataContract]
    [ProtoContract]
    public class StreamTrade : StreamEntityBase
    {
        public StreamTrade() :
            base("Trade", StreamEntityType.Trade)
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
        [JsonProperty(PropertyName = "bid")]
        [DataMember]
        [ProtoMember(9)]
        public bool Bid { get; set; }
        [JsonProperty(PropertyName = "price")]
        [DataMember]
        [ProtoMember(10)]
        public double Price { get; set; }
        [JsonProperty(PropertyName = "volume")]
        [DataMember]
        [ProtoMember(11)]
        public double Volume { get; set; }
        [JsonProperty(PropertyName = "buyer")]
        [DataMember]
        [ProtoMember(12)]
        public string Buyer { get; set; }
        [JsonProperty(PropertyName = "seller")]
        [DataMember]
        [ProtoMember(13)]
        public string Seller { get; set; }

        public override int GetPartitionKey(int partitionCount)
        {
            return (partitionCount > 0 ? Math.Abs(PortfolioId.GetHashCode()) % partitionCount : -1);
        }

        public int Compare(StreamTrade other)
        {
            int ret = 0;
            ret = Id.CompareTo(other.Id);
            if (ret != 0)
                return ret;
            ret = PortfolioId.CompareTo(other.PortfolioId);
            if (ret != 0)
                return ret;
            ret = InstrumentId.CompareTo(other.InstrumentId);
            if (ret != 0)
                return ret;
            ret = (Bid != other.Bid ? Bid ? 1 : -1 : 0);
            if (ret != 0)
                return ret;
            ret = (Price != other.Price ? Price > other.Price ? 1 : -1 : 0);
            if (ret != 0)
                return ret;
            ret = (Volume != other.Volume ? Volume > other.Volume ? 1 : -1 : 0);
            if (ret != 0)
                return ret;
            ret = Buyer.CompareTo(other.Buyer);
            if (ret != 0)
                return ret;
            ret = Seller.CompareTo(other.Seller);
            return ret;
        }
    }
}
