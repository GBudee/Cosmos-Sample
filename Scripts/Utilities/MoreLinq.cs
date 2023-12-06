using System;
using System.Collections.Generic;
using System.Linq;

namespace Utilities
{
    public static class MoreLinq
    {
        public static IEnumerable<T> CreateEnumerable<T>(params T[] items)
        {
            return items ?? Enumerable.Empty<T>();
        }
        
        public static void ZipDo<T1, T2>( this IEnumerable<T1> first, IEnumerable<T2> second, Action<T1, T2> action)
        {
            using (var e1 = first.GetEnumerator())
            using (var e2 = second.GetEnumerator())
            {
                while (e1.MoveNext() && e2.MoveNext())
                {
                    action(e1.Current, e2.Current);
                }
            }
        }
        
        public static T TrueSingleOrDefault<T>(this IEnumerable<T> source, Func<T, bool> pred)
        {
            bool found = false;
            T result = default(T);
            foreach (T item in source)
            {
                if (!pred(item)) continue;
                if (found) return default;
                result = item;
                found = true;
            }
            //either one item was found or none (result will be default)
            return result;
        }
        
        public static IEnumerable<T> ToEnumerable<T>(this T item)
        {
            yield return item;
        }
    }
}