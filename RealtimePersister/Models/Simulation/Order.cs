using RealtimePersister.Models.Streams;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RealtimePersister.Models.Simulation
{
    public class Order
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string PortfolioId { get; set; }
        public string InstrumentId { get; set; }
        public bool Bid { get; set; }
        public double Price { get; set; }
        public double Volume { get; set; }

        public StreamOrder ToStream(StreamOperation operation)
        {
            return new StreamOrder()
            {
                Id = Id,
                PortfolioId = PortfolioId,
                InstrumentId = InstrumentId,
                Bid = Bid,
                Price = Price,
                Volume = Volume,
                Operation = operation
            };
        }
    }
}
