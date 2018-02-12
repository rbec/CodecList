using System.Collections;
using System.Collections.Generic;

namespace Rbec.CodecList
{
    public sealed class CodecList<T, TCode, TDecoder> : IReadOnlyList<T>
        where TDecoder : struct, IDecoder<T, TCode>
    {
        public readonly ITimeSeries<int, T> KeyFrames;
        public readonly IReadOnlyList<TCode> Deltas;

        public CodecList(ITimeSeries<int, T> keyFrames, IReadOnlyList<TCode> deltas)
        {
            KeyFrames = keyFrames;
            Deltas = deltas;
        }

        public IEnumerator<T> GetEnumerator()
        {
            var lastValue = default(T);

            var i = 0;
            var j = 0;

            while (i < KeyFrames.Count && j < Deltas.Count)
            {
                if (KeyFrames.Keys[i] == j)
                    lastValue = KeyFrames.Values[i++];
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
            (index = KeyFrames.UpperBound(index)) == 0
                ? default(T)
                : KeyFrames.Values[index - 1];
    }
}