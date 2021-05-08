using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Serilog;
using Trash.Cache;
using Trash.Radarr.CustomFormat.Guide;
using Trash.Radarr.CustomFormat.Models;
using Trash.Radarr.CustomFormat.Models.Cache;
using Trash.Radarr.CustomFormat.Processors;

namespace Trash.Radarr.CustomFormat
{
    internal class CustomFormatGuideProcessor : ICustomFormatGuideProcessor
    {
        private readonly ICustomFormatGuideParser _guideParser;
        private readonly IServiceCache _cache;

        private readonly (
            CustomFormatProcessor CustomFormat,
            ConfigProcessor Config,
            QualityProfileProcessor QualityProfile
            ) _processors = new(new(), new(), new());

        private IList<CustomFormatData>? _guideData;

        public CustomFormatGuideProcessor(ILogger log, ICustomFormatGuideParser guideParser, IServiceCache cache)
        {
            _guideParser = guideParser;
            _cache = cache;
            Log = log;
        }

        private ILogger Log { get; }

        public IEnumerable<ProcessedCustomFormatData> ProcessedCustomFormats
            => _processors.CustomFormat.ProcessedCustomFormats;

        public List<string> CustomFormatsNotInGuide
            => _processors.Config.CustomFormatsNotInGuide;

        public List<ProcessedConfigData> ConfigData
            => _processors.Config.ConfigData;

        public Dictionary<string, List<QualityProfileCustomFormatScoreEntry>> ProfileScores
            => _processors.QualityProfile.ProfileScores;

        public List<(string name, string trashId, string profileName)> CustomFormatsWithoutScore
            => _processors.QualityProfile.CustomFormatsWithoutScore;

        public async Task BuildGuideData(IReadOnlyList<CustomFormatConfig> config)
        {
            if (_guideData == null)
            {
                Log.Debug("Requesting and parsing guide markdown");
                var markdownData = await _guideParser.GetMarkdownData();
                _guideData = _guideParser.ParseMarkdown(markdownData);
            }

            // Grab the cache if one is available
            var cache = _cache.Load<CustomFormatCache>();
            if (cache == null)
            {
                Log.Debug("Custom format cache does not exist; proceeding without it");
            }

            // Step 1: Process and filter the custom formats from the guide.
            // Custom formats in the guide not mentioned in the config are filtered out.
            _processors.CustomFormat.Process(_guideData, config, cache);

            // todo: Process cache entries that do not exist in the guide. Those should be deleted
            // This might get taken care of when we rebuild the cache based on what is actually updated when
            // we call the Radarr API

            // Step 2: Use the processed custom formats from step 1 to process the configuration.
            // CFs in config not in the guide are filtered out.
            // Actual CF objects are associated to the quality profile objects to reduce lookups
            _processors.Config.Process(_processors.CustomFormat.ProcessedCustomFormats, config);

            // Step 3: Use the processed config (which contains processed CFs) to process the quality profile scores.
            // Score precedence logic is utilized here to decide the CF score per profile (same CF can actually have
            // different scores depending on which profile it goes into).
            _processors.QualityProfile.Process(_processors.Config.ConfigData);
        }
    }
}
