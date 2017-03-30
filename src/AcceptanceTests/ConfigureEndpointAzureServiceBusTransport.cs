﻿using System.Threading.Tasks;
using NServiceBus;
using NServiceBus.AcceptanceTesting.Support;
using TesingConventions = NServiceBus.AcceptanceTesting.Customization;
using NServiceBus.AcceptanceTests.Routing.NativePublishSubscribe;
using NServiceBus.AcceptanceTests.ScenarioDescriptors;
using NServiceBus.Azure.Transports.WindowsAzureServiceBus.AcceptanceTests.Routing;
using NServiceBus.AzureServiceBus.AcceptanceTests.Infrastructure;

public class ConfigureEndpointAzureServiceBusTransport : IConfigureEndpointTestExecution
{
    public Task Configure(string endpointName, EndpointConfiguration configuration, RunSettings settings, PublisherMetadata publisherMetadata)
    {
        var connectionString = EnvironmentHelper.GetEnvironmentVariable($"{nameof(AzureServiceBusTransport)}.ConnectionString");
        var topology = EnvironmentHelper.GetEnvironmentVariable("AzureServiceBusTransport.Topology");

        var transportConfig = configuration.UseTransport<AzureServiceBusTransport>();

        AzureServiceBusTransportConfigContext azureServiceBusTransportConfigContext;
        if(settings.TryGet("AzureServiceBus.AcceptanceTests.TransportConfigContext", out azureServiceBusTransportConfigContext))
        {
            azureServiceBusTransportConfigContext.Callback?.Invoke(endpointName, transportConfig);
        }
        else
        {
            transportConfig.ConnectionString(connectionString);
        }

        if (topology == "ForwardingTopology")
        {
            transportConfig.UseForwardingTopology();
        }
        else
        {
            var endpointOrientedTopology = transportConfig.UseEndpointOrientedTopology();
            foreach (var publisher in publisherMetadata.Publishers)
            {
                foreach (var eventType in publisher.Events)
                {
                    endpointOrientedTopology.RegisterPublisher(eventType, publisher.PublisherName);
                }
            }
            
            // ATTs that that require publishers to be explicitely registered for the EndpointOrientedTopology
            endpointOrientedTopology.RegisterPublisher(typeof(When_multi_subscribing_to_a_polymorphic_event.MyEvent1), TesingConventions.Conventions.EndpointNamingConvention(typeof(When_multi_subscribing_to_a_polymorphic_event.Publisher1)));
            endpointOrientedTopology.RegisterPublisher(typeof(When_multi_subscribing_to_a_polymorphic_event.MyEvent2), TesingConventions.Conventions.EndpointNamingConvention(typeof(When_multi_subscribing_to_a_polymorphic_event.Publisher2)));

            endpointOrientedTopology.RegisterPublisher(typeof(When_publishing_to_scaled_out_subscribers.MyEvent), TesingConventions.Conventions.EndpointNamingConvention(typeof(When_publishing_to_scaled_out_subscribers.Publisher)));

            endpointOrientedTopology.RegisterPublisher(typeof(When_unsubscribing_from_event.Event), TesingConventions.Conventions.EndpointNamingConvention(typeof(When_unsubscribing_from_event.Publisher)));

            endpointOrientedTopology.RegisterPublisher(typeof(When_unsubscribing.MyEvent), TesingConventions.Conventions.EndpointNamingConvention(typeof(When_unsubscribing.Endpoint)));

            // TODO: investigate why these tests that are intended for the ForwradingTopology only fail w/o publisher registration on EndpointOrientedTopology execution on build server
            endpointOrientedTopology.RegisterPublisher(typeof(When_subscribing_to_base_and_derived_polymorphic_events_with_forwarding_topology.BaseEvent), TesingConventions.Conventions.EndpointNamingConvention(typeof(When_subscribing_to_base_and_derived_polymorphic_events_with_forwarding_topology.Publisher)));
            endpointOrientedTopology.RegisterPublisher(typeof(When_subscribing_to_base_and_derived_polymorphic_events_with_forwarding_topology.DerivedEvent), TesingConventions.Conventions.EndpointNamingConvention(typeof(When_subscribing_to_base_and_derived_polymorphic_events_with_forwarding_topology.Publisher)));
            endpointOrientedTopology.RegisterPublisher(typeof(When_unsubscribing_from_one_of_the_events_for_ForwardingTopology.MyEvent), TesingConventions.Conventions.EndpointNamingConvention(typeof(When_unsubscribing_from_one_of_the_events_for_ForwardingTopology.Endpoint)));
            endpointOrientedTopology.RegisterPublisher(typeof(When_unsubscribing_from_one_of_the_events_for_ForwardingTopology.MyOtherEvent), TesingConventions.Conventions.EndpointNamingConvention(typeof(When_unsubscribing_from_one_of_the_events_for_ForwardingTopology.Endpoint)));
        }

        transportConfig.Sanitization()
            .UseStrategy<ValidateAndHashIfNeeded>();

        configuration.RegisterComponents(c => { c.ConfigureComponent<TestIndependenceMutator>(DependencyLifecycle.SingleInstance); });

        configuration.Pipeline.Register("TestIndependenceBehavior", typeof(TestIndependenceSkipBehavior),
            "Skips messages not created during the current test.");

        // w/o retries ASB will move attempted messages to the error queue right away, which will cause false failure.
        // ScenarioRunner.PerformScenarios() verifies by default no messages are moved into error queue. If it finds any, it fails the test.
        configuration.Recoverability().Immediate(retriesSettings => retriesSettings.NumberOfRetries(3));

        return Task.FromResult(0);
    }

    public Task Cleanup()
    {
        return Task.FromResult(0);
    }
}