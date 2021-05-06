namespace Trash.Cache
{
    public interface IServiceCache
    {
        T? Load<T>() where T : class;
        void Save<T>(T obj) where T : class;
    }
}
