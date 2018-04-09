
namespace RealtimePersister.Models.Streams
{
    public class StreamCommand : StreamEntityBase
    {
        public StreamCommand() :
            base("SetInProperty", StreamEntityType.Command)
        {
        }

        public StreamCommandType Command { get; set; }
    }
}
