# CodecList
A custom data structure in C# implementing `IReadOnlyList<T>` with reduced memory footprint and ***O*** **(log n)** random access.
## Motivation
Sometimes a list of numbers changes only slowly over the indexes of the list, even though the possible range of values remains large. For example a stock price ticker may take any currency value, but two consecutive ticks will typically only differ by a few cents. This presents an opportunity for compression. The aim of this class is to provide a good trade off between space and speed of access for certain use cases.
