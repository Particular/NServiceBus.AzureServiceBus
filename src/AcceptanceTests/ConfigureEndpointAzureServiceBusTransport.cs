using System;
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
using NServiceBus.AzureServiceBus.AcceptanceTests.Infrastructure;

public class ConfigureEndpointAzureServiceBusTransport : IConfigureEndpointTestExecution
{
    public Task Configure(string endpointName, EndpointConfiguration config, RunSettings settings)
    {
        var connectionString = settings.Get<string>("Transport.ConnectionString");
        var topology = Environment.GetEnvironmentVariable("AzureServiceBusTransport.Topology", EnvironmentVariableTarget.User);
        topology = topology ?? Environment.GetEnvironmentVariable("AzureServiceBusTransport.Topology");

        var transportConfig = config.UseTransport<AzureServiceBusTransport>();

        AzureServiceBusTransportConfigContext azureServiceBusTransportConfigContext;
        if(settings.TryGet("AzureServiceBus.AcceptanceTests.TransportConfigContext", out azureServiceBusTransportConfigContext))
        {
            azureServiceBusTransportConfigContext.Callback?.Invoke(endpointName, transportConfig);
        }
        else
        {
            transportConfig.ConnectionString(connectionString);
        }

        if (topology == nameof(ForwardingTopology))
        {
            transportConfig.UseTopology<ForwardingTopology>();
        }
        else
        {
            transportConfig.UseTopology<EndpointOrientedTopology>()
                .RegisterPublisher(typeof(When_multiple_versions_of_a_message_is_published.V1Event), "MultipleVersionsOfAMessageIsPublished.V2Publisher")
                .RegisterPublisher(typeof(When_multiple_versions_of_a_message_is_published.V2Event), "MultipleVersionsOfAMessageIsPublished.V2Publisher")
                .RegisterPublisher(typeof(When_replies_to_message_published_by_a_saga.DidSomething), "RepliesToMessagePublishedByASaga.SagaEndpoint")
                .RegisterPublisher(typeof(When_started_by_base_event_from_other_saga.SomethingHappenedEvent), "StartedByBaseEventFromOtherSaga.Publisher")
                .RegisterPublisher(typeof(When_started_by_event_from_another_saga.SomethingHappenedEvent), "StartedByEventFromAnotherSaga.SagaThatPublishesAnEvent")
                .RegisterPublisher(typeof(When_base_event_from_2_publishers.DerivedEvent1), "BaseEventFrom2Publishers.Publisher1")
                .RegisterPublisher(typeof(When_base_event_from_2_publishers.DerivedEvent2), "BaseEventFrom2Publishers.Publisher2")
                .RegisterPublisher(typeof(When_publishing.IFoo), "Publishing.Publisher3")
                .RegisterPublisher(typeof(When_publishing.MyEvent), "Publishing.Publisher")
                .RegisterPublisher(typeof(When_publishing_an_event_implementing_two_unrelated_interfaces.CompositeEvent), "PublishingAnEventImplementingTwoUnrelatedInterfaces.Publisher")
                .RegisterPublisher(typeof(When_publishing_an_interface.MyEvent), "PublishingAnInterface.Publisher")
                .RegisterPublisher(typeof(When_publishing_using_root_type.EventMessage), "PublishingUsingRootType.Publisher")
                .RegisterPublisher(typeof(When_publishing_with_only_local_messagehandlers.EventHandledByLocalEndpoint), "PublishingWithOnlyLocalMessagehandlers.CentralizedStoragePublisher")
                .RegisterPublisher(typeof(When_subscribing_to_a_polymorphic_event.MyEvent), "SubscribingToAPolymorphicEvent.Publisher")
                .RegisterPublisher(typeof(When_starting_an_endpoint_with_a_saga.MyEvent), "StartingAnEndpointWithASaga.Subscriber")
                .RegisterPublisher(typeof(When_starting_an_endpoint_with_a_saga.MyEventBase), "StartingAnEndpointWithASaga.Subscriber")
                .RegisterPublisher(typeof(When_publishing_command_bestpractices_disabled.MyCommand), "PublishingCommandBestpracticesDisabled.Endpoint")
                .RegisterPublisher(typeof(When_publishing_command_bestpractices_disabled.MyEvent), "PublishingCommandBestpracticesDisabled.Endpoint")
                .RegisterPublisher(typeof(When_publishing_command_bestpractices_disabled_on_endpoint.MyCommand), "PublishingCommandBestpracticesDisabledOnEndpoint.Endpoint")
                .RegisterPublisher(typeof(When_publishing_command_bestpractices_disabled_on_endpoint.MyEvent), "PublishingCommandBestpracticesDisabledOnEndpoint.Endpoint")
                .RegisterPublisher(typeof(When_sending_events_bestpractices_disabled.MyEvent), "SendingEventsBestpracticesDisabled.Endpoint")
                .RegisterPublisher(typeof(When_sending_events_bestpractices_disabled_on_endpoint.MyEvent), "SendingEventsBestpracticesDisabledOnEndpoint.Endpoint")
                .RegisterPublisher(typeof(When_subscribing_to_command_bestpractices_disabled_on_endpoint.MyCommand), "SubscribingToCommandBestpracticesDisabledOnEndpoint.Endpoint")
                .RegisterPublisher(typeof(When_subscribing_to_command_bestpractices_disabled_on_endpoint.MyEvent), "SubscribingToCommandBestpracticesDisabledOnEndpoint.Endpoint")
                .RegisterPublisher(typeof(When_unsubscribing_to_command_bestpractices_disabled_on_endpoint.MyCommand), "UnsubscribingToCommandBestpracticesDisabledOnEndpoint.Endpoint")
                .RegisterPublisher(typeof(When_unsubscribing_to_command_bestpractices_disabled_on_endpoint.MyEvent), "UnsubscribingToCommandBestpracticesDisabledOnEndpoint.Endpoint")
                .RegisterPublisher(typeof(When_extending_the_publish_api.MyEvent), "ExtendingThePublishApi.Publisher")
                .RegisterPublisher(typeof(When_subscribing_to_base_and_derived_polymorphic_events_with_forwarding_topology.BaseEvent), "SubscribingToBaseAndDerivedPolymorphicEventsWithForwardingTopology.Publisher")
                .RegisterPublisher(typeof(When_subscribing_to_base_and_derived_polymorphic_events_with_forwarding_topology.DerivedEvent), "SubscribingToBaseAndDerivedPolymorphicEventsWithForwardingTopology.Publisher")
                .RegisterPublisher(typeof(When_publishing_to_scaled_out_subscribers_on_multicast_transports.MyEvent), "PublishingToScaledOutSubscribersOnMulticastTransports.Publisher")
                .RegisterPublisher(typeof(When_multi_subscribing_to_a_polymorphic_event_on_multicast_transports.MyEvent1), "MultiSubscribingToAPolymorphicEventOnMulticastTransports.Publisher1")
                .RegisterPublisher(typeof(When_multi_subscribing_to_a_polymorphic_event_on_multicast_transports.MyEvent2), "MultiSubscribingToAPolymorphicEventOnMulticastTransports.Publisher2")
                .RegisterPublisher(typeof(When_publishing_an_interface_with_unobtrusive.MyEvent), "PublishingAnInterfaceWithUnobtrusive.Publisher");
        }

        transportConfig.Sanitization()
            .UseStrategy<ValidateAndHashIfNeeded>();

        config.RegisterComponents(c => { c.ConfigureComponent<TestIndependenceMutator>(DependencyLifecycle.SingleInstance); });

        config.Pipeline.Register("TestIndependenceBehavior", typeof(TestIndependenceSkipBehavior),
            "Skips messages not created during the current test.");

        return Task.FromResult(0);
    }

    public Task Cleanup()
    {
        return Task.FromResult(0);
    }
}