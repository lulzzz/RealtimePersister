using RealtimePersister.Models.Streams;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RealtimePersister
{
    public interface IStreamPersisterBatch
    {
        Task Commit();
    }

    public interface IStreamPersister
    {
        bool SupportsBatches { get; }

        Task<bool> Connect();
        Task Disconnect();

        Task<IStreamPersisterBatch> CreateBatch(StreamEntityType type);
        Task Upsert(StreamEntityBase item, IStreamPersisterBatch tx = null);
        Task Delete(StreamEntityBase item, IStreamPersisterBatch tx = null);
        Task<IEnumerable<T>> GetAll<T>(StreamEntityType entityType) where T : StreamEntityBase;
        Task<T> GetById<T>(StreamEntityType entityType, string id) where T : StreamEntityBase;
        Task<IEnumerable<T>> GetFromSequenceNumber<T>(StreamEntityType entityType, 
                UInt64 sequenceNumberStart = UInt64.MinValue, UInt64 sequenceNumberEnd = UInt64.MaxValue) where T : StreamEntityBase;
    }
}
