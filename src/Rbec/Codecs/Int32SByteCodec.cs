namespace Rbec.Codecs
{
    public struct Int32SByteCodec : ICodec<int, sbyte>
    {
        public sbyte Default => 0;

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
}