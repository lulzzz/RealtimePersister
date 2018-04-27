using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RealtimePersister
{
    public class StreamPersisterFactoryCosmosDb : IStreamPersisterFactory
    {
        public IStreamPersister CreatePersister(string database)
        {
            throw new NotImplementedException();
        }
    }
}
