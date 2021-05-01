namespace Trash.Config
{
    internal class ConfigurationProvider<T> : IConfigurationProvider<T>
        where T : ServiceConfiguration
    {
        public T? ActiveConfiguration { get; set; }
    }
}
