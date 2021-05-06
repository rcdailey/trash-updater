using System;
using System.IO;
using System.IO.Abstractions;
using FluentAssertions;
using JetBrains.Annotations;
using NSubstitute;
using NUnit.Framework;
using Trash.Config;
using YamlDotNet.Core;
using YamlDotNet.Serialization.ObjectFactories;

namespace Trash.Tests.Config
{
    [TestFixture]
    [Parallelizable(ParallelScope.All)]
    public class ServiceConfigurationTest
    {
        // This test class must be public otherwise it cannot be deserialized by YamlDotNet
        [UsedImplicitly]
        private class TestServiceConfiguration : ServiceConfiguration
        {
            public const string ServiceName = "test_service";

            public override string BuildUrl()
            {
                throw new NotImplementedException();
            }
        }

        [Test]
        public void Deserialize_BaseUrlMissing_Throw()
        {
            const string yaml = @"
test_service:
- api_key: b
";
            var loader = new ConfigurationLoader<TestServiceConfiguration>(
                Substitute.For<IConfigurationProvider>(),
                Substitute.For<IFileSystem>(),
                new DefaultObjectFactory());

            Action act = () => loader.LoadFromStream(new StringReader(yaml), TestServiceConfiguration.ServiceName);

            act.Should().Throw<YamlException>()
                .WithMessage("*Property 'base_url' is required");
        }

        [Test]
        public void Deserialize_ApiKeyMissing_Throw()
        {
            const string yaml = @"
test_service:
- base_url: a
";
            var loader = new ConfigurationLoader<TestServiceConfiguration>(
                Substitute.For<IConfigurationProvider>(),
                Substitute.For<IFileSystem>(),
                new DefaultObjectFactory());

            Action act = () => loader.LoadFromStream(new StringReader(yaml), TestServiceConfiguration.ServiceName);

            act.Should().Throw<YamlException>()
                .WithMessage("*Property 'api_key' is required");
        }
    }
}
