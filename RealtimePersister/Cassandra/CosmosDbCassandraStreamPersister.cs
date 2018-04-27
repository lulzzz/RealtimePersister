using Cassandra;
using Cassandra.Mapping;
using RealtimePersister.Models.Streams;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using System.Net.Security;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;

using System.Diagnostics;

namespace RealtimePersister.CosmosDb
{
    public class CosmosDbCassandraStreamPersister : IStreamPersister
    {
        private string _cassandracontactpoint;
        private string _username;
        private string _key;
        private string _keyspace;
        private string _table;
        private static int _cassandraport;
        private int _offerThroughput;
        private ISession _session;
        private IMapper _mapper;

        public bool SupportsBatches => false;

        public CosmosDbCassandraStreamPersister(string CassandraContactPoint, int CassandraPort, string Username, string key, string keyspace, string table, int offerThroughput)
        {
            _cassandracontactpoint = CassandraContactPoint;
            _key = key;
            _username = Username;
            _keyspace = keyspace;
            _table = table;
            _cassandraport = CassandraPort;
            _offerThroughput = offerThroughput;
        }

        public async Task<bool> Connect()
        {
            try
            {
                // Connect to cassandra cluster  (Cassandra API on Azure Cosmos DB supports only TLSv1.2)
                var options = new Cassandra.SSLOptions(SslProtocols.Tls12, true, ValidateServerCertificate);
                options.SetHostNameResolver((ipAddress) => _cassandracontactpoint);
                Cluster cluster = await Task.Run(() => Cluster.Builder().WithCredentials(_username, _key).WithPort(_cassandraport).AddContactPoint(_cassandracontactpoint).WithSSL(options).Build());
                _session = cluster.Connect();

                // Creating KeySpace and table
                _session.Execute($"DROP KEYSPACE IF EXISTS {_keyspace}");
                _session.Execute($"CREATE KEYSPACE {_keyspace} WITH REPLICATION = {{ 'class' : 'NetworkTopologyStrategy', 'datacenter1' : 1 }};");
                Console.WriteLine($"created keyspace {_keyspace}");
                _session.Execute($"CREATE TABLE IF NOT EXISTS {_keyspace}.{_table} (Id text PRIMARY KEY, StreamName text, SequenceNumber bigint, EntityType text, Operation text, Date timestamp) WITH cosmosdb_provisioned_throughput=100000;");
                Console.WriteLine(String.Format($"created table {_keyspace}.{_table}"));

                _session = cluster.Connect(_keyspace);
                // _mapper = new Mapper(_session);

                return (_session != null ? true : false);
            }
            catch (Exception ex)
            {
                Console.Write(ex.Message);
                return false;
            }

        }

        public Task Disconnect()
        {
            _session = null;
            return Task.CompletedTask;
        }

        public Task<IStreamPersisterBatch> CreateBatch(StreamEntityType type)
        {
            return null;
        }

        private int _numUpserts;
        private double _timeSpentUpsert;
        private DateTime _lastReported = DateTime.UtcNow;

        public async Task<StoredLatency> Upsert(StreamEntityBase item, IStreamPersisterBatch batch = null)
        {
            StoredLatency storedLatency;

            var sw = new Stopwatch();
            sw.Start();

            if (batch != null)
            {
                var cosmosDbCassandraBatch = batch as CosmosDBSCassandratreamPersisterBatch;
                cosmosDbCassandraBatch.AddItem(item);
                storedLatency = new StoredLatency() { NumItems = 0, Time = 0.0 };
            }
            else
            {
                /*
                StreamEntity itemtoupsert = new StreamEntity
                {
                    StreamName = item.StreamName,
                    SequenceNumber = (long)item.SequenceNumber,
                    EntityType = item.EntityType.ToString(),
                    Operation = item.Operation.ToString(),
                    Id = item.Id,
                    Date = item.Date 
                };

                await _mapper.InsertAsync<StreamEntity>(itemtoupsert);
                storedLatency = new StoredLatency() { NumItems = 1, Time = StreamEntityPersisterPartition.GetStoredLatency(item) };
                */
                
                try {
                    var itemStmt = _session.Prepare($"insert into {_keyspace}.streamentity(id, streamname, sequencenumber, entitytype, operation, date) values(?, ?, ?, ?, ?, ?)");
                    var statement = itemStmt.Bind(item.Id, item.StreamName, (long)item.SequenceNumber, item.EntityType.ToString(), item.Operation.ToString(), item.Date);
                    await _session.ExecuteAsync(statement);
                    storedLatency = new StoredLatency() { NumItems = 1, Time = StreamEntityPersisterPartition.GetStoredLatency(item) };
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    storedLatency = new StoredLatency() { NumItems = 1, Time = StreamEntityPersisterPartition.GetStoredLatency(item) };
                }
               
            }
            sw.Stop();

            lock (this)
            {
                _numUpserts++;
                _timeSpentUpsert += sw.ElapsedMilliseconds;
                var now = DateTime.UtcNow;

                if (now > (_lastReported + TimeSpan.FromSeconds(10)))
                {
                    if (_numUpserts > 0)
                        Console.WriteLine($"CosmosDB Cassandra Persister; Num upserts {_numUpserts / 10} / sec, Avg time per call {_timeSpentUpsert / _numUpserts} ms.");
                    _numUpserts = 0;
                    _timeSpentUpsert = 0;
                    _lastReported = now;
                }
            }

            return storedLatency;
        }
        async Task<StoredLatency> IStreamPersister.Delete(StreamEntityBase item, IStreamPersisterBatch tx)
        {
            var itemStmt = _session.Prepare($"delete from streamentity where id = '{item.Id}'");
            var batchStmt = new BatchStatement()
                .Add(itemStmt.Bind(item.Id));
            await _session.ExecuteAsync(batchStmt);
            return new StoredLatency() { NumItems = 1, Time = StreamEntityPersisterPartition.GetStoredLatency(item) };
        }
        public async Task<IEnumerable<T>> GetAll<T>(StreamEntityType entityType) where T : StreamEntityBase
        {
            //return Task.FromResult<IEnumerable<T>>;

            var items = new List<T>();
            items = null;

            return items;
        }

        public async Task<T> GetById<T>(StreamEntityType entityType, string id) where T : StreamEntityBase
        {
            return null;
        }

        public async Task<IEnumerable<T>> GetFromSequenceNumber<T>(StreamEntityType entityType, ulong sequenceNumberStart = 0, ulong sequenceNumberEnd = ulong.MaxValue) where T : StreamEntityBase
        {
            var items = new List<T>();
            items = null;

            return items;
        }

        public static bool ValidateServerCertificate(
            object sender,
            X509Certificate certificate,
            X509Chain chain,
            SslPolicyErrors sslPolicyErrors)
        {
            if (sslPolicyErrors == SslPolicyErrors.None)
                return true;

            Console.WriteLine("Certificate error: {0}", sslPolicyErrors);
            // Do not allow this client to communicate with unauthenticated servers.
            return false;
        }

    }

    // Test only
    /*
    public class StreamEntity
    {
        public string StreamName { get; set; }
        public long SequenceNumber { get; set; }
        public string EntityType { get; set; }
        public string Operation { get; set; }
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public DateTime Date { get; set; } = DateTime.UtcNow;
    }
    */
}
