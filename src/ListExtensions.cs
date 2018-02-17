using System;
using System.Collections.Generic;

namespace Rbec.CodecList
{
    public static class ListExtensions
    {
        public static int BinarySearch<T>(this IReadOnlyList<T> list, Func<T, bool> predicate, int start, int count)
        {
            while (count > 0)
            {
                var delta = count >> 1;
                if (!predicate(list[start + delta]))
                    start += count - delta;
                count -= count - delta;
            }

            return start;
        }

        public static int LowerBound<T>(this IReadOnlyList<T> list, T key, int start, int count) =>
            list.BinarySearch(element => Comparer<T>.Default.Compare(key, element) <= 0, start, count);

        public static int LowerBound<T>(this IReadOnlyList<T> list, T key) =>
            list.LowerBound(key, 0, list.Count);

        public static int UpperBound<T>(this IReadOnlyList<T> list, T key, int start, int count)
        {
            return list.BinarySearch(element => Comparer<T>.Default.Compare(key, element) < 0, start, count);
        }

        public static int UpperBound<T>(this IReadOnlyList<T> list, T key) =>
            list.UpperBound(key, 0, list.Count);

        public static int LowerBound<TKey, TValue>(this ITimeSeries<TKey, TValue> ts, TKey key) =>
            ts.Keys.LowerBound(key);

        public static int UpperBound<TKey, TValue>(this ITimeSeries<TKey, TValue> ts, TKey key)
        {
            return ts.Keys.UpperBound(key);
        }
    }
}