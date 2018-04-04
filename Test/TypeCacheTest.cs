using Hake.Extension.Cache;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Test
{
    public class TypeComparer : IComparer<Type>
    {
        public int Compare(Type x, Type y)
        {
            return x.FullName.CompareTo(y.FullName);
        }
    }

    [TestClass]
    public class TypeCacheTest
    {
        [TestMethod]
        public void TestTypeCache()
        {
            CacheFallBack<Type, string> fallback = (key) =>
            {
                return RetrivationResult<string>.Create(key.FullName);
            };

            int times = 500;
            List<Type> data = GenerateData(10);

            ICache<Type, string> cache = new Cache<Type, string>(3, new TypeComparer());
            Random random = new Random(Guid.NewGuid().GetHashCode());
            while (times > 0)
            {
                int index = random.Next(0, data.Count);
                Type input = data[index];
                string result = cache.Get(input, fallback);
                Assert.AreEqual(input.FullName, result);
                times--;
            }
            int total = cache.TotalFetch;
            int hit = cache.HitCount;

        }

        private List<Type> GenerateData(int count)
        {
            Assembly assembly = Assembly.LoadFile(@"E:\Projects\Github\Hake.Extension.Cache\Test\bin\Test\netcoreapp2.0\Hake.Extension.Cache.dll");
            return assembly.GetTypes().Take(count).ToList();

        }
    }
}
