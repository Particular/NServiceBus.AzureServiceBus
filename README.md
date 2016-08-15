# Azure ServiceBus Transport for NServiceBus

The Azure ServiceBus transport for NServiceBus enables the use of the Azure Service Bus Brokered Messaging service as the underlying transport used by NServiceBus. 

## Documentation

[Azure Transport](http://docs.particular.net/nservicebus/windows-azure-transport)
[Samples](http://docs.particular.net/samples/azure/)

## Maintainers
The following team is responsible for this repository: @Particular/azure-service-bus-maintainers

## Running the tests

* Create 2 new ServiceBus namespaces, needs to be at least a standard tier namespace in order to support Topics. One will be used for fallback tests to name them accordingly. Eg. `andreasohlund-dev` + `andreasohlund-fallback` or similar
* Create a environment variable `AzureServiceBus.ConnectionString` and set it to the connection string for the main namespace
* Create a environment variable `AzureServiceBus.ConnectionString.Fallback` and set it to the connection string for the fallback namespace
* Create a environment vaiable `AzureServiceBusTransport.ConnectionString` (used by the AcceptanceTests) and set it to the main namespace connectionstring (or create a separate namespace if you prefer)
