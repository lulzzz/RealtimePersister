using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace RealtimePersister.Models.Streams
{
    public class StreamMarket : StreamEntityBase
    {
        public StreamMarket() :
            base("ReferenceData", StreamEntityType.Market)
        {
        }

        public string Name { get; set; }

        public int Compare(StreamMarket other)
        {
            int ret = 0;
#if false
            ret = Id.CompareTo(other.Id);
            if (ret != 0)
                return ret;
#endif
            ret = Name.CompareTo(other.Name);
            return ret;
        }
    }
}
