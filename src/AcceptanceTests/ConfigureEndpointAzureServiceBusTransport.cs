using System;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using NServiceBus;
using NServiceBus.AcceptanceTesting.Support;
using NServiceBus.AcceptanceTests.ApiExtension;
using NServiceBus.AcceptanceTests.BestPractices;
using NServiceBus.AcceptanceTests.Routing;
using NServiceBus.AcceptanceTests.Routing.AutomaticSubscriptions;
using NServiceBus.AcceptanceTests.Sagas;
using NServiceBus.AcceptanceTests.Versioning;
using NServiceBus.Azure.Transports.WindowsAzureServiceBus.AcceptanceTests.Routing;
using NServiceBus.AzureServiceBus;
using NServiceBus.AzureServiceBus.AcceptanceTests.Infrastructure;
using NServiceBus.AzureServiceBus.Addressing;

public class ConfigureEndpointAzureServiceBusTransport : IConfigureEndpointTestExecution
{
    public Task Configure(string endpointName, EndpointConfiguration config, RunSettings settings)
    {
        var connectionString = settings.Get<string>("Transport.ConnectionString");
        var topology = Environment.GetEnvironmentVariable("AzureServiceBusTransport.Topology", EnvironmentVariableTarget.User);
        topology = topology ?? Environment.GetEnvironmentVariable("AzureServiceBusTransport.Topology");

        var transportConfig = config.UseTransport<AzureServiceBusTransport>().ConnectionString(connectionString);
        
        if (topology == "ForwardingTopology")
        {
            transportConfig.UseTopology<ForwardingTopology>();
        }
        else
        {
            transportConfig.UseTopology<EndpointOrientedTopology>()
                .RegisterPublisherForType("MultipleVersionsOfAMessageIsPublished.V2Publisher", typeof(When_multiple_versions_of_a_message_is_published.V1Event))
                .RegisterPublisherForType("MultipleVersionsOfAMessageIsPublished.V2Publisher", typeof(When_multiple_versions_of_a_message_is_published.V2Event))
                .RegisterPublisherForType("RepliesToMessagePublishedByASaga.SagaEndpoint", typeof(When_replies_to_message_published_by_a_saga.DidSomething))
                .RegisterPublisherForType("StartedByBaseEventFromOtherSaga.Publisher", typeof(When_started_by_base_event_from_other_saga.SomethingHappenedEvent))
                .RegisterPublisherForType("StartedByEventFromAnotherSaga.SagaThatPublishesAnEvent", typeof(When_started_by_event_from_another_saga.SomethingHappenedEvent))
                .RegisterPublisherForType("BaseEventFrom2Publishers.Publisher1", typeof(When_base_event_from_2_publishers.DerivedEvent1))
                .RegisterPublisherForType("BaseEventFrom2Publishers.Publisher2", typeof(When_base_event_from_2_publishers.DerivedEvent2))
                .RegisterPublisherForType("Publishing.Publisher3", typeof(When_publishing.IFoo))
                .RegisterPublisherForType("Publishing.Publisher", typeof(When_publishing.MyEvent))
                .RegisterPublisherForType("PublishingAnEventImplementingTwoUnrelatedInterfaces.Publisher", typeof(When_publishing_an_event_implementing_two_unrelated_interfaces.CompositeEvent))
                .RegisterPublisherForType("PublishingAnInterface.Publisher", typeof(When_publishing_an_interface.MyEvent))
                .RegisterPublisherForType("PublishingUsingRootType.Publisher", typeof(When_publishing_using_root_type.EventMessage))
                .RegisterPublisherForType("PublishingWithOnlyLocalMessagehandlers.CentralizedStoragePublisher", typeof(When_publishing_with_only_local_messagehandlers.EventHandledByLocalEndpoint))
                .RegisterPublisherForType("SubscribingToAPolymorphicEvent.Publisher", typeof(When_subscribing_to_a_polymorphic_event.MyEvent))
                .RegisterPublisherForType("StartingAnEndpointWithASaga.Subscriber", typeof(When_starting_an_endpoint_with_a_saga.MyEvent))
                .RegisterPublisherForType("StartingAnEndpointWithASaga.Subscriber", typeof(When_starting_an_endpoint_with_a_saga.MyEventBase))
                .RegisterPublisherForType("PublishingCommandBestpracticesDisabled.Endpoint", typeof(When_publishing_command_bestpractices_disabled.MyCommand))
                .RegisterPublisherForType("PublishingCommandBestpracticesDisabled.Endpoint", typeof(When_publishing_command_bestpractices_disabled.MyEvent))
                .RegisterPublisherForType("PublishingCommandBestpracticesDisabledOnEndpoint.Endpoint", typeof(When_publishing_command_bestpractices_disabled_on_endpoint.MyCommand))
                .RegisterPublisherForType("PublishingCommandBestpracticesDisabledOnEndpoint.Endpoint", typeof(When_publishing_command_bestpractices_disabled_on_endpoint.MyEvent))
                .RegisterPublisherForType("SendingEventsBestpracticesDisabled.Endpoint", typeof(When_sending_events_bestpractices_disabled.MyEvent))
                .RegisterPublisherForType("SendingEventsBestpracticesDisabledOnEndpoint.Endpoint", typeof(When_sending_events_bestpractices_disabled_on_endpoint.MyEvent))
                .RegisterPublisherForType("SubscribingToCommandBestpracticesDisabledOnEndpoint.Endpoint", typeof(When_subscribing_to_command_bestpractices_disabled_on_endpoint.MyCommand))
                .RegisterPublisherForType("SubscribingToCommandBestpracticesDisabledOnEndpoint.Endpoint", typeof(When_subscribing_to_command_bestpractices_disabled_on_endpoint.MyEvent))
                .RegisterPublisherForType("UnsubscribingToCommandBestpracticesDisabledOnEndpoint.Endpoint", typeof(When_unsubscribing_to_command_bestpractices_disabled_on_endpoint.MyCommand))
                .RegisterPublisherForType("UnsubscribingToCommandBestpracticesDisabledOnEndpoint.Endpoint", typeof(When_unsubscribing_to_command_bestpractices_disabled_on_endpoint.MyEvent))
                .RegisterPublisherForType("ExtendingThePublishApi.Publisher", typeof(When_extending_the_publish_api.MyEvent))
                .RegisterPublisherForType("SubscribingToBaseAndDerivedPolymorphicEventsWithForwardingTopology.Publisher", typeof(When_subscribing_to_base_and_derived_polymorphic_events_with_forwarding_topology.BaseEvent))
                .RegisterPublisherForType("SubscribingToBaseAndDerivedPolymorphicEventsWithForwardingTopology.Publisher", typeof(When_subscribing_to_base_and_derived_polymorphic_events_with_forwarding_topology.DerivedEvent))
                .RegisterPublisherForType("PublishingToScaledOutSubscribersOnMulticastTransports.Publisher", typeof(When_publishing_to_scaled_out_subscribers_on_multicast_transports.MyEvent))
                .RegisterPublisherForType("MultiSubscribingToAPolymorphicEventOnMulticastTransports.Publisher1", typeof(When_multi_subscribing_to_a_polymorphic_event_on_multicast_transports.MyEvent1))
                .RegisterPublisherForType("MultiSubscribingToAPolymorphicEventOnMulticastTransports.Publisher2", typeof(When_multi_subscribing_to_a_polymorphic_event_on_multicast_transports.MyEvent2));
        }

        // TODO: remove with config API on .UseStrategy<ValidateAndHashIfNeeded>().WithV6Compatibility()
        // similar to what the original EndpointOrientedSanitization was doing
        Func<string, string> sanitizer = pathOrName => new Regex(@"[^a-zA-Z0-9\-\._]").Replace(pathOrName, "");
 
        transportConfig.Sanitization()
            .QueuePathSanitization(x => sanitizer(x))
            .TopicPathSanitization(x => sanitizer(x))
            .SubscriptionNameSanitization(x => sanitizer(x))
            .RuleNameSanitization(x => sanitizer(x))
            .UseStrategy<ValidateAndHashIfNeeded>()
            .Hash(pathOrName => MD5DeterministicNameBuilder.Build(pathOrName));

        config.RegisterComponents(c => { c.ConfigureComponent<TestIndependenceMutator>(DependencyLifecycle.SingleInstance); });

        config.Pipeline.Register("TestIndependenceBehavior", typeof(TestIndependenceSkipBehavior),
            "Skips messages not created during the current test.");

        return Task.FromResult(0);
    }

    // TODO: remove with config API on .UseStrategy<ValidateAndHashIfNeeded>().WithV6Compatibility()
    static class MD5DeterministicNameBuilder
    {
        public static string Build(string input)
        {
            //use MD5 hash to get a 16-byte hash of the string
            using (var provider = new MD5CryptoServiceProvider())
            {
                var inputBytes = Encoding.Default.GetBytes(input);
                var hashBytes = provider.ComputeHash(inputBytes);

                return new Guid(hashBytes).ToString();
            }
        }
    }


    public Task Cleanup()
    {
        return Task.FromResult(0);
    }
}