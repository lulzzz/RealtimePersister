using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RealtimePersister
{

    class RedisStreamPersisterFactory : IStreamPersisterFactory
    {
        IStreamPersister IStreamPersisterFactory.CreatePersister(string database)
        {
            throw new NotImplementedException();
        }
    }
}
