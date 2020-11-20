namespace Hake.Extension.Cache
{
    public class CacheValue<TValue>
    {
        public bool AddToCache { get; }

        public TValue Value { get; }

        private CacheValue(bool addToCache, TValue value)
        {
            AddToCache = addToCache;
            Value = value;
        }

        public static CacheValue<TValue> From(TValue result) => new CacheValue<TValue>(true, result);

        public static CacheValue<TValue> DontAdd(TValue result) => new CacheValue<TValue>(false, result);
    }
}
