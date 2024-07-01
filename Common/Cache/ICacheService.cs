namespace Common.Cache
{
    public interface ICacheService
    {
        T GetData<T>(string key);
        void SetData<T>(string key, T value, TimeSpan expiration);
        void RemoveData(string key);
    }
}
