    using NServiceBus.AcceptanceTests.Retries;
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
            When_doing_flr_with_dtc_on.X = () => 4;
            When_doing_flr_with_native_transactions.X = () => 4;
            When_handler_throws_serialization_exception.MaxNumberOfRetries = () => 4;
        }
    }

