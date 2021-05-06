using System.Collections.Generic;
using FluentAssertions;
using NUnit.Framework;
using Trash.Radarr;
using Trash.Radarr.CustomFormat.Models;
using Trash.Radarr.CustomFormat.Processors;

namespace Trash.Tests.Radarr.CustomFormat.Processors
{
    [TestFixture]
    [Parallelizable(ParallelScope.All)]
    public class QualityProfileProcessorTest
    {
        [Test]
        public void Process_ConfigDefinesScore_OverwriteScoreFromGuide()
        {
            var testConfigData = new List<ProcessedConfigData>
            {
                new()
                {
                    CustomFormats = new List<ProcessedCustomFormatData>
                    {
                        new() {Score = 100, TrashId = "id1"}
                    },
                    QualityProfiles = new List<QualityProfileConfig>
                    {
                        new() {Name = "profile1", Score = 50}
                    }
                }
            };

            var processor = new QualityProfileProcessor();
            processor.Process(testConfigData);

            processor.ProfileScores.Should().ContainKey("profile1")
                .WhichValue.Should().BeEquivalentTo(new List<QualityProfileCustomFormatScoreEntry>
                {
                    new(testConfigData[0].CustomFormats[0], 50)
                });

            processor.CustomFormatsWithoutScore.Should().BeEmpty();
        }

        [Test]
        public void Process_NoScoreInConfigOrGuide_UseZeroScore()
        {
            var testConfigData = new List<ProcessedConfigData>
            {
                new()
                {
                    CustomFormats = new List<ProcessedCustomFormatData>
                    {
                        new() {Name = "name1", Score = null, TrashId = "id1"}
                    },
                    QualityProfiles = new List<QualityProfileConfig>
                    {
                        new() {Name = "profile1"}
                    }
                }
            };

            var processor = new QualityProfileProcessor();
            processor.Process(testConfigData);

            processor.ProfileScores.Should().ContainKey("profile1")
                .WhichValue.Should().BeEquivalentTo(new List<QualityProfileCustomFormatScoreEntry>
                {
                    new(testConfigData[0].CustomFormats[0], 0)
                });

            processor.CustomFormatsWithoutScore.Should().Equal(new List<object> {("name1", "id1", "profile1")});
        }

        [Test]
        public void Process_OnlyGuideHasScores_UseGuideScore()
        {
            var testConfigData = new List<ProcessedConfigData>
            {
                new()
                {
                    CustomFormats = new List<ProcessedCustomFormatData>
                    {
                        new() {Score = 100, TrashId = "id1"}
                    },
                    QualityProfiles = new List<QualityProfileConfig>
                    {
                        new() {Name = "profile1"},
                        new() {Name = "profile2", Score = 0}
                    }
                }
            };

            var processor = new QualityProfileProcessor();
            processor.Process(testConfigData);

            var expectedScoreEntries = new List<QualityProfileCustomFormatScoreEntry>
            {
                new(testConfigData[0].CustomFormats[0], 100)
            };

            processor.ProfileScores.Should().BeEquivalentTo(
                new Dictionary<string, List<QualityProfileCustomFormatScoreEntry>>
                {
                    {"profile1", expectedScoreEntries},
                    {"profile2", expectedScoreEntries}
                });

            processor.CustomFormatsWithoutScore.Should().BeEmpty();
        }
    }
}
