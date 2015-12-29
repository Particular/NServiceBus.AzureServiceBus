//using NServiceBus.AzureServiceBus.AcceptanceTests;
using NUnit.Framework;

/// <summary>
/// Global setup fixture
/// </summary>
[SetUpFixture]
public class SetupAcceptanceTests
{
    [SetUp]
    public void SetUp()
    {
        //fix time
        //var timeSync = new TimeSynchronization();
        //timeSync.Sync();
    }
}

