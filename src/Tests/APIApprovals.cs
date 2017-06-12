namespace NServiceBus.Azure.WindowsAzureServiceBus.Tests.API
{
    using System.IO;
    using System.Runtime.CompilerServices;
    using ApiApprover;
    using NUnit.Framework;

    [TestFixture]
    public class APIApprovals
    {
        [Test]
        [MethodImpl(MethodImplOptions.NoInlining)]
        public void ApproveAzureServiceBusTransport()
        {
            Directory.SetCurrentDirectory(TestContext.CurrentContext.TestDirectory);
            PublicApiApprover.ApprovePublicApi(typeof(AzureServiceBusTransport).Assembly);
        }
    }
}