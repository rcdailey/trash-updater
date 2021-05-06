using System.Collections.Generic;
using Common;
using FluentAssertions;
using Flurl.Http.Testing;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NSubstitute;
using NUnit.Framework;
using Serilog;
using Trash.Cache;
using Trash.Radarr;
using Trash.Radarr.CustomFormat;
using Trash.Radarr.CustomFormat.Guide;
using Trash.Radarr.CustomFormat.Models;

namespace Trash.Tests.Radarr.CustomFormat
{
    [TestFixture]
    [Parallelizable(ParallelScope.All)]
    public class CustomFormatGuideProcessorTest
    {
        private class Context
        {
            public Context()
            {
                Logger = new LoggerConfiguration()
                    .WriteTo.TestCorrelator()
                    .WriteTo.NUnitOutput()
                    .MinimumLevel.Debug()
                    .CreateLogger();

                Data = new ResourceDataReader(typeof(CustomFormatGuideProcessorTest), "Data");
                ServiceCache = Substitute.For<IServiceCache>();
            }

            public ILogger Logger { get; }
            public IServiceCache ServiceCache { get; }
            public ResourceDataReader Data { get; }

            public string ReadJson(string jsonFile)
            {
                var jsonData = Data.ReadData(jsonFile);
                return JToken.Parse(jsonData).ToString(Formatting.Indented);
            }
        }

        [Test]
        public void BuildGuideData_ProcessNormalMarkdown_GetExpectedBuilderOutput()
        {
            var ctx = new Context();
            var guideProcessor =
                new CustomFormatGuideProcessor(ctx.Logger, new CustomFormatGuideParser(ctx.Logger), ctx.ServiceCache);

            // simulate guide data
            using var testHttp = new HttpTest();
            testHttp.RespondWith(ctx.Data.ReadData("CF_Markdown1.md"));

            // Simulate user config in YAML
            var config = new List<CustomFormatConfig>
            {
                new()
                {
                    Names = new List<string> {"Surround SOUND", "DTS-HD/DTS:X", "no score", "not in guide 1"},
                    QualityProfiles = new List<QualityProfileConfig>
                    {
                        new() {Name = "profile1"},
                        new() {Name = "profile2", Score = -1234}
                    }
                },
                new()
                {
                    Names = new List<string> {"no score", "not in guide 2"},
                    QualityProfiles = new List<QualityProfileConfig>
                    {
                        new() {Name = "profile3"},
                        new() {Name = "profile4", Score = 5678}
                    }
                }
            };

            guideProcessor.BuildGuideData(config);

            var expectedProcessedCustomFormatData = new List<ProcessedCustomFormatData>
            {
                new()
                {
                    Json = ctx.ReadJson("ImportableCustomFormat1_Processed.json"),
                    Name = "Surround Sound",
                    Score = 500,
                    TrashId = "43bb5f09c79641e7a22e48d440bd8868"
                },
                new()
                {
                    Json = ctx.ReadJson("ImportableCustomFormat2_Processed.json"),
                    Name = "DTS-HD/DTS:X",
                    Score = 480,
                    TrashId = "4eb3c272d48db8ab43c2c85283b69744"
                },
                new()
                {
                    Json = JsonConvert.SerializeObject(new {name = "No Score"}, Formatting.Indented),
                    Name = "No Score",
                    Score = null,
                    TrashId = "abc"
                }
            };

            guideProcessor.ProcessedCustomFormats.Should().BeEquivalentTo(expectedProcessedCustomFormatData);

            guideProcessor.ConfigData.Should().BeEquivalentTo(new List<ProcessedConfigData>
            {
                new()
                {
                    CustomFormats = expectedProcessedCustomFormatData,
                    QualityProfiles = config[0].QualityProfiles
                },
                new()
                {
                    CustomFormats = expectedProcessedCustomFormatData.GetRange(2, 1),
                    QualityProfiles = config[1].QualityProfiles
                }
            });

            guideProcessor.CustomFormatsWithoutScore.Should()
                .Equal(new List<(string name, string trashId, string profileName)>
                {
                    ("No Score", "abc", "profile1"),
                    ("No Score", "abc", "profile3")
                });

            guideProcessor.CustomFormatsNotInGuide.Should().Equal(new List<string>
            {
                "not in guide 1", "not in guide 2"
            });

            guideProcessor.ProfileScores.Should()
                .BeEquivalentTo(new Dictionary<string, List<QualityProfileCustomFormatScoreEntry>>
                {
                    {
                        "profile1", new List<QualityProfileCustomFormatScoreEntry>
                        {
                            new(expectedProcessedCustomFormatData[0], 500),
                            new(expectedProcessedCustomFormatData[1], 480),
                            new(expectedProcessedCustomFormatData[2], 0)
                        }
                    },
                    {
                        "profile2", new List<QualityProfileCustomFormatScoreEntry>
                        {
                            new(expectedProcessedCustomFormatData[0], -1234),
                            new(expectedProcessedCustomFormatData[1], -1234),
                            new(expectedProcessedCustomFormatData[2], -1234)
                        }
                    },
                    {
                        "profile3", new List<QualityProfileCustomFormatScoreEntry>
                        {
                            new(expectedProcessedCustomFormatData[2], 0)
                        }
                    },
                    {
                        "profile4", new List<QualityProfileCustomFormatScoreEntry>
                        {
                            new(expectedProcessedCustomFormatData[2], 5678)
                        }
                    }
                });
        }
    }
}
