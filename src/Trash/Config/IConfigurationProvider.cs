namespace Trash.Config
{
    public interface IConfigurationProvider<T>
        where T : ServiceConfiguration
    {
        T? ActiveConfiguration { get; set; }
    }
}
