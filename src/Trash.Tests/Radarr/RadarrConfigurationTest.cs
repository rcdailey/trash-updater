using System;
using System.IO;
using System.IO.Abstractions;
using FluentAssertions;
using NSubstitute;
using NUnit.Framework;
using Trash.Config;
using Trash.Radarr;
using YamlDotNet.Core;
using YamlDotNet.Serialization.ObjectFactories;

namespace Trash.Tests.Radarr
{
    [TestFixture]
    [Parallelizable(ParallelScope.All)]
    public class RadarrConfigurationTest
    {
        [Test]
        public void Deserialize_QualityDefinitionTypeMissing_Throw()
        {
            const string yaml = @"
radarr:
- base_url: a
  api_key: b
  quality_definition:
    preferred_ratio: 0.5
";
            var loader = new ConfigurationLoader<RadarrConfiguration>(
                Substitute.For<IConfigurationProvider<RadarrConfiguration>>(),
                Substitute.For<IFileSystem>(),
                new DefaultObjectFactory());

            Action act = () => loader.LoadFromStream(new StringReader(yaml), "radarr");

            act.Should().Throw<YamlException>()
                .WithMessage("*'type' is required for 'quality_definition'");
        }
    }
}
