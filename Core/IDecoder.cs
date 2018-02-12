namespace Rbec.CodecList
{
    public interface IDecoder<T, in TCode>
    {
        T Decode(T keyValue, TCode code);
    }
}