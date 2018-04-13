using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace RealtimePersister.Models.Streams
{
    public class StreamInstrument : StreamEntityBase
    {
        public StreamInstrument() :
            base("ReferenceData", StreamEntityType.Instrument)
        {
        }

        public string SubmarketId { get; set; }
        public string Name { get; set; }
        // Certainly needs at least instrument type here and much more (but not now)

        public override int GetPartitionKey(int partitionCount)
        {
            return (partitionCount > 0 ? Math.Abs(Id.GetHashCode()) % partitionCount : -1);
        }

        public int Compare(StreamInstrument other)
        {
            int ret = 0;
#if false
            ret = Id.CompareTo(other.Id);
            if (ret != 0)
                return ret;
#endif
            ret = SubmarketId.CompareTo(other.SubmarketId);
            if (ret != 0)
                return ret;
            ret = Name.CompareTo(other.Name);
            return ret;
        }
    }
}
