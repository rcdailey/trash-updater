namespace Trash.Config
{
    public interface IConfigurationProvider
    {
        IServiceConfiguration ActiveConfiguration { get; set; }
    }
}
