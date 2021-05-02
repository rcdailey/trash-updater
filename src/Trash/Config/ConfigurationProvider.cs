using System;

namespace Trash.Config
{
    internal class ConfigurationProvider : IConfigurationProvider
    {
        private IServiceConfiguration? _activeConfiguration;

        public IServiceConfiguration ActiveConfiguration
        {
            get => _activeConfiguration ?? throw new NullReferenceException("Active configuration has not been set");
            set => _activeConfiguration = value;
        }
    }
}
