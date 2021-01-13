namespace NServiceBus.Azure.Transports.WindowsAzureServiceBus.AcceptanceTests.Routing
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Threading.Tasks;
    using AcceptanceTesting;
    using AcceptanceTesting.Customization;
    using AzureServiceBus.AcceptanceTests.Infrastructure;
    using Features;
    using Microsoft.ServiceBus.Messaging;
    using NServiceBus.AcceptanceTests;
    using NServiceBus.AcceptanceTests.EndpointTemplates;
    using NUnit.Framework;
    using ObjectBuilder;
    using Transport;
    using Transport.AzureServiceBus;

    public class When_incoming_s_contains_typed_properties : NServiceBusAcceptanceTest
    {
        [SetUp]
        public void Setup()
        {
            factory = MessagingFactory.CreateFromConnectionString(connectionString);
        }

        [TearDown]
        public void TearDown()
        {
            factory.Close();
        }


        [Test]
        public async Task Should_be_preserved()
        {
            var satellitePath = $"{Conventions.EndpointNamingConvention(typeof(Receiver))}-satellite";
            var now = DateTime.UtcNow;

            var context = await Scenario.Define<Context>()
                .WithEndpoint<Receiver>(b => b.When(async (s, ctx) =>
                {
                    MessageSender sender = null;
                    try
                    {
                        sender = await factory.CreateMessageSenderAsync(satellitePath);
                        var message = new BrokeredMessage();
                        message.Properties["$AcceptanceTesting.TestRunId"] = ctx.TestRunId;
                        message.Properties[BrokeredMessageHeaders.EstimatedMessageSize] = 100;
                        message.Properties[BrokeredMessageHeaders.TransportEncoding] = "application/octet-stream";
                        message.Properties["CustomIntHeader"] = 200;
                        message.Properties["CustomBoolHeader"] = true;
                        message.Properties["CustomDateTimeHeader"] = now;
                        message.Properties["CustomStringHeader"] = "Custom";
                        message.Properties["CustomEmptyStringHeader"] = "";
                        message.Properties["CustomNullStringHeader"] = null;
                        message.Properties["CustomNullHeader"] = null;
                        await sender.SendAsync(message);
                    }
                    finally
                    {
                        await sender.CloseAsync();
                    }
                }))
                .Done(ctx => ctx.Received)
                .Run();

            Assert.That(context.Headers, Does.Not.ContainKey(BrokeredMessageHeaders.EstimatedMessageSize));
            Assert.That(context.Headers, Does.Not.ContainKey(BrokeredMessageHeaders.TransportEncoding));
            Assert.That(context.Headers, Does.ContainKey("CustomIntHeader").And.ContainValue("200"));
            Assert.That(context.Headers, Does.ContainKey("CustomBoolHeader").And.ContainValue("True"));
            Assert.That(context.Headers, Does.ContainKey("CustomDateTimeHeader").And.ContainValue(now.ToString(CultureInfo.InvariantCulture)));
            Assert.That(context.Headers, Does.ContainKey("CustomStringHeader").And.ContainValue("Custom"));
            Assert.That(context.Headers, Does.ContainKey("CustomEmptyStringHeader").And.ContainValue(""));
            Assert.That(context.Headers, Does.ContainKey("CustomNullStringHeader").And.ContainValue(null));
            Assert.That(context.Headers, Does.ContainKey("CustomNullHeader").And.ContainValue(null));
        }

        MessagingFactory factory;

        static readonly string connectionString = TestUtility.DefaultConnectionString;

        class Context : ScenarioContext
        {
            public bool Received { get; set; }
            public Dictionary<string, string> Headers { get; set; } = new Dictionary<string, string>();
        }


        class Receiver : EndpointConfigurationBuilder
        {
            public Receiver()
            {
                EndpointSetup<DefaultServer>(config => { });
            }

            class SatelliteFeature : Feature
            {
                public SatelliteFeature()
                {
                    EnableByDefault();
                }

                protected override void Setup(FeatureConfigurationContext context)
                {
                    context.AddSatelliteReceiver("customSatellite", $"{Conventions.EndpointNamingConvention(typeof(Receiver))}-satellite", new PushRuntimeSettings(1), DefaultRecoverabilityPolicy.Invoke, Handle);
                }

                static Task Handle(IBuilder builder, MessageContext context)
                {
                    var scenarioContext = builder.Build<Context>();
                    if (context.Headers["$AcceptanceTesting.TestRunId"] != scenarioContext.TestRunId.ToString())
                    {
                        return Task.CompletedTask;
                    }

                    scenarioContext.Headers = context.Headers;
                    scenarioContext.Received = true;
                    return Task.CompletedTask;
                }
            }
        }
    }
}