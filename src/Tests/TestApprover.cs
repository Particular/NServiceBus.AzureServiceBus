namespace NServiceBus.AzureServiceBus.Tests
{
    using System.IO;
    using ApprovalTests;
    using ApprovalTests.Namers;

    static class TestApprover
    {
        public static void Verify(string text)
        {
            var writer = new ApprovalTextWriter(text);
            var namer = new ApprovalNamer();
            Approvals.Verify(writer, namer, Approvals.GetReporter());
        }

        class ApprovalNamer : UnitTestFrameworkNamer
        {
            public ApprovalNamer()
            {
                var assemblyPath = GetType().Assembly.Location;
                var assemblyDir = Path.GetDirectoryName(assemblyPath);

                SourcePath = Path.Combine(assemblyDir, "..", "..", "..", "ApprovalFiles");
            }

            public override string SourcePath { get; }
        }
    }
}