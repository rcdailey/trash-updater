using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;
using Serilog;
using Trash.Extensions;
using Trash.Radarr.CustomFormat.Models;
using Trash.Radarr.CustomFormat.Models.Cache;

namespace Trash.Radarr.CustomFormat
{
    public class CustomFormatJsonMerger
    {
        public enum MergeResult
        {
            Created,
            Updated,
            Deleted
        }

        public CustomFormatJsonMerger(ILogger logger)
        {
            Log = logger;
        }

        public IDictionary<MergeResult, List<JObject>> MergedCustomFormats { get; } =
            new Dictionary<MergeResult, List<JObject>>();

        private ILogger Log { get; }

        public void MergeJson(IEnumerable<ProcessedCustomFormatData> processedCustomFormats,
            IReadOnlyCollection<JObject> radarrCfs)
        {
            foreach (var guideCf in processedCustomFormats)
            {
                // Try to find match in cache first and in guide by name second
                var radarrCf =
                    radarrCfs.FirstOrDefault(rcf => guideCf.CacheEntry?.CustomFormatId == rcf["id"].Value<int>()) ??
                    radarrCfs.FirstOrDefault(rcf => guideCf.Name.EqualsIgnoreCase(rcf["name"].Value<string>()));

                var guideCfJson = BuildNewRadarrCf(guideCf.Json);

                // no match; we add this CF as brand new
                if (radarrCf == null)
                {
                    MergedCustomFormats[MergeResult.Created].Add(guideCfJson);
                }
                // found match in radarr CFs; update the existing CF
                else
                {
                    UpdateRadarrCf(radarrCf, guideCfJson);

                    var cfName = radarrCf["name"].Value<string>();

                    if (!JToken.DeepEquals(radarrCf, radarrCf))
                    {
                        MergedCustomFormats[MergeResult.Updated].Add(radarrCf);
                    }
                    else
                    {
                        Log.Debug("Skipping update of existing CF because there's nothing to update: {Name}", cfName);
                    }
                }
            }
        }

        private static void UpdateRadarrCf(JObject radarrCf, JObject guideCfJson)
        {
            MergeProperties(radarrCf, guideCfJson, JTokenType.Array);

            var radarrSpecs = radarrCf["specifications"].Children<JObject>();
            var guideSpecs = guideCfJson["specifications"].Children<JObject>();

            var newRadarrSpecs = new JArray();

            foreach (var pair in guideSpecs.GroupBy(gs => radarrSpecs.FirstOrDefault(gss => KeyMatch(gss, gs, "name")))
                .SelectMany(kvp => kvp.Select(gs => new {RadarrSpec = kvp.Key, GuideSpec = gs})))
            {
                if (pair.RadarrSpec != null)
                {
                    MergeProperties(pair.RadarrSpec, pair.GuideSpec);
                    newRadarrSpecs.Add(pair.RadarrSpec);
                }
                else
                {
                    newRadarrSpecs.Add(pair.GuideSpec);
                }
            }

            radarrCf["specifications"] = newRadarrSpecs;
        }

        private static bool KeyMatch(JObject left, JObject right, string keyName)
            => left[keyName].Value<string>() == right[keyName].Value<string>();

        private static void MergeProperties(JObject radarrCf, JObject guideCfJson,
            JTokenType exceptType = JTokenType.None)
        {
            foreach (var guideProp in guideCfJson.Properties().Where(p => p.Value.Type != exceptType))
            {
                radarrCf[guideProp.Name] = guideProp.Value;
            }
        }

        private static JObject BuildNewRadarrCf(string jsonPayload)
        {
            // Information on required fields from nitsua
            /*
                ok, for the specs.. you need name, implementation, negate, required, fields
                for fields you need name & value
                top level you need name, includeCustomFormatWhenRenaming, specs and id (if updating)
                everything else radarr can handle with backend logic
             */

            var cf = JObject.Parse(jsonPayload);
            foreach (var child in cf["specifications"].Children())
            {
                var field = child["fields"];
                field["name"] = "value";
                child["fields"] = new JArray {field};
            }

            return cf;
        }
    }
}
