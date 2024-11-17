using Core;

namespace UnitTests
{
    [TestClass()]
    public class GenotypeSpaceTests
    {
        [DataTestMethod]
        [DataRow(0, 1, 0, 0, 0)]
        [DataRow(0, 1, 0, 1, 0)]
        [DataRow(0, 1, 0, 10, 1)]
        [DataRow(0, 1, 0, 16, 1)]
        [DataRow(0, 1, 0, 20, 1)]
        public void Real2Int(double a, double b, double x, int decimalPlaces, int expected)
        {
            GenotypeSpace space = GenotypeSpace.FromDecimalPlaces(decimalPlaces, a, b);
            int result = Utils.Real2Int(x, space);
            Assert.AreEqual(expected, result, $"Failed with x={x} in [{a}, {b}], decimalPlaces={decimalPlaces}");
        }

        [DataTestMethod]
        [DataRow(0, 1, "0", 0)]
        [DataRow(0, 1, "1", 1)]
        [DataRow(0, 1, "0000000000000000000000000000000", 0)]
        [DataRow(0, 1, "0000000000000000000000000000001", 1)]
        [DataRow(0, 1, "0000000010000110000100010110011", 4393139)]
        [DataRow(0, 1, "0010010101110011011000010100001", 314159265)]
        [DataRow(0, 1, "1111111111111111111111111111111", 2147483647)] // 2^31 - 1
        public void Bin2Int(double a, double b, string x, int expected)
        {
            int l = x.Length;
            GenotypeSpace space = GenotypeSpace.FromDecimalPlaces(424242424, a, b); // random number as there is no rounding
            int result = Utils.Bin2Int(x);
            Assert.AreEqual(expected, result, $"Failed with x={x} ({l} bits) in [{a}, {b}]");
        }

        [DataTestMethod]
        [DataRow(0, 1, 0.1, 1)]
        [DataRow(0, 1, 0.1, 2)]
        [DataRow(0, 1, 0.1, 6)]
        [DataRow(0, 1, 0.1, 8)]
        [DataRow(0, 1, 0.1, 10)]
        [DataRow(0, 1, 0.1, 12)]
        [DataRow(0, 1, 0.1, 14)]
        [DataRow(0, 1, 0.1, 16)]
        [DataRow(1.1, 1)]
        [DataRow(1.1, 2)]
        [DataRow(1.1, 6)]
        [DataRow(1.1, 8)]
        [DataRow(1.1, 10)]
        [DataRow(1.1, 12)]
        [DataRow(1.1, 14)]
        [DataRow(1.1, 16)]
        public void Real2Bin2Real(double a, double b, double real, int decimalPlaces)
        {
            GenotypeSpace space = GenotypeSpace.FromDecimalPlaces(decimalPlaces, a, b);

            string bin = Utils.Real2Bin(real, space);
            Assert.AreEqual(real, Utils.Bin2Real(bin, space), $"Failed with decimalPlaces={decimalPlaces}");
        }

    }
}
