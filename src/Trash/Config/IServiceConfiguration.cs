namespace Trash.Config
{
    public interface IServiceConfiguration
    {
        string BaseUrl { get; init; }
        string ApiKey { get; init; }
        string BuildUrl();
    }
}
