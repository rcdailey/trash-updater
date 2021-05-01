namespace Trash.Config
{
    public abstract class ServiceConfiguration
    {
        public string BaseUrl { get; init; } = "";
        public string ApiKey { get; init; } = "";

        public abstract string BuildUrl();
    }
}
