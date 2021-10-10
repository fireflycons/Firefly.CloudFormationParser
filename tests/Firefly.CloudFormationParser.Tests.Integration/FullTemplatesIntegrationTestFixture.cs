namespace Firefly.CloudFormationParser.Tests.Integration
{
    using System;

    using Firefly.EmbeddedResourceLoader;
    using Firefly.EmbeddedResourceLoader.Materialization;

    /// <summary>
    /// Fixture to manage the materialization and cleanup of directory containing test CloudFormation templates
    /// </summary>
    /// <seealso cref="Firefly.EmbeddedResourceLoader.AutoResourceLoader" />
    /// <seealso cref="System.IDisposable" />
    public class FullTemplatesIntegrationTestFixture : AutoResourceLoader, IDisposable
    {
        /// <summary>
        /// Gets or sets the temporary directory where all the embedded resources are materialized.
        /// </summary>
        [EmbeddedResource(@"FullTemplates")]
#pragma warning disable 649
        public TempDirectory TempDirectory { get; set; }
#pragma warning restore 649

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            // Delete temporary directory.
            this.TempDirectory?.Dispose();
        }
    }
}