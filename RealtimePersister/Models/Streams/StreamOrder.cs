using System;

namespace RealtimePersister.Models.Streams
{
    public class StreamOrder : StreamEntityBase
    {
        public StreamOrder() :
            base("FrontOffice", StreamEntityType.Order)
        {
        }

        public string PortfolioId { get; set; }
        public string InstrumentId { get; set; }
        public bool Bid { get; set; }
        public double Price { get; set; }
        public double Volume { get; set; }

        public override int GetPartitionKey(int partitionCount)
        {
            return (partitionCount > 0 ? Math.Abs(PortfolioId.GetHashCode()) % partitionCount : -1);
        }

        public int Compare(StreamOrder other)
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
            return ret;
        }
    }
}
