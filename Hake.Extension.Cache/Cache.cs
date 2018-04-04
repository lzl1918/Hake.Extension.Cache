using System;
using System.Collections.Generic;

namespace Hake.Extension.Cache
{
    public class Cache<TKey, TValue> : ICache<TKey, TValue>
    {
        private Dictionary<TKey, TValue> dictionary;
        private SortedList<TKey, CounterValue> counter;
        private Stack<CounterValue> counterPool;
        private TKey maxKey;
        private CounterValue maxCounter;
        private object locker;

        private int capacity;

#if TEST
        public int TotalFetch { get; private set; }
        public int HitCount { get; private set; }
#endif

        public int Capacity => capacity;
        public int Count => dictionary.Count;

        public Cache(int capacity, IComparer<TKey> keyComparer, IEqualityComparer<TKey> keyEqualComparer)
        {
            locker = new object();
            this.capacity = capacity;
            dictionary = new Dictionary<TKey, TValue>(capacity: capacity, comparer: keyEqualComparer);
            counter = new SortedList<TKey, CounterValue>(capacity: capacity, comparer: keyComparer);
            counterPool = new Stack<CounterValue>(capacity: capacity);
            int i = 0;
            while (i < capacity)
            {
                counterPool.Push(new CounterValue());
                i++;
            }
        }
        public Cache(int capacity, IComparer<TKey> keyComparer)
        {
            locker = new object();
            this.capacity = capacity;
            dictionary = new Dictionary<TKey, TValue>(capacity: capacity);
            counter = new SortedList<TKey, CounterValue>(capacity: capacity, comparer: keyComparer);
            counterPool = new Stack<CounterValue>(capacity: capacity);
            int i = 0;
            while (i < capacity)
            {
                counterPool.Push(new CounterValue());
                i++;
            }
        }
        public Cache(int capacity, IEqualityComparer<TKey> keyEqualComparer)
        {
            locker = new object();
            this.capacity = capacity;
            dictionary = new Dictionary<TKey, TValue>(capacity: capacity, comparer: keyEqualComparer);
            counter = new SortedList<TKey, CounterValue>(capacity: capacity);
            counterPool = new Stack<CounterValue>(capacity: capacity);
            int i = 0;
            while (i < capacity)
            {
                counterPool.Push(new CounterValue());
                i++;
            }
        }

        public Cache(int capacity)
        {
            locker = new object();
            this.capacity = capacity;
            dictionary = new Dictionary<TKey, TValue>(capacity: capacity);
            counter = new SortedList<TKey, CounterValue>(capacity: capacity);
            counterPool = new Stack<CounterValue>(capacity: capacity);
            int i = 0;
            while (i < capacity)
            {
                counterPool.Push(new CounterValue());
                i++;
            }
        }

        public TValue Get(TKey key, CacheFallBack<TKey, TValue> fallback)
        {
            lock (locker)
            {
#if TEST
                TotalFetch++;
#endif
                if (dictionary.TryGetValue(key, out TValue value))
                {
#if TEST
                    HitCount++;
#endif
                    UpdateCounter(key);
                    return value;
                }

                if (fallback == null)
                    throw new ArgumentNullException(nameof(fallback));

                RetrivationResult<TValue> retrivationResult = fallback(key);
                if (!retrivationResult.AddToCache)
                    return retrivationResult.Value;

                TValue result = retrivationResult.Value;
                CounterValue currentCounter;
                if (dictionary.Count >= capacity)
                {
                    dictionary.Remove(maxKey);
                    counter.Remove(maxKey);
                    currentCounter = maxCounter;
                    currentCounter.ResetCounter();
                    UpdateCounterAfterFail();
                    dictionary.Add(key, result);
                    counter.Add(key, currentCounter);
                }
                else
                {
                    UpdateCounterAfterFail();
                    dictionary.Add(key, result);
                    currentCounter = counterPool.Pop();
                    currentCounter.ResetCounter();
                    counter.Add(key, currentCounter);
                }
                return result;
            }
        }

        private void UpdateCounter(TKey hitKey)
        {
            CounterValue value = counter[hitKey];
            int count = value.Counter;
            value.ResetCounter();
            maxKey = hitKey;
            maxCounter = value;
            int maxCount = 0;
            int valueCount;
            foreach (var pair in counter)
            {
                if (pair.Key.Equals(hitKey))
                    continue;

                value = pair.Value;
                valueCount = value.Counter;
                if (valueCount < count)
                    value.AddCounter();

                if (valueCount > maxCount)
                {
                    maxKey = pair.Key;
                    maxCount = valueCount;
                    maxCounter = value;
                }
            }
        }
        private void UpdateCounterAfterFail()
        {
            CounterValue value;
            int maxCount = -1;
            int valueCount;
            foreach (var pair in counter)
            {
                value = pair.Value;
                valueCount = value.Counter;
                value.AddCounter();

                if (valueCount > maxCount)
                {
                    maxKey = pair.Key;
                    maxCount = valueCount;
                    maxCounter = value;
                }
            }
        }

        public void Clear()
        {
            lock (locker)
            {
                dictionary.Clear();
                foreach (var pair in counter)
                {
                    counterPool.Push(pair.Value);
                }
                counter.Clear();
            }
        }
    }
}
