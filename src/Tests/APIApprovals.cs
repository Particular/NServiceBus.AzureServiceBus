namespace NServiceBus.Azure.WindowsAzureServiceBus.Tests.API
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Runtime.CompilerServices;
    using ApprovalTests;
    using NUnit.Framework;
    using PublicApiGenerator;

    [TestFixture]
    public class APIApprovals
    {
        [Test]
        [MethodImpl(MethodImplOptions.NoInlining)]
        public void ApproveAzureServiceBusTransport()
        {
            var combine = Path.Combine(TestContext.CurrentContext.TestDirectory, "NServiceBus.Azure.Transports.WindowsAzureServiceBus.dll");
            var assembly = Assembly.LoadFile(combine);
            var publicApi = Filter(ApiGenerator.GeneratePublicApi(assembly));
            Approvals.Verify(publicApi);
        }

        string Filter(string text)
        {
            return string.Join(Environment.NewLine, text.Split(new[]
                {
                    Environment.NewLine
                }, StringSplitOptions.RemoveEmptyEntries)
                .Where(l => !l.StartsWith("[assembly: ReleaseDateAttribute("))
                .Where(l => !string.IsNullOrWhiteSpace(l))
            );
        }
    }
}