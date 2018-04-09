using RealtimePersister.Models.Streams;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RealtimePersister.Models.Simulation
{
    public class Submarket
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string MarketId { get; set; }
        public string Name { get; set; }
        public ConcurrentDictionary<string, Instrument> InstrumentsById { get; } = new ConcurrentDictionary<string, Instrument>();
        public ConcurrentDictionary<string, string> InstrumentsByName { get; } = new ConcurrentDictionary<string, string>();
        public int NumInstruments { get; set; }
        public StreamSubmarket ToStream(StreamOperation operation)
        {
            return new StreamSubmarket()
            {
                Id = Id,
                MarketId = MarketId,
                Name = Name,
                Operation = operation
            };
        }
    }
}
