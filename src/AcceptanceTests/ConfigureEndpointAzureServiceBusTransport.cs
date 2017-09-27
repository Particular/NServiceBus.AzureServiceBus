using System.Threading.Tasks;
using NServiceBus;
using NServiceBus.AcceptanceTesting.Support;
using NServiceBus.AcceptanceTests.DelayedDelivery;
using NServiceBus.AcceptanceTests.Routing;
using TestingConventions = NServiceBus.AcceptanceTesting.Customization;
using NServiceBus.AcceptanceTests.Routing.NativePublishSubscribe;
using NServiceBus.AcceptanceTests.Sagas;
using NServiceBus.AcceptanceTests.ScenarioDescriptors;
using NServiceBus.AcceptanceTests.Versioning;
using NServiceBus.Azure.Transports.WindowsAzureServiceBus.AcceptanceTests.Routing;
using NServiceBus.AzureServiceBus.AcceptanceTests.Infrastructure;
using NServiceBus.Configuration.AdvancedExtensibility;
using NUnit.Framework;

public class ConfigureEndpointAzureServiceBusTransport : IConfigureEndpointTestExecution
{
    public Task Configure(string endpointName, EndpointConfiguration configuration, RunSettings settings, PublisherMetadata publisherMetadata)
    {
        PreventInconclusiveTestsFromRunning(endpointName);

        configuration.UseSerialization<NewtonsoftSerializer>();

        var connectionString = EnvironmentHelper.GetEnvironmentVariable($"{nameof(AzureServiceBusTransport)}.ConnectionString");
        var topology = EnvironmentHelper.GetEnvironmentVariable("AzureServiceBusTransport.Topology");

        configuration.GetSettings().Set("AzureServiceBus.AcceptanceTests.UsedTopology", topology);

        var transportConfig = configuration.UseTransport<AzureServiceBusTransport>();

        if(settings.TryGet<AzureServiceBusTransportConfigContext>("AzureServiceBus.AcceptanceTests.TransportConfigContext", out var azureServiceBusTransportConfigContext))
        {
            azureServiceBusTransportConfigContext.Callback?.Invoke(endpointName, transportConfig);
        }
        else
        {
            transportConfig.ConnectionString(connectionString);

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

                // ATTs that that require publishers to be explicitly registered for the EndpointOrientedTopology
                endpointOrientedTopology.RegisterPublisher(typeof(When_multi_subscribing_to_a_polymorphic_event.MyEvent1), TestingConventions.Conventions.EndpointNamingConvention(typeof(When_multi_subscribing_to_a_polymorphic_event.Publisher1)));
                endpointOrientedTopology.RegisterPublisher(typeof(When_multi_subscribing_to_a_polymorphic_event.MyEvent2), TestingConventions.Conventions.EndpointNamingConvention(typeof(When_multi_subscribing_to_a_polymorphic_event.Publisher2)));

                endpointOrientedTopology.RegisterPublisher(typeof(When_publishing_to_scaled_out_subscribers.MyEvent), TestingConventions.Conventions.EndpointNamingConvention(typeof(When_publishing_to_scaled_out_subscribers.Publisher)));

                endpointOrientedTopology.RegisterPublisher(typeof(When_unsubscribing_from_event.Event), TestingConventions.Conventions.EndpointNamingConvention(typeof(When_unsubscribing_from_event.Publisher)));

                endpointOrientedTopology.RegisterPublisher(typeof(When_unsubscribing.MyEvent), TestingConventions.Conventions.EndpointNamingConvention(typeof(When_unsubscribing.Endpoint)));

                endpointOrientedTopology.RegisterPublisher(typeof(When_multiple_versions_of_a_message_is_published.V1Event), TestingConventions.Conventions.EndpointNamingConvention(typeof(When_multiple_versions_of_a_message_is_published.V2Publisher)));
                endpointOrientedTopology.RegisterPublisher(typeof(When_multiple_versions_of_a_message_is_published.V2Event), TestingConventions.Conventions.EndpointNamingConvention(typeof(When_multiple_versions_of_a_message_is_published.V2Publisher)));

                endpointOrientedTopology.RegisterPublisher(typeof(When_replies_to_message_published_by_a_saga.DidSomething), TestingConventions.Conventions.EndpointNamingConvention(typeof(When_replies_to_message_published_by_a_saga.SagaEndpoint)));

                endpointOrientedTopology.RegisterPublisher(typeof(When_started_by_base_event_from_other_saga.BaseEvent), TestingConventions.Conventions.EndpointNamingConvention(typeof(When_started_by_base_event_from_other_saga.Publisher)));
                endpointOrientedTopology.RegisterPublisher(typeof(When_started_by_event_from_another_saga.SomethingHappenedEvent), TestingConventions.Conventions.EndpointNamingConvention(typeof(When_started_by_event_from_another_saga.SagaThatPublishesAnEvent)));

                //When_two_sagas_subscribe_to_the_same_event
                endpointOrientedTopology.RegisterPublisher(typeof(When_two_sagas_subscribe_to_the_same_event.GroupPendingEvent), TestingConventions.Conventions.EndpointNamingConvention(typeof(When_two_sagas_subscribe_to_the_same_event.Publisher)));

                // TODO: investigate why these tests that are intended for the ForwradingTopology only fail w/o publisher registration on EndpointOrientedTopology execution on build server
                endpointOrientedTopology.RegisterPublisher(typeof(When_subscribing_to_base_and_derived_polymorphic_events_with_forwarding_topology.BaseEvent), TestingConventions.Conventions.EndpointNamingConvention(typeof(When_subscribing_to_base_and_derived_polymorphic_events_with_forwarding_topology.Publisher)));
                endpointOrientedTopology.RegisterPublisher(typeof(When_subscribing_to_base_and_derived_polymorphic_events_with_forwarding_topology.DerivedEvent), TestingConventions.Conventions.EndpointNamingConvention(typeof(When_subscribing_to_base_and_derived_polymorphic_events_with_forwarding_topology.Publisher)));
                endpointOrientedTopology.RegisterPublisher(typeof(When_unsubscribing_from_one_of_the_events_for_ForwardingTopology.MyEvent), TestingConventions.Conventions.EndpointNamingConvention(typeof(When_unsubscribing_from_one_of_the_events_for_ForwardingTopology.Endpoint)));
                endpointOrientedTopology.RegisterPublisher(typeof(When_unsubscribing_from_one_of_the_events_for_ForwardingTopology.MyOtherEvent), TestingConventions.Conventions.EndpointNamingConvention(typeof(When_unsubscribing_from_one_of_the_events_for_ForwardingTopology.Endpoint)));

                endpointOrientedTopology.RegisterPublisher(typeof(When_publishing_from_sendonly.MyEvent), TestingConventions.Conventions.EndpointNamingConvention(typeof(When_publishing_from_sendonly.SendOnlyPublisher)));
                endpointOrientedTopology.RegisterPublisher(typeof(When_subscribing_to_a_base_event.IBaseEvent), TestingConventions.Conventions.EndpointNamingConvention(typeof(When_subscribing_to_a_base_event.Publisher)));
                endpointOrientedTopology.RegisterPublisher(typeof(When_subscribing_to_a_derived_event.SpecificEvent), TestingConventions.Conventions.EndpointNamingConvention(typeof(When_subscribing_to_a_derived_event.Publisher)));

                endpointOrientedTopology.RegisterPublisher(typeof(When_publishing_with_overridden_local_address.MyEvent), TestingConventions.Conventions.EndpointNamingConvention(typeof(When_publishing_with_overridden_local_address.Publisher)));
				// Both publisher and subscriber are the same endpoint with overridden endpoint name. We can't detect both from the message type.
                endpointOrientedTopology.RegisterPublisher(typeof(When_publishing_and_subscribing_to_self_with_overridden_address.MyEvent), "myinputqueue");
            }
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

    void PreventInconclusiveTestsFromRunning(string endpointName)
    {
        if (endpointName == TestingConventions.Conventions.EndpointNamingConvention(typeof(When_deferring_to_non_local.Endpoint))
            || endpointName == TestingConventions.Conventions.EndpointNamingConvention(typeof(When_deferring_a_message.Endpoint)))
        {
            Assert.Inconclusive("Flaky test that relies on time and cannot be executed.");
        }
    }

    public Task Cleanup()
    {
        return Task.FromResult(0);
    }
}