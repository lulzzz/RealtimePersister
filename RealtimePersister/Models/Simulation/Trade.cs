using RealtimePersister.Models.Streams;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RealtimePersister.Models.Simulation
{
    public class Trade
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string PortfolioId { get; set; }
        public string InstrumentId { get; set; }
        public bool Bid { get; set; }
        public double Price { get; set; }
        public double Volume { get; set; }
        public string Buyer { get; set; }
        public string Seller { get; set; }

        public StreamTrade ToStream(StreamOperation operation)
        {
            return new StreamTrade()
            {
                Id = Id,
                PortfolioId = PortfolioId,
                InstrumentId = InstrumentId,
                Bid = Bid,
                Price = Price,
                Volume = Volume,
                Buyer = Buyer,
                Seller = Seller,
                Operation = operation
            };
        }
    }
}
