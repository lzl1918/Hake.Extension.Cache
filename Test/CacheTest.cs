using Hake.Extension.Cache;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;

namespace Test
{
    [TestClass]
    public class CacheTest
    {
        [TestMethod]
        public void TestCache()
        {
            CacheFallBack<string, string> fallback = (key) => CacheValue<string>.From(key);

            int times = 500;
            List<string> data = GenerateData(100);

            ICache<string, string> cache = new Cache<string, string>(50);
            Random random = new Random(Guid.NewGuid().GetHashCode());
            while (times > 0)
            {
                int index = random.Next(0, data.Count);
                string input = data[index];
                string result = cache.Get(input, fallback);
                Assert.AreEqual(input, result);
                times--;
            }
            int total = cache.TotalFetch;
            int hit = cache.HitCount;
            Console.WriteLine($"Hit rate: {hit * 1.0 / total}");
        }

        private List<string> GenerateData(int count)
        {
            List<string> list = new List<string>();

            while (count > 0)
            {
                list.Add(RandomString());
                count--;
            }
            return list;
        }

        private string RandomString()
        {
            return Guid.NewGuid().ToString();
        }
    }
}
