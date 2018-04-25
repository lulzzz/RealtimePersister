using System;
using System.Collections.Generic;
using System.Text;

namespace RealtimePersister.Models.Streams
{
    public enum StreamEntityType
    {
        Command,
        Batch,
        Rule,
        Portfolio,
        Position,
        Order,
        Trade,
        Market,
        Submarket,
        Instrument,
        Price,

        Max // just so we now how many we have
    }
}
