namespace NServiceBus.Azure.WindowsAzureServiceBus.Tests.API
{
    using System.IO;
    using System.Runtime.CompilerServices;
    using ApiApprover;
    using ApprovalTests;
    using Mono.Cecil;
    using NUnit.Framework;

    [TestFixture]
    public class APIApprovals
    {
        [Test]
        [MethodImpl(MethodImplOptions.NoInlining)]
        public void ApproveAzureServiceBusTransport()
        {
            var assemblyPath = Path.GetFullPath(typeof(AzureServiceBusTransport).Assembly.Location);
            var asm = AssemblyDefinition.ReadAssembly(assemblyPath);
            var publicApi = PublicApiGenerator.CreatePublicApiForAssembly(asm, definition => true, false);
            Approvals.Verify(publicApi);
        }
    }
}