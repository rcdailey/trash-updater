using System;
using System.Collections.Generic;
using System.Linq;
using Trash.Extensions;
using Trash.Radarr.CustomFormat.Models;

namespace Trash.Radarr.CustomFormat.Processors
{
    public class ConfigProcessor
    {
        public List<ProcessedCustomFormatData> RenamedCustomFormats { get; private set; } = new();
        public List<string> CustomFormatsNotInGuide { get; } = new();
        public List<ProcessedConfigData> ConfigData { get; } = new();

        public void Process(IReadOnlyCollection<ProcessedCustomFormatData> processedCfs, IEnumerable<CustomFormatConfig> config)
        {
            foreach (var configCf in config)
            {
                // Also get the list of CFs that are in the guide
                var cfsInGuide = configCf.Names
                    .ToLookup(n =>
                    {
                        // Iterate up to two times:
                        // 1. Find a match in the cache using name in config. If not found,
                        // 2. Find a match in the guide using name in config.
                        return processedCfs.FirstOrDefault(
                                   cf => cf.CacheEntry?.CustomFormatName.EqualsIgnoreCase(n) ?? false) ??
                               processedCfs.FirstOrDefault(
                                   cf => cf.Name.EqualsIgnoreCase(n));
                    });

                // Names grouped under 'null' were not found in the guide OR the cache
                CustomFormatsNotInGuide.AddRange(
                    cfsInGuide[null].Distinct(StringComparer.CurrentCultureIgnoreCase));

                ConfigData.Add(new ProcessedConfigData
                {
                    CustomFormats = cfsInGuide.Where(grp => grp.Key != null).Select(grp => grp.Key!).ToList(),
                    QualityProfiles = configCf.QualityProfiles
                });
            }

            var allCfs = ConfigData
                .SelectMany(cd => cd.CustomFormats.Select(cf => cf))
                .Distinct()
                .ToList();

            // List of CFs in cache vs guide that have mismatched Trash ID. This means that a CF was renamed
            // to the same name as a previous CF's name, and we should treat that one as missing.
            // CustomFormatsSameNameDiffTrashId = allCfs
            //     .Where(cf => cf.CacheEntry != null)
            //     .GroupBy(cf => allCfs.FirstOrDefault(
            //         cf2 => cf2.Name.EqualsIgnoreCase(cf.CacheEntry!.CustomFormatName) &&
            //                !cf2.TrashId.EqualsIgnoreCase(cf.CacheEntry.TrashId)))
            //     .Where(grp => grp.Key != null)
            //     .Select(grp => grp.Append(grp.Key!).ToList())
            //     .ToList();

            // CFs in the guide that match the same TrashID in cache but have different names. Warn the user that it
            // is renamed in the guide and they need to update their config.
            RenamedCustomFormats = allCfs
                .Where(cf => cf.CacheEntry != null && !cf.CacheEntry.CustomFormatName.EqualsIgnoreCase(cf.Name))
                .ToList();
        }
    }
}
