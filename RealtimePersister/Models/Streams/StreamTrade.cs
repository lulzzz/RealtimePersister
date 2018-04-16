using System;

namespace RealtimePersister.Models.Streams
{
    public class StreamTrade : StreamEntityBase
    {
        public StreamTrade() :
            base("Trade", StreamEntityType.Trade)
        {
        }

        public string PortfolioId { get; set; }
        public string InstrumentId { get; set; }
        public bool Bid { get; set; }
        public double Price { get; set; }
        public double Volume { get; set; }
        public string Buyer { get; set; }
        public string Seller { get; set; }

        public override int GetPartitionKey(int partitionCount)
        {
            return (partitionCount > 0 ? Math.Abs(PortfolioId.GetHashCode()) % partitionCount : -1);
        }

        public int Compare(StreamTrade other)
        {
            int ret = 0;
#if false
            ret = Id.CompareTo(other.Id);
            if (ret != 0)
                return ret;
#endif
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
