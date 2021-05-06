using System.Collections.Generic;
using System.Threading.Tasks;
using Trash.Radarr.CustomFormat.Models;

namespace Trash.Radarr.CustomFormat
{
    internal interface ICustomFormatGuideProcessor
    {
        List<ProcessedCustomFormatData> ProcessedCustomFormats { get; }
        List<string> CustomFormatsNotInGuide { get; }
        List<ProcessedConfigData> ConfigData { get; }
        Dictionary<string, List<QualityProfileCustomFormatScoreEntry>> ProfileScores { get; }
        List<(string name, string trashId, string profileName)> CustomFormatsWithoutScore { get; }
        Task BuildGuideData(IReadOnlyList<CustomFormatConfig> config);
    }
}
