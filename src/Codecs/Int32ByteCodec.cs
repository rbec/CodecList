namespace Rbec.CodecList.Codecs
{
    public struct Int32ByteCodec : ICodec<int, byte>
    {
        public byte Default => 0;

        public bool TryEncode(int value, int keyValue, out byte offset)
        {
            var difference = value - keyValue;
            offset = (byte) difference;
            return difference >= byte.MinValue && difference <= byte.MaxValue;
        }

        public int Decode(int keyValue, byte offset) => keyValue + offset;
    }
}