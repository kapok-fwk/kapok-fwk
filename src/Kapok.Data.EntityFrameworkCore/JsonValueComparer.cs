using System;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Newtonsoft.Json;

namespace Kapok.Data.EntityFrameworkCore
{
    internal class JsonValueComparer<T> : ValueComparer<T>
    {
        public JsonValueComparer()
            : base(
            (left, right) => IsJsonEquals(left, right), 
            t => GetJsonHashCode(t), 
            t => GetJsonSnapshot(t))
        {
        }

        private static T GetJsonSnapshot(T instance)
        {
            if (instance is ICloneable cloneable)
                return (T)cloneable.Clone();

#pragma warning disable 8605
#pragma warning disable 8603
            return (T)JsonConvert.DeserializeObject(JsonConvert.SerializeObject(instance), typeof(T));
#pragma warning restore 8603
#pragma warning restore 8605
        }

        private static int GetJsonHashCode(T instance)
        {
            if (instance is IEquatable<T>)
                return instance.GetHashCode();

            return JsonConvert.SerializeObject(instance).GetHashCode();
        }

        private static bool IsJsonEquals(T left, T right)
        {
            if (left is IEquatable<T> equatable)
                return equatable.Equals(right);

            return JsonConvert.SerializeObject(left).Equals(JsonConvert.SerializeObject(right));
        }
    }
}
