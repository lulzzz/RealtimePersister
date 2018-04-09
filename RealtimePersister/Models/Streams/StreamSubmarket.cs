using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace RealtimePersister.Models.Streams
{
    public class StreamSubmarket : StreamEntityBase
    {
        public StreamSubmarket() :
            base("ReferenceData", StreamEntityType.Submarket)
        {
        }

        public string MarketId { get; set; }
        public string Name { get; set; }

        public int Compare(StreamSubmarket other)
        {
            int ret = 0;
            ret = Id.CompareTo(other.Id);
            if (ret != 0)
                return ret;
            ret = MarketId.CompareTo(other.MarketId);
            if (ret != 0)
                return ret;
            ret = Name.CompareTo(other.Name);
            return ret;
        }
    }
}
