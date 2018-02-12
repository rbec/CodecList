using System.Collections;
using System.Collections.Generic;

namespace Rbec.CodecList
{
    public sealed class CodecList<T, TCode, TDecoder> : IReadOnlyList<T>
        where TDecoder : struct, IDecoder<T, TCode>
    {
        public readonly ITimeSeries<int, T> Keys;
        public readonly IReadOnlyList<TCode> Deltas;

        public CodecList(ITimeSeries<int, T> keys, IReadOnlyList<TCode> deltas)
        {
            Keys = keys;
            Deltas = deltas;
        }

        public IEnumerator<T> GetEnumerator()
        {
            var lastValue = default(T);

            var i = 0;
            var j = 0;

            while (i < Keys.Count && j < Deltas.Count)
            {
                if (Keys.Keys[i] == j)
                    lastValue = Keys.Values[i++];
                yield return default(TDecoder).Decode(lastValue, Deltas[j++]);
            }

            while (j < Deltas.Count)
                yield return default(TDecoder).Decode(lastValue, Deltas[j++]);
        }

        IEnumerator IEnumerable.GetEnumerator() =>
            GetEnumerator();

        public int Count =>
            Deltas.Count;

        public T this[int index] =>
            default(TDecoder).Decode(KeyValue(index), Deltas[index]);

        private T KeyValue(int index) =>
            (index = Keys.UpperBound(index)) == 0
                ? default(T)
                : Keys.Values[index - 1];
    }
}