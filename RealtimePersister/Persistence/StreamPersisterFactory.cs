using SaxoRiskPOC.StreamPersister.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace SaxoRiskPOC.StreamPersister.CosmosDB
{
    public class StreamPersisterFactory : IStreamPersisterFactory
    {
        private string _uri;
        private string _key;
        private string _collection;

        public StreamPersisterFactory(string uri, string key, string collection)
        {
            _uri = uri;
            _key = key;
            _collection = collection;
        }
        public IStreamPersister CreatePersister(string database)
        {
            return new StreamPersister(_uri, _key, database, _collection);
        }
    }
}
