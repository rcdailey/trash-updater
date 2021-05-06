using System.IO.Abstractions;
using System.Reflection;
using Autofac;
using CliFx;
using Serilog;
using Serilog.Core;
using Trash.Cache;
using Trash.Command;
using Trash.Config;
using Trash.Radarr.Api;
using Trash.Radarr.CustomFormat;
using Trash.Radarr.CustomFormat.Guide;
using Trash.Radarr.QualityDefinition;
using Trash.Sonarr.Api;
using Trash.Sonarr.QualityDefinition;
using Trash.Sonarr.ReleaseProfile;
using YamlDotNet.Serialization;

namespace Trash
{
    public static class CompositionRoot
    {
        private static void SetupLogging(ContainerBuilder builder)
        {
            builder.RegisterType<LoggingLevelSwitch>().SingleInstance();
            builder.Register(c =>
                {
                    const string template = "[{Level:u3}] {Message:lj}{NewLine}{Exception}";
                    return new LoggerConfiguration()
                        .MinimumLevel.ControlledBy(c.Resolve<LoggingLevelSwitch>())
                        .WriteTo.Console(outputTemplate: template)
                        .CreateLogger();
                })
                .As<ILogger>();
        }

        private static void SonarrRegistrations(ContainerBuilder builder)
        {
            builder.RegisterType<SonarrApi>().As<ISonarrApi>();

            // Release Profile Support
            builder.RegisterType<ReleaseProfileUpdater>();
            builder.RegisterType<ReleaseProfileGuideParser>().As<IReleaseProfileGuideParser>();

            // Quality Definition Support
            builder.RegisterType<SonarrQualityDefinitionUpdater>();
            builder.RegisterType<SonarrQualityDefinitionGuideParser>().As<ISonarrQualityDefinitionGuideParser>();
        }

        private static void RadarrRegistrations(ContainerBuilder builder)
        {
            builder.RegisterType<RadarrApi>().As<IRadarrApi>();

            // Quality Definition Support
            builder.RegisterType<RadarrQualityDefinitionUpdater>();
            builder.RegisterType<RadarrQualityDefinitionGuideParser>().As<IRadarrQualityDefinitionGuideParser>();

            // Custom Format Support
            builder.RegisterType<CustomFormatUpdater>().As<ICustomFormatUpdater>();
            builder.RegisterType<CustomFormatGuideParser>().As<ICustomFormatGuideParser>();
            builder.RegisterType<CustomFormatGuideProcessor>().As<ICustomFormatGuideProcessor>();
        }

        private static void ConfigurationRegistrations(ContainerBuilder builder)
        {
            builder.RegisterType<ObjectFactory>()
                .As<IObjectFactory>();

            builder.RegisterGeneric(typeof(ConfigurationLoader<>))
                .As(typeof(IConfigurationLoader<>));

            builder.RegisterType<ConfigurationProvider>()
                .As<IConfigurationProvider>()
                .SingleInstance();

            builder.Register(c => c.Resolve<IConfigurationProvider>().ActiveConfiguration)
                .As<IServiceConfiguration>();
        }

        private static void CommandRegistrations(ContainerBuilder builder)
        {
            // Register all types deriving from CliFx's ICommand. These are all of our supported subcommands.
            builder.RegisterAssemblyTypes(Assembly.GetExecutingAssembly())
                .Where(t => t.IsAssignableTo(typeof(ICommand)));

            // Used to access the chosen command class. This is assigned from CliTypeActivator
            builder.RegisterType<ActiveServiceCommandProvider>()
                .As<IActiveServiceCommandProvider>()
                .SingleInstance();

            builder.Register(c => c.Resolve<IActiveServiceCommandProvider>().ActiveCommand)
                .As<IServiceCommand>();
        }

        public static IContainer Setup()
        {
            return Setup(new ContainerBuilder());
        }

        public static IContainer Setup(ContainerBuilder builder)
        {
            builder.RegisterType<FileSystem>()
                .As<IFileSystem>();

            builder.RegisterType<ServiceCache>().As<IServiceCache>();
            builder.RegisterType<CacheStoragePath>().As<ICacheStoragePath>();

            ConfigurationRegistrations(builder);
            CommandRegistrations(builder);

            SetupLogging(builder);
            SonarrRegistrations(builder);
            RadarrRegistrations(builder);

            // builder.RegisterSource(new AnyConcreteTypeNotAlreadyRegisteredSource());
            return builder.Build();
        }
    }
}
