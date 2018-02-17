using System.Collections;
using System.Collections.Generic;

namespace Rbec
{
    public sealed class CodecList<T, TOffset, TDecoder> : IReadOnlyList<T>
        where TDecoder : struct, IDecoder<T, TOffset>
    {
        public readonly ITimeSeries<int, T> KeyFrames;
        public readonly IReadOnlyList<TOffset> Offsets;

        public CodecList(ITimeSeries<int, T> keyFrames, IReadOnlyList<TOffset> offsets)
        {
            KeyFrames = keyFrames;
            Offsets = offsets;
        }

        public IEnumerator<T> GetEnumerator()
        {
            var lastValue = default(T);

            var i = 0;
            var j = 0;

            while (i < KeyFrames.Count && j < Offsets.Count)
            {
                if (KeyFrames.Keys[i] == j)
                    lastValue = KeyFrames.Values[i++];
                yield return default(TDecoder).Decode(lastValue, Offsets[j++]);
            }

            while (j < Offsets.Count)
                yield return default(TDecoder).Decode(lastValue, Offsets[j++]);
        }

        IEnumerator IEnumerable.GetEnumerator() =>
            GetEnumerator();

        public int Count =>
            Offsets.Count;

        public T this[int index]
        {
            get { return default(TDecoder).Decode(KeyValue(index), Offsets[index]); }
        }

        private T KeyValue(int index)
        {
            return (index = KeyFrames.Keys.UpperBound(index)) == 0
                       ? default(T)
                       : KeyFrames.Values[index - 1];
        }
    }
}