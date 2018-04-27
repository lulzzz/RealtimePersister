using System.Configuration;

namespace RealtimePersister.CosmosDb
{
    public class CosmosDbCassandraStreamPersisterFactory : IStreamPersisterFactory
    {
        private static IStreamPersister _persister;

        public IStreamPersister CreatePersister(string keyspace)
        {
            if (_persister == null)
            {
                var CassandraContactPoint = ConfigurationManager.AppSettings["CassandraUrl"];
                var username = ConfigurationManager.AppSettings["CassandraUsername"];
                var key = ConfigurationManager.AppSettings["CassandraKey"];
                _persister = new CosmosDbCassandraStreamPersister(CassandraContactPoint, 10350, username, key, "syncweek", "StreamEntity", 100000);
            }

            return _persister;
        }
    }
}
