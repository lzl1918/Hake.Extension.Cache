namespace Hake.Extension.Cache
{
    public interface ICache<TKey, TValue>
    {
        int Capacity { get; }
        int Count { get; }

#if TEST
        int TotalFetch { get; }
        int HitCount { get; }
#endif

        TValue Get(TKey key, CacheFallBack<TKey, TValue> fallback);
        void Clear();
    }
}
