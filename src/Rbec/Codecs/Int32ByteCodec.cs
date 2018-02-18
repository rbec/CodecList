namespace Rbec.Codecs
{
    public struct Int32ByteCodec : ICodec<int, byte>
    {
        public byte Default => 0;

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
}