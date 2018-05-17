namespace MassTransit.Authority.Tests
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using Contracts;
    using Internals;
    using NUnit.Framework;
    using Subjects;
    using Testing;
    using Util;


    [TestFixture]
    public class Inserting_a_fact
    {
        [Test]
        public async Task Should_publish_an_event_after_the_fact_is_added()
        {
            InMemoryTestHarness harness = new InMemoryTestHarness();

            var sinks = new ConnectionList<IFactSink>();

            InsertFactConsumer InsertFactConsumerFactory()
            {
                var clientFactory = harness.Bus.CreateClientFactory();

                return new InsertFactConsumer(sinks, clientFactory);
            }

            var insertFactHarness = harness.Consumer(InsertFactConsumerFactory, "authority-facts");

            InsertFactTypeConsumer<Customer> InsertCustomerConsumerFactory()
            {
                return new InsertFactTypeConsumer<Customer>();
            }

            var insertCustomerHarness = harness.Consumer(InsertCustomerConsumerFactory, "authority-fact-customer");

            await harness.Start();

            var destinationAddress = new Uri(harness.BaseAddress, "authority-fact-customer");
            var factType = TypeMetadataCache<Customer>.MessageTypeNames.First();

            var endpoint = await harness.Bus.GetSendEndpoint(new Uri(harness.BaseAddress, "authority-facts"));
            await endpoint.Send<ConnectFactTypeSink>(new
            {
                destinationAddress,
                factType
            });
            
            await Task.Delay(1000);

            try
            {
                FactBuilder builder = new FactBuilder();

                var customerHandle = builder.Create<Customer>(new
                {
                    Id = "877123",
                    Name = "Frank's Taco Stand",
                    EstablishedOn = new DateTime(2003, 7, 3, 0, 0, 0, DateTimeKind.Utc)
                });

                var clientFactory = harness.Bus.CreateClientFactory(harness.TestTimeout);

                Guid sessionId = NewId.NextGuid();

                var sessionClient = new AuthoritySessionClient(clientFactory, harness.BaseAddress, sessionId);

                await sessionClient.InsertFact(customerHandle);
            }
            finally
            {
                await harness.Stop();
            }
        }
    }
}