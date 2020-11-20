namespace Hake.Extension.Cache
{
    public delegate CacheValue<TValue> CacheFallBack<TKey, TValue>(TKey key);
}
