using System.Collections.Generic;
using System.Linq;
using Rbec.Codecs;
using Xunit;

namespace Rbec.Tests
{
    public class TestCodecList
    {
        private readonly int[] elements =
            {1000, 1003, 1005, 1002, 995, 998, 1001, 1150, 1145, 800, 1000};

        [Fact]
        public void ToCodecListCountIsCorrect()
        {
            var codecList = elements.ToCodecList<int, sbyte, Int32SByteCodec>();

            Assert.StrictEqual(elements.Length, codecList.Count);
        }

        [Fact]
        public void ToCodecListOffsetsAreCorrect()
        {
            var codecList = elements.ToCodecList<int, sbyte, Int32SByteCodec>();

            Assert.Equal(new sbyte[] {0, 3, 5, 2, -5, -2, 1, 0, -5, 0, 0}, codecList.Offsets);
        }

        [Fact]
        public void ToCodecListKeyFramesKeysAreCorrect()
        {
            var codecList = elements.ToCodecList<int, sbyte, Int32SByteCodec>();

            Assert.Equal(new[] {0, 7, 9, 10}, codecList.KeyFrames.Keys);
        }

        [Fact]
        public void ToCodecListKeyFramesValuesAreCorrect()
        {
            var codecList = elements.ToCodecList<int, sbyte, Int32SByteCodec>();

            Assert.Equal(new[] {1000, 1150, 800, 1000}, codecList.KeyFrames.Values);
        }

        public static IEnumerable<object[]> TestElements =>
            new[]
            {
                new object[] {new int[0]},
                new object[] {new[] {0}},
                new object[] {new[] {1000}},
                new object[] {new[] {int.MaxValue}},
                new object[] {new[] {int.MinValue}},
                new object[] {new[] {int.MaxValue, int.MinValue}},
                new object[] {new[] {int.MinValue, int.MaxValue}},
                new object[] {new[] {1000, 1000, 1000, 1000, 0, 1000}},
                new object[] {new[] {1000, 0, 1000, 0, -1000, 1000}}
            };

        [Theory]
        [MemberData(nameof(TestElements))]
        public void TestEnumeratorIsCorrect(int[] test)
        {
            var codecList = test.ToCodecList<int, sbyte, Int32SByteCodec>();

            Assert.Equal((IEnumerable<int>) test, (IEnumerable<int>) codecList);
        }

        [Theory]
        [MemberData(nameof(TestElements))]
        public void TestCountIsCorrect(int[] test)
        {
            var codecList = test.ToCodecList<int, sbyte, Int32SByteCodec>();

            Assert.Equal(test.Length, codecList.Count);
        }

        [Theory]
        [MemberData(nameof(TestElements))]
        public void TestIndexerIsCorrect(int[] test)
        {
            var codecList = test.ToCodecList<int, sbyte, Int32SByteCodec>();

            Assert.Equal((IEnumerable<int>) test, Enumerable.Range(0, test.Length).Select(index => codecList[index]));
        }
    }
}