namespace NServiceBus.Azure.Transports.WindowsAzureServiceBus.AcceptanceTests.Addressing
{
    using System;
    using System.Security.Cryptography;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using AcceptanceTesting;
    using AcceptanceTesting.Customization;
    using NServiceBus.AcceptanceTests;
    using NServiceBus.AcceptanceTests.EndpointTemplates;
    using NUnit.Framework;
    using Transport.AzureServiceBus;

    public class When_sending_directly_with_hierarchy_composition_and_secure_connection : NServiceBusAcceptanceTest
    {
        const string NamespaceHierarchyPrefix = "scadapter/";

        [Test]
        public async Task Should_send_and_receive_message() => await Scenario.Define<Context>()
                .WithEndpoint<SourceEndpoint>(b =>
                {
                    b.When(async (bus, c) =>
                    {
                        var sendOptions = new SendOptions();
                        sendOptions.SetDestination($"{Conventions.EndpointNamingConvention(typeof(TargetEndpoint))}@default");
                        await bus.Send(new MyRequest(), sendOptions);
                    });
                })
                .WithEndpoint<TargetEndpoint>()
                .Done(c => c.RequestsReceived == 1)
                .Run();


        public class SourceEndpoint : EndpointConfigurationBuilder
        {
            public SourceEndpoint()
            {
                EndpointSetup<DefaultServer>(c =>
                {
                    var transport = c.ConfigureAzureServiceBus();
                    transport.Composition().UseStrategy<HierarchyComposition>().PathGenerator(path => NamespaceHierarchyPrefix);
                    transport.UseNamespaceAliasesInsteadOfConnectionStrings();
                    transport.Sanitization().UseStrategy<CustomSanitization>();
                });
            }
        }

        public class TargetEndpoint : EndpointConfigurationBuilder
        {
            public TargetEndpoint()
            {
                EndpointSetup<DefaultServer>(c =>
                {
                    var transport = c.ConfigureAzureServiceBus();
                    transport.Composition().UseStrategy<HierarchyComposition>().PathGenerator(path => NamespaceHierarchyPrefix);
                    transport.UseNamespaceAliasesInsteadOfConnectionStrings();
                    transport.Sanitization().UseStrategy<CustomSanitization>();
                });
            }

            class MyRequestHandler : IHandleMessages<MyRequest>
            {
                public Context Context { get; set; }

                public Task Handle(MyRequest message, IMessageHandlerContext context)
                {
                    Context.ReceivedRequest();
                    return Task.FromResult(0);
                }
            }
        }

        class CustomSanitization : ISanitizationStrategy
        {
            public string Sanitize(string entityPathOrName, EntityType entityType)
            {
                var entityPathOrNameMaxLength = 0;

                switch (entityType)
                {
                    case EntityType.Queue:
                    case EntityType.Topic:
                    case EntityType.Subscription:
                    case EntityType.Rule:
                        entityPathOrNameMaxLength = 50;
                        break;
                    default:
                        break;
                }

                if (entityPathOrName.Length > entityPathOrNameMaxLength)
                {
                    var pathWithoutNamespaceHierarchyPrefix = entityPathOrName.Remove(0, NamespaceHierarchyPrefix.Length);
                    entityPathOrName = MD5DeterministicNameBuilder.Build(pathWithoutNamespaceHierarchyPrefix);

                    // sanitization took place, restore namespace hierarchy prefix
                    if (entityType == EntityType.Queue || entityType == EntityType.Topic)
                    {
                        return $"{NamespaceHierarchyPrefix}{entityPathOrName}";
                    }
                }

                return entityPathOrName;
            }

            static class MD5DeterministicNameBuilder
            {
                public static string Build(string input)
                {
                    var inputBytes = Encoding.Default.GetBytes(input);
                    using (var provider = new MD5CryptoServiceProvider())
                    {
                        var hashBytes = provider.ComputeHash(inputBytes);
                        return new Guid(hashBytes).ToString();
                    }
                }
            }

        }

        public class MyRequest : IMessage
        {
        }

        public class MyRequestImpl : MyRequest
        {
        }

        public class MyResponse : IMessage
        {
        }

        class Context : ScenarioContext
        {
            long receivedRequest;

            public long RequestsReceived => Interlocked.Read(ref receivedRequest);

            public void ReceivedRequest() => Interlocked.Increment(ref receivedRequest);
        }
    }
}