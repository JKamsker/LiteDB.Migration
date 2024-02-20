using LiteDB;

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace LiteDB.Migration.Helpers;

internal class BsonMapperCloner
{
    internal static T Clone<T>(T obj)
    {
        // get all public and private fields of the object
        var fields = typeof(T).GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
        T newObj = default;
        if (typeof(T) == typeof(BsonMapper))
        {
            newObj = (T)(object)new BsonMapper();
        }
        else
        {
            newObj = Activator.CreateInstance<T>();
        }

        foreach (var field in fields)
        {
            var value = field.GetValue(obj);
            // Custom clone ConcurrentDictionary and Dictionary
            value = DictionaryCloner.CloneIfDictionary(value);

            // copy the value of the field to the new object
            field.SetValue(newObj, value);
        }

        return newObj;
    }
}

internal class DictionaryCloner
{
    public static object CloneIfDictionary(object obj)
    {
        // Check if the object is null to avoid NullReferenceException
        if (obj == null)
        {
            return null;
        }

        Type objType = obj.GetType();

        // Check if the object is a Dictionary<T1, T2>
        if (IsGenericType(objType) && objType.GetGenericTypeDefinition() == typeof(Dictionary<,>))
        {
            // Use reflection to create a new instance of the same type
            var clone = Activator.CreateInstance(objType);
            //MethodInfo addMethod = objType.GetMethod("Add");

            //// Iterate through the dictionary and add each key-value pair to the new dictionary
            //foreach (var item in (dynamic)obj)
            //{
            //    addMethod.Invoke(clone, new object[] { item.Key, item.Value });
            //}

            var copyMethod = CopyDictionaryMethod.MakeGenericMethod(objType.GetGenericArguments());
            copyMethod.Invoke(null, new object[] { obj, clone });

            return clone;
        }
        // Check if the object is a ConcurrentDictionary<T1, T2>
        else if (IsGenericType(objType) && objType.GetGenericTypeDefinition() == typeof(ConcurrentDictionary<,>))
        {
            // Use reflection to create a new instance of the same type
            var clone = Activator.CreateInstance(objType);
            //MethodInfo tryAddMethod = objType.GetMethod("TryAdd");

            //// Iterate through the dictionary and add each key-value pair to the new dictionary
            //foreach (var item in (dynamic)obj)
            //{
            //    tryAddMethod.Invoke(clone, new object[] { item.Key, item.Value });
            //}

            // Without dynamic

            var copyMethod = CopyDictionaryMethod.MakeGenericMethod(objType.GetGenericArguments());
            copyMethod.Invoke(null, new object[] { obj, clone });

            return clone;
        }
        else
        {
            // Return null or throw an exception if obj is not a Dictionary or ConcurrentDictionary
            // Depending on your use case, you might want to return the original object instead
            return obj;
        }
    }

    private static MethodInfo CopyDictionaryMethod = typeof(DictionaryCloner).GetMethod(nameof(CopyDictionary), BindingFlags.NonPublic | BindingFlags.Static);

    private static void CopyDictionary<TKey, TValue>(IDictionary<TKey, TValue> source, IDictionary<TKey, TValue> destination)
    {
        foreach (var item in source)
        {
            destination.Add(item.Key, item.Value);
        }
    }

    private static bool IsGenericType(Type type)
    {
#if NETSTANDARD1_3
        return type.GetTypeInfo().IsGenericType;
#else
        return type.IsGenericType;
#endif
    }
}