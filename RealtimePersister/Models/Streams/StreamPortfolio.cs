using System;

namespace RealtimePersister.Models.Streams
{
    public class StreamPortfolio : StreamEntityBase
    {
        public StreamPortfolio() :
            base("FrontOffice", StreamEntityType.Portfolio)
        {
        }

        public string Name { get; set; }
        public double Balance { get; set; }
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
