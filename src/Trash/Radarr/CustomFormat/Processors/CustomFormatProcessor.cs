using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;
using Trash.Extensions;
using Trash.Radarr.CustomFormat.Guide;
using Trash.Radarr.CustomFormat.Models;
using Trash.Radarr.CustomFormat.Models.Cache;

namespace Trash.Radarr.CustomFormat.Processors
{
    public class CustomFormatProcessor
    {
        public List<ProcessedCustomFormatData> ProcessedCustomFormats { get; private set; } = new();

        public void Process(IEnumerable<CustomFormatData> customFormatGuideData, IEnumerable<CustomFormatConfig> config,
            CustomFormatCache? cache)
        {
            var allConfigCfNames = config
                .SelectMany(c => c.Names)
                .Distinct(StringComparer.CurrentCultureIgnoreCase)
                .ToList();

            ProcessedCustomFormats = customFormatGuideData
                .Select(cf => ProcessCustomFormatData(cf, cache))
                .Where(pgd => allConfigCfNames.Any(n => pgd.CacheEntry != null || n.EqualsIgnoreCase(pgd.Name)))
                .ToList();
        }

        private static ProcessedCustomFormatData ProcessCustomFormatData(CustomFormatData guideData,
            CustomFormatCache? cache)
        {
            JObject obj = JObject.Parse(guideData.Json);

            var newData = new ProcessedCustomFormatData
            {
                Name = obj["name"].Value<string>(),
                TrashId = obj["trash_id"].Value<string>(),
                Score = guideData.Score
            };

            // Remove trash_id, it's metadata that is not meant for Radarr itself
            // Radarr supposedly drops this anyway, but I prefer it to be removed by TrashUpdater
            obj.Property("trash_id").Remove();

            // Serialize the JSON back out so that any modifications get saved
            newData.Json = obj.ToString();

            newData.CacheEntry =
                cache?.TrashIdMappings.FirstOrDefault(c => c.TrashId.EqualsIgnoreCase(newData.TrashId));

            return newData;
        }
    }
}
