using System;
using System.Collections.Generic;
using System.Text;

namespace LiteDB.Migration.Helpers;

#if NETSTANDARD1_3

internal static class DeconstructEx
{
    // Deconstructs KeyValuePair<T,T1> into two variables
    public static void Deconstruct<TKey, TValue>(this KeyValuePair<TKey, TValue> pair, out TKey key, out TValue value)
    {
        key = pair.Key;
        value = pair.Value;
    }
}

#endif