using Trash.Radarr.CustomFormat.Models.Cache;

namespace Trash.Radarr.CustomFormat.Models
{
    public class ProcessedCustomFormatData
    {
        public string Name { get; init; } = "";
        public string TrashId { get; init; } = "";
        public int? Score { get; init; }
        public string Json { get; set; } = "";
        public TrashIdMapping? CacheEntry { get; set; }

        public string CacheAwareName => CacheEntry?.CustomFormatName ?? Name;
    }
}
