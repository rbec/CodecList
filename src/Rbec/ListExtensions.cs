using System;
using System.Collections.Generic;

namespace Rbec
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

        public static int UpperBound<T>(this IReadOnlyList<T> list, T key, int start, int count) =>
            list.BinarySearch(element => Comparer<T>.Default.Compare(key, element) < 0, start, count);

        public static int UpperBound<T>(this IReadOnlyList<T> list, T key) =>
            list.UpperBound(key, 0, list.Count);

        public static CodecList<T, TOffset, TCodec> ToCodecList<T, TOffset, TCodec>(this IEnumerable<T> elements)
            where TCodec : struct, ICodec<T, TOffset>
        {
            var offsets = new List<TOffset>();
            var keys = new List<int>();
            var values = new List<T>();

            var last = default(T);
            foreach (var element in elements)
            {
                if (!default(TCodec).TryEncode(element, last, out var offset))
                {
                    keys.Add(offsets.Count);
                    values.Add(last = element);
                }

                offsets.Add(offset);
            }

            var keyFrames = new TimeSeries<int, T>(keys, values);

            return new CodecList<T, TOffset, TCodec>(keyFrames, offsets);
        }
    }
}