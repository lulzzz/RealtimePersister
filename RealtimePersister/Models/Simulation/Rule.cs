using RealtimePersister.Models.Streams;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RealtimePersister.Models.Simulation
{
    public class Rule
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string PortfolioId { get; set; }
        public string Expression { get; set; }

        public StreamRule ToStream(StreamOperation operation)
        {
            return new StreamRule()
            {
                Id = Id,
                PortfolioId = PortfolioId,
                Expression = Expression,
                Operation = operation
            };
        }
    }
}
