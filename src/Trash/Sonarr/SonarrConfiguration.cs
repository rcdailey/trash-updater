using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Flurl;
using JetBrains.Annotations;
using Trash.Config;
using Trash.Sonarr.QualityDefinition;
using Trash.Sonarr.ReleaseProfile;

namespace Trash.Sonarr
{
    [UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
    public class SonarrConfiguration : ServiceConfiguration
    {
        public IList<ReleaseProfileConfig> ReleaseProfiles { get; set; } = new List<ReleaseProfileConfig>();
        public SonarrQualityDefinitionType? QualityDefinition { get; init; }

        public override string BuildUrl()
        {
            return BaseUrl
                .AppendPathSegment("api/v3")
                .SetQueryParams(new {apikey = ApiKey});
        }
    }

    [UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
    public class ReleaseProfileConfig
    {
        // -1 does not map to a valid enumerator. this is to force validation to fail if it is not set from YAML
        // all of this craziness is to avoid making the enum type nullable which will make using the property
        // frustrating.
        [EnumDataType(typeof(ReleaseProfileType),
            ErrorMessage = "'type' is required for 'release_profiles' elements")]
        public ReleaseProfileType Type { get; init; } = (ReleaseProfileType) (-1);

        public bool StrictNegativeScores { get; init; }
        public SonarrProfileFilterConfig Filter { get; init; } = new();
        public ICollection<string> Tags { get; init; } = new List<string>();
    }

    [UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
    public class SonarrProfileFilterConfig
    {
        public bool IncludeOptional { get; set; }
        // todo: Add Include & Exclude later (list of strings)
    }
}
