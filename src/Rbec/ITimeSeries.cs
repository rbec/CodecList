using System.Collections.Generic;

namespace Rbec
{
    public interface ITimeSeries<TKey, TValue> : IReadOnlyList<KeyValuePair<TKey, TValue>>
    {
        IReadOnlyList<TKey> Keys { get; }
        IReadOnlyList<TValue> Values { get; }
    }
}