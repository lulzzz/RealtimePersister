namespace RealtimePersister.Models.Streams
{
    public class StreamPosition : StreamEntityBase
    {
        public StreamPosition() :
            base("FrontOffice", StreamEntityType.Position)
        {
        }

        public string PortfolioId { get; set; }
        public string InstrumentId { get; set; }
        public double Price { get; set; }
        public double Volume { get; set; }

        public int Compare(StreamPosition other)
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
            ret = (Price != other.Price ? Price > other.Price ? 1 : -1 : 0);
            if (ret != 0)
                return ret;
            ret = (Volume != other.Volume ? Volume > other.Volume ? 1 : -1 : 0);
            return ret;
        }
    }
}
