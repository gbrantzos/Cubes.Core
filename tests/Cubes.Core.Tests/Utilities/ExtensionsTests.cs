using Cubes.Core.Utilities;
using Xunit;

namespace Cubes.Core.Tests.Utilities
{
    public class ExtensionsTests
    {
        [Fact]
        public void Safecast_HappyPath()
        {
            var t1 = 3.14d;
            var resultDecimal = t1.SafeCast<decimal>();
            Assert.Equal(3.14m, resultDecimal);

            var t2 = 3.00m;
            var resultInt = t2.SafeCast<int>();
            Assert.Equal(3, resultInt);

            var t3 = 5;
            var resultInt2 = t3.SafeCast<int>();
            Assert.Equal(5, resultInt2);
        }

        [Fact]
        public void SafeCast_Handles_Null()
        {
            object tmp = null;
            var resultInt = tmp.SafeCast<int>(-1);
            Assert.Equal(-1, resultInt);

            var resultLong = tmp.SafeCast<long>(-1);
            Assert.Equal(-1, resultLong);

            var resultDouble = tmp.SafeCast<double>();
            Assert.Equal(0d, resultDouble);

            var resultDecimal = tmp.SafeCast<decimal>();
            Assert.Equal(0m, resultDecimal);
        }

        [Fact]
        public void SafeCast_OnExceptions_ReturnsDefault()
        {
            var tmp = "Test";
            var resultInt = tmp.SafeCast<int>(-1);
            Assert.Equal(-1, resultInt);
        }
    }
}
