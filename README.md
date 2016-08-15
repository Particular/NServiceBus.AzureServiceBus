# Azure ServiceBus Transport for NServiceBus

The Azure ServiceBus transport for NServiceBus enables the use of the Azure Service Bus Brokered Messaging service as the underlying transport used by NServiceBus. 

## Documentation

[Azure Transport](http://docs.particular.net/nservicebus/windows-azure-transport)
[Samples](http://docs.particular.net/samples/azure/)

## Maintainers
The following team is responsible for this repository: @Particular/azure-service-bus-maintainers

## Running the tests

* Create a ServiceBus namespace, needs to be at least a standard tier namespace in order to support Topics
* Create a environment variable `X` and set it to the connection string for the namespace
