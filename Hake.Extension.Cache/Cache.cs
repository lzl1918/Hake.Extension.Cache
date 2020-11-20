using System;
using System.Collections.Generic;

namespace Hake.Extension.Cache
{
    public class Cache<TKey, TValue> : ICache<TKey, TValue>
    {
        private readonly Dictionary<TKey, TValue> mDictionary;
        private readonly Dictionary<TKey, Counter> mUsageCounters;
        private readonly Stack<Counter> mCounterPool;

        private TKey mMaxKey;
        private Counter mMaxCounter;

        private readonly object mLocker;

        private readonly int mCapacity;

#if TEST
        public int TotalFetch { get; private set; }
        public int HitCount { get; private set; }
#endif

        public int Capacity => mCapacity;

        public int Count => mDictionary.Count;

        public Cache(int capacity, IEqualityComparer<TKey> keyComparer)
        {
            mLocker = new object();
            mCapacity = capacity;
            mDictionary = new Dictionary<TKey, TValue>(capacity: capacity, comparer: keyComparer);
            mUsageCounters = new Dictionary<TKey, Counter>(capacity: capacity, comparer: keyComparer);
            mCounterPool = new Stack<Counter>(capacity: capacity);
            for (int i = 0; i < capacity; i++)
            {
                mCounterPool.Push(new Counter());
            }
        }

        public Cache(int capacity) : this(capacity, null) { }

        public TValue Get(TKey key, CacheFallBack<TKey, TValue> fallback)
        {
            lock (mLocker)
            {
#if TEST
                TotalFetch++;
#endif
                if (mDictionary.TryGetValue(key, out TValue value))
                {
#if TEST
                    HitCount++;
#endif
                    UpdateCounter(key);
                    return value;
                }

                if (fallback == null)
                {
                    throw new ArgumentNullException(nameof(fallback));
                }

                CacheValue<TValue> cacheValue = fallback(key);
                if (!cacheValue.AddToCache)
                {
                    return cacheValue.Value;
                }

                TValue result = cacheValue.Value;
                Counter currentCounter;
                if (mDictionary.Count >= mCapacity)
                {
                    mDictionary.Remove(mMaxKey);
                    mUsageCounters.Remove(mMaxKey);
                    currentCounter = mMaxCounter;
                    currentCounter.Reset();
                    IncreaseAllCounters();
                    FindMaxKey();
                    mDictionary.Add(key, result);
                    mUsageCounters.Add(key, currentCounter);
                }
                else
                {
                    IncreaseAllCounters();
                    FindMaxKey();
                    mDictionary.Add(key, result);
                    currentCounter = mCounterPool.Pop();
                    currentCounter.Reset();
                    mUsageCounters.Add(key, currentCounter);
                }
                return result;
            }
        }

        private void UpdateCounter(TKey hitKey)
        {
            Counter hitCounter = mUsageCounters[hitKey];
            int hitCount = hitCounter.Count;
            hitCounter.Reset();
            mMaxKey = hitKey;
            mMaxCounter = hitCounter;
            int maxCount = 0;
#if NET5_0
            foreach ((TKey key, Counter counter) in mUsageCounters)
            {
#else
            foreach (KeyValuePair<TKey, Counter> pair in mUsageCounters)
            {
                TKey key = pair.Key;
                Counter counter = pair.Value;
#endif
                if (key.Equals(hitKey))
                {
                    continue;
                }

                int valueCount = counter.Count;
                if (valueCount < hitCount)
                {
                    counter.Increase();
                }

                if (valueCount > maxCount)
                {
                    mMaxKey = key;
                    maxCount = valueCount;
                    mMaxCounter = counter;
                }
            }
        }

        private void IncreaseAllCounters()
        {
            foreach (Counter counter in mUsageCounters.Values)
            {
                counter.Increase();
            }
        }

        private void FindMaxKey()
        {
            int maxCount = 0;
#if NET5_0
            foreach ((TKey key, Counter counter) in mUsageCounters)
            {
#else
            foreach (KeyValuePair<TKey, Counter> pair in mUsageCounters)
            {
                TKey key = pair.Key;
                Counter counter = pair.Value;
#endif
                int valueCount = counter.Count;
                if (valueCount > maxCount)
                {
                    mMaxKey = key;
                    maxCount = valueCount;
                    mMaxCounter = counter;
                }
            }
        }

        public void Clear()
        {
            lock (mLocker)
            {
                mDictionary.Clear();
                foreach (Counter counter in  mUsageCounters.Values)
                {
                    mCounterPool.Push(counter);
                }
                mUsageCounters.Clear();
            }
        }
    }
}
