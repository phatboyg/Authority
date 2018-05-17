namespace MassTransit.Authority
{
    using System;
    using System.Collections.Generic;


    public interface IFactSink
    {
        (bool,Uri) Matches(IEnumerable<string> factTypes);
    }
}