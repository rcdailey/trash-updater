using System.Collections.Generic;
using System.Threading.Tasks;
using Flurl;
using Flurl.Http;
using Trash.Config;
using Trash.Radarr.Api.Objects;

namespace Trash.Radarr.Api
{
    public class RadarrApi : IRadarrApi
    {
        private readonly IServiceConfiguration _serviceConfig;

        public RadarrApi(IServiceConfiguration serviceConfig)
        {
            _serviceConfig = serviceConfig;
        }

        public async Task<List<RadarrQualityDefinitionItem>> GetQualityDefinition()
        {
            return await BaseUrl()
                .AppendPathSegment("qualitydefinition")
                .GetJsonAsync<List<RadarrQualityDefinitionItem>>();
        }

        public async Task<IList<RadarrQualityDefinitionItem>> UpdateQualityDefinition(
            IList<RadarrQualityDefinitionItem> newQuality)
        {
            return await BaseUrl()
                .AppendPathSegment("qualityDefinition/update")
                .PutJsonAsync(newQuality)
                .ReceiveJson<List<RadarrQualityDefinitionItem>>();
        }

        public async Task<List<CustomFormatItem>> GetCustomFormats()
        {
            return await BaseUrl()
                .AppendPathSegment("customformat")
                .GetJsonAsync<List<CustomFormatItem>>();
        }

        private string BaseUrl()
        {
            return _serviceConfig.BuildUrl();
        }
    }
}
