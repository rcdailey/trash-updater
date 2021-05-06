using System.Threading.Tasks;

namespace Trash.Radarr.CustomFormat
{
    public interface ICustomFormatUpdater
    {
        Task Process(IRadarrCommand args, RadarrConfiguration config);
    }
}
