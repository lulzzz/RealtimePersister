using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace RealtimePersister.Models.Streams
{
    public class StreamRule : StreamEntityBase
    {
        public StreamRule() :
            base("Rule", StreamEntityType.Rule)
        {
        }

        public string PortfolioId { get; set; }
        public string Expression { get; set; } // this is very vague by intention - we won't execute any roles

        public int Compare(StreamRule other)
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
            ret = Expression.CompareTo(other.Expression);
            return ret;
        }
    }
}
