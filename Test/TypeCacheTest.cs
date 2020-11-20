using Hake.Extension.Cache;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Test
{
    public class TypeComparer : IEqualityComparer<Type>
    {
        public bool Equals(Type x, Type y)
        {
            return x.AssemblyQualifiedName.Equals(y.AssemblyQualifiedName);
        }

        public int GetHashCode(Type obj)
        {
            return obj.AssemblyQualifiedName.GetHashCode();
        }
    }

    [TestClass]
    public class TypeCacheTest
    {
        [TestMethod]
        public void TestTypeCache()
        {
            CacheFallBack<Type, string> fallback = (key) => CacheValue<string>.From(key.FullName);

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

            Console.WriteLine($"Hit rate: {hit * 1.0 / total}");
        }

        private List<Type> GenerateData(int count)
        {
            string assemblyPath = Path.Combine(Directory.GetCurrentDirectory(), "Hake.Extension.Cache.dll");
            Assembly assembly = Assembly.LoadFile(assemblyPath);
            return assembly.GetTypes().Take(count).ToList();
        }
    }
}
