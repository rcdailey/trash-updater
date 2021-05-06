using System.Collections.Generic;
using System.Threading.Tasks;
using Trash.Radarr.Api.Objects;

namespace Trash.Radarr.Api
{
    public interface IRadarrApi
    {
        Task<List<RadarrQualityDefinitionItem>> GetQualityDefinition();
        Task<IList<RadarrQualityDefinitionItem>> UpdateQualityDefinition(IList<RadarrQualityDefinitionItem> newQuality);
        Task<List<CustomFormatItem>> GetCustomFormats();
    }
}
