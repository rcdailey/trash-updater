using System.Collections.Generic;
using System.IO;

namespace Trash.Config
{
    public interface IConfigurationLoader<out T>
        where T : ServiceConfiguration
    {
        IEnumerable<T> Load(string propertyName, string configSection);
        IEnumerable<T> LoadFromStream(TextReader stream, string configSection);
        IEnumerable<T> LoadMany(IEnumerable<string> configFiles, string configSection);
    }
}
