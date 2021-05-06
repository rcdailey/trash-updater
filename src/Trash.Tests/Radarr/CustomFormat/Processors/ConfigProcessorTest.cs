using System.Collections.Generic;
using FluentAssertions;
using Newtonsoft.Json;
using NUnit.Framework;
using Trash.Radarr;
using Trash.Radarr.CustomFormat.Models;
using Trash.Radarr.CustomFormat.Models.Cache;
using Trash.Radarr.CustomFormat.Processors;

namespace Trash.Tests.Radarr.CustomFormat.Processors
{
    [TestFixture]
    [Parallelizable(ParallelScope.All)]
    public class ConfigProcessorTest
    {
        [Test]
        public void Process_AllCfsFoundInGuide_NoIssues()
        {
            var testProcessedCfs = new List<ProcessedCustomFormatData>
            {
                new()
                {
                    Json = JsonConvert.SerializeObject(new {name = "name1"}, Formatting.Indented),
                    Name = "name1",
                    Score = 100,
                    TrashId = "id1"
                },
                new()
                {
                    Json = JsonConvert.SerializeObject(new {name = "name3"}, Formatting.Indented),
                    Name = "name3",
                    Score = null,
                    TrashId = "id3"
                }
            };

            var testConfig = new CustomFormatConfig[]
            {
                new()
                {
                    Names = new List<string> {"name1", "name3"},
                    QualityProfiles = new List<QualityProfileConfig>
                    {
                        new() {Name = "profile1", Score = 50}
                    }
                }
            };

            var processor = new ConfigProcessor();
            processor.Process(testProcessedCfs, testConfig);

            processor.RenamedCustomFormats.Should().BeEmpty();
            processor.CustomFormatsNotInGuide.Should().BeEmpty();
            processor.ConfigData.Should().BeEquivalentTo(new List<ProcessedConfigData>
            {
                new()
                {
                    CustomFormats = testProcessedCfs,
                    QualityProfiles = testConfig[0].QualityProfiles
                }
            });
        }

        [Test]
        public void Process_CachedCfsDifferentTrashIdSameName_AreInRenamedList()
        {
            var testProcessedCfs = new List<ProcessedCustomFormatData>
            {
                new()
                {
                    Name = "name1",
                    TrashId = "id1",
                    CacheEntry = new TrashIdMapping
                    {
                        TrashId = "id1",
                        CustomFormatName = "name2"
                    }
                },
                new()
                {
                    Name = "name2",
                    TrashId = "id2",
                    CacheEntry = new TrashIdMapping
                    {
                        TrashId = "id2",
                        CustomFormatName = "name1"
                    }
                }
            };

            var testConfig = new CustomFormatConfig[]
            {
                new()
                {
                    Names = new List<string> {"name1", "name2"}
                }
            };

            var processor = new ConfigProcessor();
            processor.Process(testProcessedCfs, testConfig);

            processor.RenamedCustomFormats.Should().BeEquivalentTo(testProcessedCfs);
            processor.CustomFormatsNotInGuide.Should().BeEmpty();
            processor.ConfigData.Should().BeEquivalentTo(new List<ProcessedConfigData>
            {
                new()
                {
                    CustomFormats = testProcessedCfs
                }
            });
        }

        [Test]
        public void Process_CfsMissingFromConfig_AreSkipped()
        {
            var testProcessedCfs = new List<ProcessedCustomFormatData>
            {
                new() {Name = "name1"},
                new() {Name = "name2"}
            };

            var testConfig = new CustomFormatConfig[]
            {
                new()
                {
                    Names = new List<string> {"name1"}
                }
            };

            var processor = new ConfigProcessor();
            processor.Process(testProcessedCfs, testConfig);

            processor.RenamedCustomFormats.Should().BeEmpty();
            processor.CustomFormatsNotInGuide.Should().BeEmpty();
            processor.ConfigData.Should().BeEquivalentTo(new List<ProcessedConfigData>
            {
                new()
                {
                    CustomFormats = new List<ProcessedCustomFormatData>
                    {
                        new() {Name = "name1"}
                    }
                }
            });
        }

        [Test]
        public void Process_CfsMissingFromGuide_AreAddedToNotInGuideList()
        {
            var testProcessedCfs = new List<ProcessedCustomFormatData>
            {
                new() {Name = "name1"},
                new() {Name = "name2"}
            };

            var testConfig = new CustomFormatConfig[]
            {
                new()
                {
                    Names = new List<string> {"name1", "name3"}
                }
            };

            var processor = new ConfigProcessor();
            processor.Process(testProcessedCfs, testConfig);

            processor.RenamedCustomFormats.Should().BeEmpty();
            processor.CustomFormatsNotInGuide.Should().BeEquivalentTo(new List<string> {"name3"});
            processor.ConfigData.Should().BeEquivalentTo(new List<ProcessedConfigData>
            {
                new()
                {
                    CustomFormats = new List<ProcessedCustomFormatData>
                    {
                        new() {Name = "name1"}
                    }
                }
            });
        }

        [Test]
        public void Process_CfWithSameNameInCache_CacheNamesShouldMatchFirst()
        {
            var testProcessedCfs = new List<ProcessedCustomFormatData>
            {
                new()
                {
                    Json = JsonConvert.SerializeObject(new {name = "name1"}, Formatting.Indented),
                    Name = "name1",
                    Score = 100,
                    TrashId = "id1"
                },
                new()
                {
                    Json = JsonConvert.SerializeObject(new {name = "name3"}, Formatting.Indented),
                    Name = "name3",
                    Score = null,
                    TrashId = "id3",
                    CacheEntry = new TrashIdMapping
                    {
                        TrashId = "id3",
                        CustomFormatName = "name1"
                    }
                }
            };

            var testConfig = new CustomFormatConfig[]
            {
                new()
                {
                    Names = new List<string> {"name1"}
                }
            };

            var processor = new ConfigProcessor();
            processor.Process(testProcessedCfs, testConfig);

            processor.CustomFormatsNotInGuide.Should().BeEmpty();
            processor.ConfigData.Should().BeEquivalentTo(new List<ProcessedConfigData>
            {
                new()
                {
                    CustomFormats = new() { testProcessedCfs[1] }
                }
            });
        }
    }
}
