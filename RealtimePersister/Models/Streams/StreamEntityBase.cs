using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Newtonsoft.Json;
using ProtoBuf;
using System.Runtime.Serialization;

namespace RealtimePersister.Models.Streams
{
    [DataContract]
    [KnownType(typeof(StreamBatch))]
    [KnownType(typeof(StreamCommand))]
    [KnownType(typeof(StreamInstrument))]
    [KnownType(typeof(StreamMarket))]
    [KnownType(typeof(StreamOrder))]
    [KnownType(typeof(StreamPortfolio))]
    [KnownType(typeof(StreamPosition))]
    [KnownType(typeof(StreamPrice))]
    [KnownType(typeof(StreamRule))]
    [KnownType(typeof(StreamSubmarket))]
    [KnownType(typeof(StreamTrade))]
    [ProtoContract]
    [ProtoInclude(100, typeof(StreamBatch))]
    [ProtoInclude(101, typeof(StreamCommand))]
    [ProtoInclude(102, typeof(StreamInstrument))]
    [ProtoInclude(103, typeof(StreamMarket))]
    [ProtoInclude(104, typeof(StreamOrder))]
    [ProtoInclude(105, typeof(StreamPortfolio))]
    [ProtoInclude(106, typeof(StreamPosition))]
    [ProtoInclude(107, typeof(StreamPrice))]
    [ProtoInclude(108, typeof(StreamRule))]
    [ProtoInclude(109, typeof(StreamSubmarket))]
    [ProtoInclude(110, typeof(StreamTrade))]
    public class StreamEntityBase
    {
        private static JsonSerializerSettings _settings = new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.All };

        public StreamEntityBase(string streamName, StreamEntityType entityType)
        {
            StreamName = streamName;
            EntityType = entityType;
        }

        public virtual int GetPartitionKey(int partitionCount)
        {
            return -1;
        }

        [JsonProperty(PropertyName = "streamname")]
        [DataMember]
        [ProtoMember(1)]
        public string StreamName { get; set; }
        [JsonProperty(PropertyName = "sequencenumber")]
        [DataMember]
        [ProtoMember(2)]
        public UInt64 SequenceNumber { get; set; }
        [JsonProperty(PropertyName = "entitytype")]
        [DataMember]
        [ProtoMember(3)]
        public StreamEntityType EntityType { get; private set; }
        [JsonProperty(PropertyName = "operation")]
        [DataMember]
        [ProtoMember(4)]
        public StreamOperation Operation { get; set; }
        [JsonProperty(PropertyName = "id")]
        [DataMember]
        [ProtoMember(5)]
        public string Id { get; set; } = Guid.NewGuid().ToString();
        [JsonProperty(PropertyName = "date")]
        [DataMember]
        [ProtoMember(6)]
        public DateTime Date { get; set; } = DateTime.UtcNow;

        public string ToJsonString()
        {
            return JsonConvert.SerializeObject(this, this.GetType(), _settings);
        }

        public byte[] ToJsonByteArray()
        {
            return Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(this, this.GetType(), _settings));
        }

        static public StreamEntityBase FromJsonString(string json)
        {
            return JsonConvert.DeserializeObject<StreamEntityBase>(json, _settings);
        }

        static public StreamEntityBase FromJsonByteArray(byte[] array, int offset = 0, int length = 0)
        {
            return JsonConvert.DeserializeObject<StreamEntityBase>(Encoding.UTF8.GetString(array, offset, (length > 0 ? length : array.Length)), _settings);
        }

        public string ToProtoBufString()
        {
            using (var ms = new MemoryStream())
            {
                Serializer.Serialize(ms, this);
                ms.Position = 0;
                return Encoding.UTF8.GetString(ms.GetBuffer());
            }
        }

        public byte[] ToProtoBufByteArray()
        {
            using (var ms = new MemoryStream())
            {
                Serializer.Serialize(ms, this);
                using (var s = new BinaryReader(ms))
                {
                    ms.Position = 0;
                    var data = s.ReadBytes((int)ms.Length);
                    return data;
                }
            }
        }

        public void ToProtoBufStream(Stream s)
        {
            Serializer.Serialize(s, this);
        }

        static public StreamEntityBase FromProtoBufString(string protoBuf)
        {
            using (var ms = new MemoryStream(Encoding.UTF8.GetBytes(protoBuf)))
            {
                ms.Position = 0;
                return Serializer.Deserialize<StreamEntityBase>(ms);
            }
        }

        static public StreamEntityBase FromProtoBufByteArray(byte[] array, int offset = 0, int length = 0)
        {
            try
            {
                using (var ms = new MemoryStream(array, offset, (length > 0 ? length : array.Length)))
                {
                    ms.Position = 0;
                    return Serializer.Deserialize<StreamEntityBase>(ms);
                }
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        static public StreamEntityBase FromProtoBufStream(Stream s)
        {
            return Serializer.Deserialize<StreamEntityBase>(s);
        }

        public virtual Dictionary<string, object> ToKeyValueDictionary()
        {
            var dict = new Dictionary<string, object>();
            dict["streamname"] = StreamName;
            dict["sequencenumber"] = SequenceNumber;
            dict["entitytype"] = EntityType;
            dict["operation"] = Operation;
            dict["id"] = Id;
            dict["date"] = Date;
            return dict;
        }
}
}
