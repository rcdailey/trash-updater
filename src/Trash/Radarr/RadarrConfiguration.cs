using Flurl;
using JetBrains.Annotations;
using Trash.Config;
using Trash.Radarr.QualityDefinition;

namespace Trash.Radarr
{
    [UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
    public class RadarrConfiguration : ServiceConfiguration
    {
        public QualityDefinitionConfig? QualityDefinition { get; init; }

        public override string BuildUrl()
        {
            return BaseUrl
                .AppendPathSegment("api/v3")
                .SetQueryParams(new {apikey = ApiKey});
        }
    }

    [UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
    public class QualityDefinitionConfig
    {
        public RadarrQualityDefinitionType Type { get; init; }
        public decimal PreferredRatio { get; set; } = 1.0m;
    }
}
