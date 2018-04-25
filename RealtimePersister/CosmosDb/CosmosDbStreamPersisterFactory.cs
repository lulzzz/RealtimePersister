using System.Configuration;

namespace RealtimePersister.CosmosDb
{
    public class CosmosDbStreamPersisterFactory : IStreamPersisterFactory
    {
        private static IStreamPersister _persister;

        public IStreamPersister CreatePersister(string database)
        {
            if (_persister == null)
            {
                var uri = ConfigurationManager.AppSettings["EndpointUrl"];
                var key = ConfigurationManager.AppSettings["PrimaryKey"];
                _persister = new CosmosDbStreamPersister(uri, key, database, "syncweek", 100_000);
            }

            return _persister;
        }
    }
}
