using System;
using System.IO;
using System.IO.Abstractions;
using FluentAssertions;
using NSubstitute;
using NUnit.Framework;
using Trash.Config;
using Trash.Sonarr;
using YamlDotNet.Core;
using YamlDotNet.Serialization.ObjectFactories;

namespace Trash.Tests.Sonarr
{
    [TestFixture]
    [Parallelizable(ParallelScope.All)]
    public class SonarrConfigurationTest
    {
        [Test]
        public void Deserialize_ReleaseProfileTypeMissing_Throw()
        {
            const string yaml = @"
sonarr:
- base_url: a
  api_key: b
  release_profiles:
  - strict_negative_scores: true
";
            var loader = new ConfigurationLoader<SonarrConfiguration>(
                Substitute.For<IConfigurationProvider<SonarrConfiguration>>(),
                Substitute.For<IFileSystem>(),
                new DefaultObjectFactory());

            Action act = () => loader.LoadFromStream(new StringReader(yaml), "sonarr");

            act.Should().Throw<YamlException>()
                .WithMessage("*'type' is required for 'release_profiles' elements");
        }
    }
}
