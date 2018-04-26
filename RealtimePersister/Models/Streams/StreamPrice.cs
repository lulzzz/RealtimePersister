using System;
using Newtonsoft.Json;
using ProtoBuf;
using System.Runtime.Serialization;
using System.Collections.Generic;

namespace RealtimePersister.Models.Streams
{
    [DataContract]
    [ProtoContract]
    public class StreamPrice : StreamEntityBase
    {
        public StreamPrice() :
            base("Price", StreamEntityType.Price)
        {
        }

        [JsonProperty(PropertyName = "pricelatest")]
        [DataMember]
        [ProtoMember(7)]
        public double PriceLatest { get; set; }
        [JsonProperty(PropertyName = "pricedate")]
        [DataMember]
        [ProtoMember(8)]
        public DateTime PriceDate { get; set; }
        // these are not needed now
#if false
        public double PriceLow { get; set; }
        public double PriceHigh { get; set; }
        public double PriceOpen { get; set; }
        public double PriceClose { get; set; }
        public double PriceBid { get; set; }
        public double VolumeBid { get; set; }
        public double PriceAsk { get; set; }
        public double VolumeAsk { get; set; }
        public double LastTradeVolume { get; set; }
        public double LastTradeDate { get; set; }
#endif

        public override int GetPartitionKey(int partitionCount)
        {
            return (partitionCount > 0 ? Math.Abs(Id.GetHashCode()) % partitionCount : -1);
        }

        public int Compare(StreamPrice other)
        {
            int ret = 0;
            ret = (PriceLatest != other.PriceLatest ? PriceLatest > other.PriceLatest ? 1 : -1 : 0);
            if (ret != 0)
                return ret;
            ret = (PriceDate != other.PriceDate ? PriceDate > other.PriceDate ? 1 : -1 : 0);
            return ret;
        }

        public override Dictionary<string, object> ToKeyValueDictionary()
        {
            var dict = base.ToKeyValueDictionary();
            dict["pricelatest"] = PriceLatest;
            dict["pricedate"] = PriceDate;
            return dict;
        }
    }
}
