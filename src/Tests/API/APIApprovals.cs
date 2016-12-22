namespace NServiceBus.Azure.WindowsAzureServiceBus.Tests.API
{
    using System.IO;
    using System.Runtime.CompilerServices;
    using ApprovalTests;
    using ApprovalTests.Reporters;
    using NUnit.Framework;

    [TestFixture]
    public class APIApprovals
    {
        [Test]
        [MethodImpl(MethodImplOptions.NoInlining)]
        [UseReporter(typeof(DiffReporter))]
        public void ApproveAzureServiceBusTransport()
        {
            Directory.SetCurrentDirectory(TestContext.CurrentContext.TestDirectory);
            var publicApi = PublicApiGenerator.ApiGenerator.GeneratePublicApi(typeof(AzureServiceBusTransport).Assembly, null, false);
            Approvals.Verify(publicApi);
        }
    }
}
