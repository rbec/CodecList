using System;
using System.Collections;
using System.Collections.Generic;

namespace Core
{
  public sealed class OffsetList<T, TOffset, TCodec> : IReadOnlyList<T>
    where TCodec : struct, ICodec<T, TOffset>
  {
    public readonly List<KeyValuePair<int, T>> KeyValues;
    public readonly List<TOffset> Offsets;

    public OffsetList(List<KeyValuePair<int, T>> keyValues, List<TOffset> offsets)
    {
      KeyValues = keyValues;
      Offsets = offsets;
    }

    public IEnumerator<T> GetEnumerator()
    {
      var keyIndex = KeyValues.Count == 0 || KeyValues[0].Key == 0 ? 0 : 1;
      var keyValue = keyIndex == 0 ? default(T) : KeyValues[keyIndex - 1].Value;
      var next = keyIndex == KeyValues.Count ? Count : KeyValues[keyIndex].Key;
      var end = Count;

      var i = 0;
      while (end > next)
      {
        while (i < next)
          yield return default(TCodec).Decode(keyValue, Offsets[i++]);
        keyValue = KeyValues[keyIndex++].Value;
        if (keyIndex == KeyValues.Count)
          break;
        next = KeyValues[keyIndex].Key;
      }
      while (i < end)
        yield return default(TCodec).Decode(keyValue, Offsets[i++]);
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    public int Count => Offsets.Count;
    public T this[int index] => default(TCodec).Decode(KeyValue(index), Offsets[index]);

    private T KeyValue(int index)
    {
      var keyIndex = KeyValues.BinarySearch(keyValue => index < keyValue.Key);
      return keyIndex == 0
        ? default(T)
        : KeyValues[keyIndex - 1].Value;
    }

    public void Add(T item) { }
  }

  public interface ICodec<T, TOffset>
  {
    bool TryEncode(T value, T keyValue, out TOffset offset);
    T Decode(T keyValue, TOffset offset);
  }

  public static class Lists
  {
    public static int BinarySearch<T>(this IReadOnlyList<T> list, Func<T, bool> predicate, int start, int count)
    {
      while (count > 0)
      {
        var δ = count >> 1;
        if (!predicate(list[start + δ]))
          start += count - δ;
        count -= count - δ;
      }
      return start;
    }

    public static int BinarySearch<T>(this IReadOnlyList<T> list, Func<T, bool> predicate) => list.BinarySearch(predicate, 0, list.Count);
  }
}