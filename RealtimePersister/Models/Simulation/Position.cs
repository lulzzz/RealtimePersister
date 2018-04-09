using RealtimePersister.Models.Streams;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RealtimePersister.Models.Simulation
{
    public class Position
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string PortfolioId { get; set; }
        public string InstrumentId { get; set; }
        public double Price { get; set; }
        public double Volume { get; set; }

        public StreamPosition ToStream(StreamOperation operation)
        {
            return new StreamPosition()
            {
                Id = Id,
                PortfolioId = PortfolioId,
                InstrumentId = InstrumentId,
                Price = Price,
                Volume = Volume,
                Operation = operation
            };
        }
    }
}
