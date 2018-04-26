using Newtonsoft.Json;
using ProtoBuf;
using System.Runtime.Serialization;

namespace RealtimePersister.Models.Streams
{
    [DataContract]
    [ProtoContract]
    public class StreamCommand : StreamEntityBase
    {
        public StreamCommand() :
            base("SetInProperty", StreamEntityType.Command)
        {
        }

        [JsonProperty(PropertyName = "command")]
        [DataMember]
        [ProtoMember(7)]
        public StreamCommandType Command { get; set; }
    }
}
