
namespace RealtimePersister.Redis
{
    public class StreamPersisterFactory : IStreamPersisterFactory
    {
        private string _address;

        public StreamPersisterFactory(string address)
        {
            _address = address;
        }
        public IStreamPersister CreatePersister(string database)
        {
            return new RedisStreamPersister(database, _address);
        }
    }
}
