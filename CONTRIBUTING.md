For information on contributing, see https://docs.particular.net/platform/contributing.

## Running the Acceptance Tests
 
Follow these steps to run the acceptance tests locally:

* Add a new environment variable `Transport.UseSpecific` with the value `AzureServiceBusTransport`
* Add a new environment variable `AzureServiceBusTransport.ConnectionString` containing a connection string to your Azure storage account 
* Add a new environment variable `AzureServiceBusTransport.Topology` and set it to _ForwardingTopology_ to run tests configuring transport with `ForwardingTopology` topology. Don't setup `AzureServiceBusTransport.Topology` environment variable to run tests with `EndpointOrientedTopology` topology


## Running Unit/Integration Tests

To execute tests under `NServiceBus.AzureServiceBus.Tests`, two environment variables are required:

1. `AzureServiceBus.ConnectionString`
1. `AzureServiceBus.ConnectionString.Fallback`

Note that those should **not** point to the same namespace.
