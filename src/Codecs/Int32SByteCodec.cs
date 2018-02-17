namespace Rbec.CodecList.Codecs
{
    public struct Int32SByteCodec : ICodec<int, sbyte>
    {
        public sbyte Default => 0;

        public bool TryEncode(int value, int keyValue, out sbyte offset)
        {
            var difference = value - keyValue;
            offset = (sbyte) difference;
            return difference >= sbyte.MinValue && difference <= sbyte.MaxValue;
        }

        public int Decode(int keyValue, sbyte offset) => keyValue + offset;
    }
}