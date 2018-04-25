using RealtimePersister.Models.Streams;
using System;
using System.Collections.Concurrent;
using System.Threading;

namespace RealtimePersister.Models.Simulation
{
    public class Instrument
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string SubmarketId { get; set; }
        public string Name { get; set; }
        public double PriceLatest { get; set; }
        public DateTime PriceDate { get; set; }
        public ConcurrentDictionary<string, Order> Orders { get; } = new ConcurrentDictionary<string, Order>();
        public ConcurrentDictionary<string, Trade> Trades { get; } = new ConcurrentDictionary<string, Trade>();

        public StreamInstrument ToStream(StreamOperation operation)
        {
            return new StreamInstrument()
            {
                Id = Id,
                SubmarketId = SubmarketId,
                Name = Name,
                Operation = operation
            };
        }

        static long _sequencenumber = 0;

        public StreamPrice ToPriceStream(StreamOperation operation)
        {
            return new StreamPrice()
            {
                Id = "Price:" + Id.Substring(11),
                PriceLatest = PriceLatest,
                PriceDate = PriceDate,
                Operation = operation,
                SequenceNumber = (ulong)Interlocked.Increment(ref _sequencenumber)
            };
        }
    }
}
