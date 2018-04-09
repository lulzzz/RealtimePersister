using RealtimePersister.Models.Streams;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RealtimePersister.Models.Simulation
{
    public class Portfolio
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string Name { get; set; }
        public double Balance { get; set; }
        public bool CheckRules { get; set; }
        public ConcurrentDictionary<string, Position> Positions { get; } = new ConcurrentDictionary<string, Position>();
        public List<Rule> Rules { get; } = new List<Rule>();

        public StreamPortfolio ToStream(StreamOperation operation)
        {
            return new StreamPortfolio()
            {
                Id = Id,
                Name = Name,
                Balance = Balance,
                CheckRules = CheckRules,
                Operation = operation
            };
        }
    }
}
