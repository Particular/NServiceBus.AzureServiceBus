using System;
using System.Linq;
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
                .RegisterPublisher(typeof(When_multiple_versions_of_a_message_is_published.V1Event), NameForPublisher<When_multiple_versions_of_a_message_is_published.V2Publisher>())
                .RegisterPublisher(typeof(When_multiple_versions_of_a_message_is_published.V2Event), NameForPublisher<When_multiple_versions_of_a_message_is_published.V2Publisher>())
                .RegisterPublisher(typeof(When_replies_to_message_published_by_a_saga.DidSomething), NameForPublisher<When_replies_to_message_published_by_a_saga.SagaEndpoint>())
                .RegisterPublisher(typeof(When_started_by_base_event_from_other_saga.SomethingHappenedEvent), NameForPublisher<When_started_by_base_event_from_other_saga.Publisher>())
                .RegisterPublisher(typeof(When_started_by_event_from_another_saga.SomethingHappenedEvent), NameForPublisher<When_started_by_event_from_another_saga.SagaThatPublishesAnEvent>())
                .RegisterPublisher(typeof(When_base_event_from_2_publishers.DerivedEvent1), NameForPublisher<When_base_event_from_2_publishers.Publisher1>())
                .RegisterPublisher(typeof(When_base_event_from_2_publishers.DerivedEvent2), NameForPublisher<When_base_event_from_2_publishers.Publisher2>())
                .RegisterPublisher(typeof(When_publishing.IFoo), NameForPublisher<When_publishing.Publisher3>())
                .RegisterPublisher(typeof(When_publishing.MyEvent), NameForPublisher<When_publishing.Publisher>())
                .RegisterPublisher(typeof(When_publishing_an_event_implementing_two_unrelated_interfaces.CompositeEvent), NameForPublisher<When_publishing_an_event_implementing_two_unrelated_interfaces.Publisher>())
                .RegisterPublisher(typeof(When_publishing_an_interface.MyEvent), NameForPublisher<When_publishing_an_interface.Publisher>())
                .RegisterPublisher(typeof(When_publishing_using_root_type.EventMessage), NameForPublisher<When_publishing_using_root_type.Publisher>())
                .RegisterPublisher(typeof(When_publishing_with_only_local_messagehandlers.EventHandledByLocalEndpoint), NameForPublisher<When_publishing_with_only_local_messagehandlers.CentralizedStoragePublisher>())
                .RegisterPublisher(typeof(When_subscribing_to_a_polymorphic_event.MyEvent), NameForPublisher<When_subscribing_to_a_polymorphic_event.MyEvent>())
                .RegisterPublisher(typeof(When_starting_an_endpoint_with_a_saga.MyEvent), NameForPublisher<When_starting_an_endpoint_with_a_saga.Subscriber>())
                .RegisterPublisher(typeof(When_starting_an_endpoint_with_a_saga.MyEventBase), NameForPublisher<When_starting_an_endpoint_with_a_saga.Subscriber>())
                .RegisterPublisher(typeof(When_publishing_command_bestpractices_disabled.MyCommand), NameForPublisher<When_publishing_command_bestpractices_disabled.Endpoint>())
                .RegisterPublisher(typeof(When_publishing_command_bestpractices_disabled.MyEvent), NameForPublisher<When_publishing_command_bestpractices_disabled.Endpoint>())
                .RegisterPublisher(typeof(When_publishing_command_bestpractices_disabled_on_endpoint.MyCommand), NameForPublisher<When_publishing_command_bestpractices_disabled_on_endpoint.Endpoint>())
                .RegisterPublisher(typeof(When_publishing_command_bestpractices_disabled_on_endpoint.MyEvent), NameForPublisher<When_publishing_command_bestpractices_disabled_on_endpoint.Endpoint>())
                .RegisterPublisher(typeof(When_sending_events_bestpractices_disabled.MyEvent), NameForPublisher<When_sending_events_bestpractices_disabled.Endpoint>())
                .RegisterPublisher(typeof(When_sending_events_bestpractices_disabled_on_endpoint.MyEvent), NameForPublisher<When_sending_events_bestpractices_disabled_on_endpoint.Endpoint>())
                .RegisterPublisher(typeof(When_subscribing_to_command_bestpractices_disabled_on_endpoint.MyCommand), NameForPublisher<When_subscribing_to_command_bestpractices_disabled_on_endpoint.Endpoint>())
                .RegisterPublisher(typeof(When_subscribing_to_command_bestpractices_disabled_on_endpoint.MyEvent), NameForPublisher<When_subscribing_to_command_bestpractices_disabled_on_endpoint.Endpoint>())
                .RegisterPublisher(typeof(When_unsubscribing_to_command_bestpractices_disabled_on_endpoint.MyCommand), NameForPublisher<When_unsubscribing_to_command_bestpractices_disabled_on_endpoint.Endpoint>())
                .RegisterPublisher(typeof(When_unsubscribing_to_command_bestpractices_disabled_on_endpoint.MyEvent), NameForPublisher<When_unsubscribing_to_command_bestpractices_disabled_on_endpoint.Endpoint>())
                .RegisterPublisher(typeof(When_extending_the_publish_api.MyEvent), NameForPublisher<When_extending_the_publish_api.Publisher>())
                .RegisterPublisher(typeof(When_subscribing_to_base_and_derived_polymorphic_events_with_forwarding_topology.BaseEvent), NameForPublisher<When_subscribing_to_base_and_derived_polymorphic_events_with_forwarding_topology.Publisher>())
                .RegisterPublisher(typeof(When_subscribing_to_base_and_derived_polymorphic_events_with_forwarding_topology.DerivedEvent), NameForPublisher<When_subscribing_to_base_and_derived_polymorphic_events_with_forwarding_topology.Publisher>())
                .RegisterPublisher(typeof(When_publishing_to_scaled_out_subscribers_on_multicast_transports.MyEvent), NameForPublisher<When_publishing_to_scaled_out_subscribers_on_multicast_transports.Publisher>())
                .RegisterPublisher(typeof(When_multi_subscribing_to_a_polymorphic_event_on_multicast_transports.MyEvent1), NameForPublisher<When_multi_subscribing_to_a_polymorphic_event_on_multicast_transports.Publisher1>())
                .RegisterPublisher(typeof(When_multi_subscribing_to_a_polymorphic_event_on_multicast_transports.MyEvent2), NameForPublisher<When_multi_subscribing_to_a_polymorphic_event_on_multicast_transports.Publisher2>())
                .RegisterPublisher(typeof(When_publishing_an_interface_with_unobtrusive.MyEvent), NameForPublisher<When_publishing_an_interface_with_unobtrusive.Publisher>());
        }

        transportConfig.Sanitization()
            .UseStrategy<ValidateAndHashIfNeeded>();

        config.RegisterComponents(c => { c.ConfigureComponent<TestIndependenceMutator>(DependencyLifecycle.SingleInstance); });

        config.Pipeline.Register("TestIndependenceBehavior", typeof(TestIndependenceSkipBehavior),
            "Skips messages not created during the current test.");

        return Task.FromResult(0);
    }

    // Copy of ATT Conventions.EndpointNamingConvention logic to convert nested endpoint class name to publisher name
    static string NameForPublisher<T>() where T : class
    {
        var classAndEndpoint = typeof(T).FullName.Split('.').Last();
        var testName = classAndEndpoint.Split('+').First();
        testName = testName.Replace("When_", "");
        var endpoint = classAndEndpoint.Split('+').Last();
        testName = System.Threading.Thread.CurrentThread.CurrentCulture.TextInfo.ToTitleCase(testName);
        testName = testName.Replace("_", "");

        return testName + "." + endpoint;
    }

    public Task Cleanup()
    {
        return Task.FromResult(0);
    }
}