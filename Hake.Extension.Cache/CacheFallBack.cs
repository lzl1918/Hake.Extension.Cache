using System;
using System.Collections.Generic;

namespace Hake.Extension.Cache
{
    public delegate RetrivationResult<TValue> CacheFallBack<TKey, TValue>(TKey key);
}
