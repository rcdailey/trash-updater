using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
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
        public List<CustomFormatConfig> CustomFormats { get; set; } = new();

        public override string BuildUrl()
        {
            return BaseUrl
                .AppendPathSegment("api/v3")
                .SetQueryParams(new {apikey = ApiKey});
        }
    }

    [UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
    public class CustomFormatConfig
    {
        public List<string> Names { get; set; } = new();
        public List<QualityProfileConfig> QualityProfiles { get; set; } = new();
    }

    [UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
    public class QualityProfileConfig
    {
        [Required]
        public string Name { get; set; } = "";

        public int Score { get; set; }
    }

    [UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
    public class QualityDefinitionConfig
    {
        // -1 does not map to a valid enumerator. this is to force validation to fail if it is not set from YAML
        // all of this craziness is to avoid making the enum type nullable which will make using the property
        // frustrating.
        [EnumDataType(typeof(RadarrQualityDefinitionType),
            ErrorMessage = "'type' is required for 'quality_definition'")]
        public RadarrQualityDefinitionType Type { get; init; } = (RadarrQualityDefinitionType) (-1);

        public decimal PreferredRatio { get; set; } = 1.0m;
    }
}
