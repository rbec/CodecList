# CodecList
A custom data structure in C# implementing `IReadOnlyList<T>` with reduced memory footprint and ***O*** **(log n)** random access.
## Motivation
Sometimes a list of numbers changes only slowly over the indexes of the list, even though the possible range of values remains large. For example a stock price ticker may take any currency value, but two consecutive ticks will typically only differ by a few cents. This presents an opportunity for compression. The aim of this class is to provide a good trade off between space and speed of access for certain use cases.
## Example
The list of numbers below might be stored as a simple array of 4 byte integers per element.

| Index  | Value |
| ------:|------:|
| *0*      | 1000  |
| *1*      | 1003  |
| *2*      | 1005  |
| *3*      | 1002  |
| *4*      | 995   |
| *5*      | 998   |
| *6*      | 1001  |

Since the numbers only change by a small amount each time we could store just the first element (`1000`) as a 4 byte integer (`int` in C#) and encode each remaining number as an offset from this first element in a single signed byte (`sbyte`).

| Index  | Value | Offset |
| ------:|------:|-------:|
| *0*      | 1000  | 0      |
| *1*      | 1003  | +3      |
| *2*      | 1005  | +5      |
| *3*      | 1002  | +2      |
| *4*      | 995   | -5      |
| *5*      | 998   | -2      |
| *6*      | 1001  | +1      |

A signed byte in [two's complement](https://en.wikipedia.org/wiki/Two%27s_complement) can range from `-128` to `+127` which means we can only represent numbers that far from the first number. To solve this problem we can introduce the concept of a *key frame* (the name borrowed from the idea of a [key frame](https://en.wikipedia.org/wiki/Key_frame) in video compression) to store a new starting point every time the numbers move outside of the representable range.

| Index  | Value | KeyFrame | Offset |
| ------:|------:|---------:|-------:|
| *0*      | 1000  | 1000     | 0      |
| *1*      | 1003  |          | +3     |
| *2*      | 1005  |          | +5     |
| *3*      | 1002  |          | +2     |
| *4*      | 995   |          | -5     |
| *5*      | 998   |          | -2     |
| *6*      | 1001  |          | +1     |
| *7*      | 1150  | 1150     | 0      |
| *8*      | 1145  |          | -5     |

We can therefore encode this list of numbers with two lists - one for the key frames and one for the offsets. Each key frame consists of a 4 byte integer specifying the index in the original array from which it applies and a 4 byte value that needs to be added to the subsequent offsets.

#### Key Frames
| Index | Key | Value |
| -----:| ---:| -----:|
| *0*     | 0   | 1000  |
| *1*     | 7   | 1150  |

#### Offsets
| Index  | Offset |
| ------:|-------:|
| *0*      | 0      |
| *1*      | +3     |
| *2*      | +5     |
| *3*      | +2     |
| *4*      | -5     |
| *5*      | -2     |
| *6*      | +1     |
| *7*      | 0      |
| *8*      | -5     |

## Implementation
Our implementation need not tie us to specific types of key frame or offsets. We can use the C# type system to generalise the idea to any type `T` in `IReadOnlyList<T>` with any type of offset `TOffset`. Let's define some interfaces to abstract this concept out from the implementation itself.
```C#
public interface IDecoder<T, in TOffset>
{
    T Decode(T keyValue, TOffset offset);
}

public interface ICodec<T, TOffset> : IDecoder<T, TOffset>
{
    bool TryEncode(T value, T keyValue, out TOffset offset);
}
```
The example above using `int` and `sbyte` would be implemented like this:

``` C#
public struct Int32SByteCodec : ICodec<int, sbyte>
{
    public bool TryEncode(int value, int keyValue, out sbyte offset)
    {
        var difference = value - keyValue;
        if (difference >= sbyte.MinValue && difference <= sbyte.MaxValue)
        {
            offset = (sbyte)difference;
            return true;
        }
        offset = 0;
        return false;
    }

    public int Decode(int keyValue, sbyte offset) => keyValue + offset;
}
```
Using this we can implement `IReadOnlyList<T>`:
``` C#
public CodecList<T, TOffset, TDecoder> : IReadOnlyList<T>
{
    public ITimeSeries<int, T> KeyFrames;
    public IReadOnlyList<TOffset> Offsets;
}

public interface ITimeSeries<TKey, TValue> : IReadyOnlyList<KeyValuePair<TKey, TValue>>
{
    IReadOnlyList<TKey> Keys { get; } // must be ordered and increasing
    IReadOnlyList<TValue> Values { get; }
}
```
Specifying `TDecoder` as a generic type parameter constrained to be a struct allows a C# performance trick to be used. Since our implementation of `IDecoder` does not reference any internal state or fields, all instances will behave alike, including `default(TDecoder)`. We don't need to pass it in as a parameter and because it's a struct the compiler can inline the method calls.

#### Implementation of `GetEnumerator()`
``` C#
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
```
If there is no key frame at the start (index `0`) we will assume the first offset is from `default(T)`.
#### Implementation of `this[int index] { get; }`

##### Example

| Index  | Value | KeyFrame | Offset |
| ------:|------:|---------:|-------:|
| *0*      | 1000  | 1000     | 0      |
| *1*      | 1003  |          | +3     |
| *2*      | 1005  |          | +5     |
| *3*      | 1002  |          | +2     |
| *4*      | 995   |          | -5     |
| *5*      | 998   |          | -2     |
| *6*      | 1001  |          | +1     |
| *7*      | 1150  | 1150     | 0      |
| > ***8***  | **1145**  |          | **-5**     |
| *9*      | 800   | 800      | 0      |
| *10*     | 1000  | 1000     | 0     |

To find the element at index `8` we need the offset at index `8` (`-5`) which can be looked up directly by index in the `Offsets` list. We also need to find the applicable key frame. In this case it's the 2nd key frame (at index `1`) which applies to offsets starting from index `7`. Since our key frames are ordered by the index from which they apply we can use a binary search. Specifically we want to find the last key frame with a key less than or equal to `8`:

``` C#
var keyFrameIndex = KeyFrames.Keys.UpperBound(index) - 1;
```
`UpperBound` is a variantion of binary search that returns the first index of an element that is greater than the search value. It is [implemented in the C++ standard library](http://www.cplusplus.com/reference/algorithm/upper_bound/) but not in C#. Here is an implementation in C#:
``` C#
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

public static int UpperBound<T>(this IReadOnlyList<T> list, T key, int start, int count)
{
    return list.BinarySearch(element => Comparer<T>.Default.Compare(key, element) < 0, start, count);
}
```
Define a function that returns the key frame value to be added to the offset at a given index:
``` C#
private T KeyValue(int index)
{
    return (index = KeyFrames.Keys.UpperBound(index)) == 0
                ? default(T)
                : KeyFrames.Values[index - 1];
}
```
Finally we can use this to implement the indexer:
``` C#
public T this[int index]
{
    get { return default(TDecoder).Decode(KeyValue(index), Offsets[index]); }
}
```
### Encoding
We need a way to encode a list of elements in our compressed format. Using the `TryEncode` method on the `TCodec` interface:
``` C#
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
```
### Increasing Integer Sequences
If the difference between successive elements is non-negative (i.e. it is increasing) we can save more key frames by using the offset as a `byte` rather than `sbyte` allowing differences between `0` and `255`.
``` C#
public struct Int32ByteCodec : ICodec<int, byte>
{
    public bool TryEncode(int value, int keyValue, out byte offset)
    {
        var difference = value - keyValue;
        if (difference >= byte.MinValue && difference <= byte.MaxValue)
        {
            offset = (byte)difference;
            return true;
        }
        offset = 0;
        return false;
    }

    public int Decode(int keyValue, byte offset) => keyValue + offset;
}
```
This can be used to encode timestamps of trades, which are always non-decreasing.
