using System;
using System.IO;
using System.Threading.Tasks;
using CliFx.Attributes;
using Flurl.Http;
using JetBrains.Annotations;
using Serilog;
using Serilog.Core;
using Trash.Command;
using Trash.Config;
using Trash.Radarr.CustomFormat;
using Trash.Radarr.QualityDefinition;

namespace Trash.Radarr
{
    [Command("radarr", Description = "Perform operations on a Radarr instance")]
    [UsedImplicitly]
    public class RadarrCommand : ServiceCommand, IRadarrCommand
    {
        private readonly IConfigurationLoader<RadarrConfiguration> _configLoader;
        private readonly Func<RadarrQualityDefinitionUpdater> _qualityUpdaterFactory;
        private readonly Lazy<ICustomFormatUpdater> _customFormatUpdater;

        public RadarrCommand(
            ILogger logger,
            LoggingLevelSwitch loggingLevelSwitch,
            IConfigurationLoader<RadarrConfiguration> configLoader,
            Func<RadarrQualityDefinitionUpdater> qualityUpdaterFactory,
            Lazy<ICustomFormatUpdater> customFormatUpdater)
            : base(logger, loggingLevelSwitch)
        {
            _configLoader = configLoader;
            _qualityUpdaterFactory = qualityUpdaterFactory;
            _customFormatUpdater = customFormatUpdater;
        }

        public override string CacheStoragePath { get; } =
            Path.Join(AppPaths.AppDataPath, "cache/radarr");

        public override async Task Process()
        {
            try
            {
                foreach (var config in _configLoader.LoadMany(Config, "radarr"))
                {
                    if (config.QualityDefinition != null)
                    {
                        await _qualityUpdaterFactory().Process(this, config);
                    }

                    if (config.CustomFormats.Count > 0)
                    {
                        await _customFormatUpdater.Value.Process(this, config);
                    }
                }
            }
            catch (FlurlHttpException e)
            {
                Log.Error(e, "HTTP error while communicating with Radarr");
                ExitDueToFailure();
            }
        }
    }
}
