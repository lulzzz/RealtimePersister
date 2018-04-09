namespace RealtimePersister
{
    public interface IStreamPersisterFactory
    {
        IStreamPersister CreatePersister(string database);
    }
}
