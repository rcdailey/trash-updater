using System;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Serilog;
using Trash.Cache;
using Trash.Extensions;
using Trash.Radarr.Api;
using Trash.Radarr.CustomFormat.Models.Cache;

namespace Trash.Radarr.CustomFormat
{
    internal class CustomFormatUpdater : ICustomFormatUpdater
    {
        private readonly IRadarrApi _api;
        private readonly IServiceCache _cache;
        private readonly ICustomFormatGuideProcessor _guideProcessor;

        public CustomFormatUpdater(ILogger log, ICustomFormatGuideProcessor guideProcessor, IRadarrApi api,
            IServiceCache cache)
        {
            Log = log;
            _guideProcessor = guideProcessor;
            _api = api;
            _cache = cache;
        }

        private ILogger Log { get; }

        public async Task Process(IRadarrCommand args, RadarrConfiguration config)
        {
            await _guideProcessor.BuildGuideData(config.CustomFormats);

            if (!ValidateDataAndCheckShouldProceed(config))
            {
                return;
            }

            if (args.Preview)
            {
                PreviewCustomFormats();
                return;
            }

            // todo: when processing quality profile response data, print a log line for each CF and the score it gets
            var radarrCfs = await _api.GetCustomFormats();



            // Log.Information("Updating Existing Custom Format: {Name}", cfName);

            // Log.Information("Creating New Custom Format: {Name}", guideCfJson["name"].Value<string>());

            // cache.TrashIdMappings.Add(new TrashIdMapping
            // {
            //     CustomFormatId = response["id"].Value<int>(),
            //     TrashId = guideCf.TrashId,
            //     CustomFormatName = guideCf.CacheAwareName
            // });
        }

        private bool ValidateDataAndCheckShouldProceed(RadarrConfiguration config)
        {
            if (_guideProcessor.CustomFormatsNotInGuide.Count > 0)
            {
                Log.Warning("The Custom Formats below do not exist in the guide and will " +
                            "be skipped. Names must match the 'name' field in the actual JSON, not the header in " +
                            "the guide! Either fix the names or remove them from your YAML config to resolve this " +
                            "warning");
                Log.Warning("{CfList}", _guideProcessor.CustomFormatsNotInGuide);
            }

            var cfsWithoutQualityProfiles = _guideProcessor.ConfigData
                .Where(d => d.QualityProfiles.Count == 0)
                .SelectMany(d => d.CustomFormats.Select(cf => cf.Name))
                .ToList();

            if (cfsWithoutQualityProfiles.Count > 0)
            {
                Log.Debug("These custom formats will be uploaded but are not associated to a quality profile in the " +
                          "config file: {UnassociatedCfs}", cfsWithoutQualityProfiles);
            }

            // No CFs are defined in this item, or they are all invalid. Skip this whole instance.
            if (_guideProcessor.ConfigData.Count == 0)
            {
                Log.Error("Guide processing yielded no custom formats for configured instance host {BaseUrl}",
                    config.BaseUrl);
                return false;
            }

            if (_guideProcessor.CustomFormatsWithoutScore.Count > 0)
            {
                Log.Warning("The below custom formats have no score in the guide or YAML " +
                            "config and will be skipped (remove them from your config or specify a " +
                            "score to fix this warning)");
                Log.Warning("{CfList}", _guideProcessor.CustomFormatsWithoutScore);
            }

            return true;
        }

        private void PreviewCustomFormats()
        {
            Console.WriteLine("");
            Console.WriteLine("=========================================================");
            Console.WriteLine("            >>> Custom Formats From Guide <<<            ");
            Console.WriteLine("=========================================================");
            Console.WriteLine("");

            const string format = "{0,-30} {1,-35}";
            Console.WriteLine(format, "Custom Format", "Trash ID");
            Console.WriteLine(string.Concat(Enumerable.Repeat('-', 1 + 30 + 35)));

            foreach (var cf in _guideProcessor.ProcessedCustomFormats)
            {
                Console.WriteLine(format, cf.Name, cf.TrashId);
            }

            Console.WriteLine("");
            Console.WriteLine("=========================================================");
            Console.WriteLine("      >>> Quality Profile Assignments & Scores <<<       ");
            Console.WriteLine("=========================================================");
            Console.WriteLine("");

            const string profileFormat = "{0,-18} {1,-20} {2,-8}";
            Console.WriteLine(profileFormat, "Profile", "Custom Format", "Score");
            Console.WriteLine(string.Concat(Enumerable.Repeat('-', 2 + 18 + 20 + 8)));

            foreach (var (profileName, scoreEntries) in _guideProcessor.ProfileScores)
            {
                Console.WriteLine(profileFormat, profileName, "", "");

                foreach (var scoreEntry in scoreEntries)
                {
                    var matchingCf = _guideProcessor.ProcessedCustomFormats
                        .FirstOrDefault(cf => cf.TrashId.EqualsIgnoreCase(scoreEntry.CustomFormat.TrashId));

                    if (matchingCf == null)
                    {
                        Log.Warning("Quality Profile refers to CF not found in guide: {TrashId}",
                            scoreEntry.CustomFormat.TrashId);
                        continue;
                    }

                    Console.WriteLine(profileFormat, "", matchingCf.Name, scoreEntry.Score);
                }
            }

            Console.WriteLine("");
        }
    }
}
