namespace Rbec
{
    public interface ICodec<T, TOffset> : IDecoder<T, TOffset>
    {
        TOffset Default { get; }
        bool TryEncode(T value, T keyValue, out TOffset offset);
    }
}