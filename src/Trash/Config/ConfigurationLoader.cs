using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;
using System.Linq;
using Trash.YamlDotNet;
using YamlDotNet.Core;
using YamlDotNet.Core.Events;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace Trash.Config
{
    public class ConfigurationLoader<T> : IConfigurationLoader<T>
        where T : ServiceConfiguration
    {
        private readonly IConfigurationProvider<T> _configProvider;
        private readonly IDeserializer _deserializer;
        private readonly IFileSystem _fileSystem;

        public ConfigurationLoader(IConfigurationProvider<T> configProvider, IFileSystem fileSystem,
            IObjectFactory objectFactory)
        {
            _configProvider = configProvider;
            _fileSystem = fileSystem;
            _deserializer = new DeserializerBuilder()
                .WithNamingConvention(UnderscoredNamingConvention.Instance)
                // .WithNamingConvention(CamelCaseNamingConvention.Instance)
                .WithTypeConverter(new YamlNullableEnumTypeConverter())
                .WithObjectFactory(objectFactory)
                .Build();
        }

        public IEnumerable<T> Load(string configPath, string configSection)
        {
            using var stream = _fileSystem.File.OpenText(configPath);
            return LoadFromStream(stream, configSection);
        }

        public IEnumerable<T> LoadFromStream(TextReader stream, string configSection)
        {
            var parser = new Parser(stream);
            parser.Consume<StreamStart>();
            parser.Consume<DocumentStart>();
            parser.Consume<MappingStart>();

            var configs = new List<T>();
            while (parser.TryConsume<Scalar>(out var key))
            {
                if (key.Value == configSection)
                {
                    configs = _deserializer.Deserialize<List<T>>(parser);
                }

                parser.SkipThisAndNestedEvents();
            }

            if (configs.Count == 0)
            {
                throw new ConfigurationException(configSection, typeof(T));
            }

            return configs;
        }

        public IEnumerable<T> LoadMany(IEnumerable<string> configFiles, string configSection)
        {
            foreach (var config in configFiles.SelectMany(file => Load(file, configSection)))
            {
                _configProvider.ActiveConfiguration = config;
                yield return config;
            }
        }
    }
}
