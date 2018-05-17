namespace MassTransit.Authority.Tests.Subjects
{
    using System;


    public interface Customer
    {
        string Id { get; }
        string Name { get; }

        /// <summary>
        /// The date when they became a customer (no time component)
        /// </summary>
        DateTime EstablishedOn { get; }
    }
}