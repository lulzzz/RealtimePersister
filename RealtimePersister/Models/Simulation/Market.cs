using RealtimePersister.Models.Streams;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RealtimePersister.Models.Simulation
{
    public class Market
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string Name { get; set; }
        public ConcurrentDictionary<string, Submarket> SubmarketsById { get; } = new ConcurrentDictionary<string, Submarket>();
        public ConcurrentDictionary<string, string> SubmarketsByName { get; } = new ConcurrentDictionary<string, string>();

        public StreamMarket ToStream(StreamOperation operation)
        {
            return new StreamMarket()
            {
                Id = Id,
                Name = Name,
                Operation = operation
            };
        }
    }
}
