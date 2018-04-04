namespace Hake.Extension.Cache
{
    public class RetrivationResult<TValue>
    {
        public bool AddToCache { get; }
        public TValue Value { get; }

        private RetrivationResult(bool addToCache, TValue value)
        {
            AddToCache = addToCache;
            Value = value;
        }

        public static RetrivationResult<TValue> Create(TValue result) => new RetrivationResult<TValue>(true, result);
        public static RetrivationResult<TValue> SupressResult(TValue result) => new RetrivationResult<TValue>(false, result);
    }
}
