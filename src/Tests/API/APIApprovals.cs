namespace NServiceBus.Azure.WindowsAzureServiceBus.Tests.API
{
    using System.IO;
    using System.Runtime.CompilerServices;
    using ApprovalTests;
    using ApprovalTests.Reporters;
    using NUnit.Framework;
    using PublicApiGenerator;

    [TestFixture]
    public class APIApprovals
    {
        [Test]
        [MethodImpl(MethodImplOptions.NoInlining)]
        [UseReporter(typeof(DiffReporter))]
        public void ApproveAzureServiceBusTransport()
        {
            Directory.SetCurrentDirectory(TestContext.CurrentContext.TestDirectory);
            var publicApi = ApiGenerator.GeneratePublicApi(typeof(AzureServiceBusTransport).Assembly);
            Approvals.Verify(publicApi);
        }
    }
}