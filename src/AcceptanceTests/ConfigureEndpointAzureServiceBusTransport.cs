using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NServiceBus;
using NServiceBus.AcceptanceTesting.Support;
using NServiceBus.AcceptanceTests.ApiExtension;
using NServiceBus.AcceptanceTests.BestPractices;
using NServiceBus.AcceptanceTests.Routing;
using NServiceBus.AcceptanceTests.Routing.AutomaticSubscriptions;
using NServiceBus.AcceptanceTests.Sagas;
using NServiceBus.AcceptanceTests.ScenarioDescriptors;
using NServiceBus.AcceptanceTests.Versioning;
using NServiceBus.AzureServiceBus;
using NServiceBus.AzureServiceBus.AcceptanceTests.Infrastructure;

public class ConfigureScenariosForAzureServiceBusTransport : IConfigureSupportedScenariosForTestExecution
{
    public IEnumerable<Type> UnsupportedScenarioDescriptorTypes { get; } = new[]
    {
        typeof(AllDtcTransports),
        typeof(AllTransportsWithMessageDrivenPubSub),
        typeof(AllTransportsWithoutNativeDeferral),
        typeof(AllNativeMultiQueueTransactionTransports),
        typeof(AllTransportsWithoutNativeDeferralAndWithAtomicSendAndReceive)
    };
}

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
                .RegisterPublisherForType("ExtendingThePublishApi.Publisher", typeof(When_extending_the_publish_api.MyEvent));
        }

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