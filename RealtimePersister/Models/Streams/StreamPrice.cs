using System;

namespace RealtimePersister.Models.Streams
{
    public class StreamPrice : StreamEntityBase
    {
        public StreamPrice() :
            base("Price", StreamEntityType.Price)
        {
        }

        public double PriceLatest { get; set; }
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
#if false
            ret = Id.CompareTo(other.Id);
            if (ret != 0)
                return ret;
#endif
            ret = (PriceLatest != other.PriceLatest ? PriceLatest > other.PriceLatest ? 1 : -1 : 0);
            if (ret != 0)
                return ret;
            ret = (PriceDate != other.PriceDate ? PriceDate > other.PriceDate ? 1 : -1 : 0);
            return ret;
        }
    }
}
