# Legacy Azure Service Bus transport for NServiceBus

The Azure ServiceBus transport for NServiceBus enables the use of the Azure Service Bus Brokered Messaging service as the underlying transport used by NServiceBus. 
This transport uses the legacy [WindowsAzure.ServiceBus nuget package](https://www.nuget.org/packages/WindowsAzure.ServiceBus/).

## Documentation

[Azure Service Bus Transport](https://docs.particular.net/transports/azure-service-bus/legacy/)

[Samples](http://docs.particular.net/samples/azure/)

## Running the Acceptance Tests

Follow these steps to run the acceptance tests locally:
* Add a new environment variable `Transport.UseSpecific` with the value `AzureServiceBusTransport`
* Add a new environment variable `AzureServiceBusTransport.ConnectionString` containing a connection string to your Azure Service Bus namespace
* Add a new environment variable `AzureServiceBusTransport.Topology` with the value `ForwardingTopology` or `EndpointOrientedTopology`

## Running the Unit Tests

* Add a new environment variable `AzureServiceBus.ConnectionString`containing a connection string to your Azure Service Bus namespace (could be same as for acceptance tests)
* Add a new environment variable `AzureServiceBus.ConnectionString.Fallback` containing a connection string to your Azure Service Bus fallback namespace


