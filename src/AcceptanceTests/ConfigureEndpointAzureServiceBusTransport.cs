using System;
using System.Threading.Tasks;
using NServiceBus;
using NServiceBus.AcceptanceTesting.Support;
using NServiceBus.AcceptanceTests.Routing;
using NServiceBus.AcceptanceTests.Routing.NativePublishSubscribe;
using NServiceBus.Azure.Transports.WindowsAzureServiceBus.AcceptanceTests.Routing;
using NServiceBus.AzureServiceBus.AcceptanceTests.Infrastructure;
using NServiceBus.Configuration.AdvanceExtensibility;
using Conventions = NServiceBus.AcceptanceTesting.Customization.Conventions;

public class ConfigureEndpointAzureServiceBusTransport : IConfigureEndpointTestExecution
{

    public Task Configure(string endpointName, EndpointConfiguration config, RunSettings settings, PublisherMetadata publisherMetadata)
    {
        var connectionString = settings.Get<string>("Transport.ConnectionString");
        var topology = Environment.GetEnvironmentVariable("AzureServiceBusTransport.Topology", EnvironmentVariableTarget.User);
        topology = topology ?? Environment.GetEnvironmentVariable("AzureServiceBusTransport.Topology");

        config.GetSettings().Set("AzureServiceBus.AcceptanceTests.UsedTopology", topology);

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

        if (topology == "ForwardingTopology")
        {
            transportConfig.UseForwardingTopology();
        }
        else
        {
            var topologyConfiguration = transportConfig.UseEndpointOrientedTopology();
            foreach (var publisher in publisherMetadata.Publishers)
            {
                foreach (var @event in publisher.Events)
                {
                    topologyConfiguration.RegisterPublisher(@event, publisher.PublisherName);
                }
            }

//            topologyConfiguration.RegisterPublisher(typeof(When_sending_events_bestpractices_disabled.MyEvent), Conventions.EndpointNamingConvention(typeof(When_sending_events_bestpractices_disabled.Endpoint)));
//            topologyConfiguration.RegisterPublisher(typeof(When_sending_events_bestpractices_disabled_on_endpoint.MyEvent), Conventions.EndpointNamingConvention(typeof(When_sending_events_bestpractices_disabled_on_endpoint.Endpoint)));

            topologyConfiguration.RegisterPublisher(typeof(When_base_event_from_2_publishers.DerivedEvent1), Conventions.EndpointNamingConvention(typeof(When_base_event_from_2_publishers.Publisher1)));
            topologyConfiguration.RegisterPublisher(typeof(When_base_event_from_2_publishers.DerivedEvent1), Conventions.EndpointNamingConvention(typeof(When_base_event_from_2_publishers.Publisher2)));

            topologyConfiguration.RegisterPublisher(typeof(When_multi_subscribing_to_a_polymorphic_event.MyEvent1), Conventions.EndpointNamingConvention(typeof(When_multi_subscribing_to_a_polymorphic_event.Publisher1)));
            topologyConfiguration.RegisterPublisher(typeof(When_multi_subscribing_to_a_polymorphic_event.MyEvent2), Conventions.EndpointNamingConvention(typeof(When_multi_subscribing_to_a_polymorphic_event.Publisher2)));

            topologyConfiguration.RegisterPublisher(typeof(When_publishing_to_scaled_out_subscribers.MyEvent), Conventions.EndpointNamingConvention(typeof(When_publishing_to_scaled_out_subscribers.Publisher)));
            topologyConfiguration.RegisterPublisher(typeof(When_unsubscribing_from_event.Event), Conventions.EndpointNamingConvention(typeof(When_unsubscribing_from_event.Publisher)));
        }

        transportConfig.Sanitization()
            .UseStrategy<ValidateAndHashIfNeeded>();

        config.RegisterComponents(c => { c.ConfigureComponent<TestIndependenceMutator>(DependencyLifecycle.SingleInstance); });

        config.Pipeline.Register("TestIndependenceBehavior", typeof(TestIndependenceSkipBehavior),
            "Skips messages not created during the current test.");

        // w/o retries ASB will move attempted messages to the error queue right away, which will cause false failure.
        // ScenarioRunner.PerformScenarios() verifies by default no messages are moved into error queue. If it finds any, it fails the test.
        config.Recoverability().Immediate(retriesSettings => retriesSettings.NumberOfRetries(3));

        return Task.FromResult(0);
    }

    public Task Cleanup()
    {
        return Task.FromResult(0);
    }
}