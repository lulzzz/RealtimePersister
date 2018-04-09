using System;
using System.Collections.Generic;
using System.Text;

namespace RealtimePersister.Models.Streams
{
    public enum StreamCommandType
    {
        StartSnapshot,
        EndSnapshot,
        Clear
    }
}
