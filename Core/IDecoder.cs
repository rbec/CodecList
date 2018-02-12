namespace Rbec.CodecList
{
    public interface IDecoder<T, in TOffset>
    {
        T Decode(T keyValue, TOffset offset);
    }
}