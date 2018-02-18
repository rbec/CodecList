using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Rbec
{
    public class TimeSeries<TKey, TValue> : ITimeSeries<TKey, TValue>
    {
        public TimeSeries(IReadOnlyList<TKey> keys, IReadOnlyList<TValue> values)
        {
            Keys = keys;
            Values = values;
        }

        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator() =>
            Keys.Zip(Values, (key, value) => new KeyValuePair<TKey, TValue>(key, value))
                .GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() =>
            GetEnumerator();

        public int Count =>
            Math.Min(Keys.Count, Values.Count);

        public KeyValuePair<TKey, TValue> this[int index] =>
            new KeyValuePair<TKey, TValue>(Keys[index], Values[index]);

        public IReadOnlyList<TKey> Keys { get; }
        public IReadOnlyList<TValue> Values { get; }
    }
}