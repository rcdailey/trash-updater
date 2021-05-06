using System.Collections.Generic;
using Trash.Cache;

namespace Trash.Radarr.CustomFormat.Models.Cache
{
    [CacheObjectName("custom-format-cache")]
    public class CustomFormatCache
    {
        public List<TrashIdMapping> TrashIdMappings { get; init; } = new();
    }

    public class TrashIdMapping
    {
        public string CustomFormatName { get; init; } = "";
        public string TrashId { get; init; } = "";
        public int CustomFormatId { get; init; }
    }
}
