using System;
using System.Data.HashFunction.FNV;
using System.IO;
using System.IO.Abstractions;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Trash.Config;

namespace Trash.Cache
{
    public class ServiceCache : IServiceCache
    {
        private static readonly Regex AllowedObjectNameCharacters = new(@"^\w+$", RegexOptions.Compiled);
        private readonly IServiceConfiguration _config;
        private readonly IFileSystem _fileSystem;
        private readonly IFNV1a _hash;
        private readonly ICacheStoragePath _storagePath;

        public ServiceCache(IFileSystem fileSystem, ICacheStoragePath storagePath, IServiceConfiguration config)
        {
            _fileSystem = fileSystem;
            _storagePath = storagePath;
            _config = config;
            _hash = FNV1aFactory.Instance.Create(FNVConfig.GetPredefinedConfig(32));
        }

        public T Load<T>()
        {
            var json = _fileSystem.File.ReadAllText(PathFromAttribute<T>());
            return JObject.Parse(json).ToObject<T>();
        }

        public void Save<T>(T obj)
        {
            var path = PathFromAttribute<T>();
            _fileSystem.Directory.CreateDirectory(Path.GetDirectoryName(path));
            _fileSystem.File.WriteAllText(path, JsonConvert.SerializeObject(obj, Formatting.Indented));
        }

        private static string GetCacheObjectNameAttribute<T>()
        {
            var attribute = typeof(T).GetCustomAttribute<CacheObjectNameAttribute>();
            if (attribute == null)
            {
                throw new ArgumentException($"{nameof(CacheObjectNameAttribute)} is missing on type {nameof(T)}");
            }

            return attribute.Name;
        }

        private string BuildServiceGuid()
        {
            return _hash.ComputeHash(Encoding.ASCII.GetBytes(_config.BaseUrl)).AsHexString();
        }

        private string PathFromAttribute<T>()
        {
            var objectName = GetCacheObjectNameAttribute<T>();
            if (!AllowedObjectNameCharacters.IsMatch(objectName))
            {
                throw new ArgumentException($"Object name '{objectName}' has unacceptable characters");
            }

            return Path.Join(_storagePath.Path, BuildServiceGuid(), objectName + ".json");
        }
    }
}
