using Core;

namespace UnitTests
{
    [TestClass()]
    public class ConversionsTests
    {
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
            int result = Utils.Bin2Int(x);
            Assert.AreEqual(expected, result, $"Failed with x={x} ({x.Length} bits) in [{a}, {b}]");
        }

        [DataTestMethod]
        [DataRow(0, 1, 0, "0")]
        [DataRow(0, 1, 1, "1")]
        [DataRow(0, 1, 4393139, "10000110000100010110011")]
        [DataRow(0, 1, 314159265, "10010101110011011000010100001")]
        [DataRow(0, 1, 2147483647, "1111111111111111111111111111111")] // 2^31 - 1
        public void Int2Bin(double a, double b, int x, string expected)
        {
            string result = Utils.Int2Bin(x, expected.Length);
            Assert.AreEqual(expected, result, $"Failed with x={x} in [{a}, {b}]");
        }

        [DataTestMethod]
        [DataRow(0, 4, 1, 0)]
        [DataRow(0, 4, 1, 1)]
        [DataRow(0, 4, 1, 6)]
        [DataRow(-4, 0, -4, 0)]
        [DataRow(-4, 0, -1, 0)]
        [DataRow(-4, 0, 0, 0)]
        [DataRow(-4, 0, -3.9, 1)]
        [DataRow(-4, 0, -0.9, 1)]
        [DataRow(-3.5, 2.5, -3.1, 1)]
        [DataRow(-3.5, 2.5, 2.1, 1)]
        public void Real2Int(double a, double b, double x, int decimalPlaces)
        {
            GenotypeSpace space = GenotypeSpace.FromDecimalPlaces(decimalPlaces, a, b);

            int result = Utils.Real2Int(x, space);
            int expected = (int)Math.Round((x - a) / (b - a) * (Math.Pow(2, space.precision.l) - 1));

            Assert.AreEqual(expected, result, $"Failed with x={x} in [{a}, {b}], decimalPlaces={decimalPlaces}");
        }
        [DataTestMethod]
        [DataRow(0, 2, 0, 1)]
        [DataRow(0, 10, 0, 1000)]
        [DataRow(999, 10, 0, 1)]
        [DataRow(999, 10, 0, 10)]
        [DataRow(999, 10, 0, 1000)]
        [DataRow(999, 10, 0, 1023)]
        [DataRow(1000, 10, 0, 1023)]
        [DataRow(1023, 10, 0, 1023)]
        [DataRow(3, 2, 0, 1)]
        [DataRow(7, 4, 0, 1)]
        [DataRow(0, 31, 0, 1)]
        [DataRow(1, 31, 0, 1)]
        [DataRow(31415, 16, 0, 1)]
        [DataRow(2147483647, 31, 0, 1)]
        public void Int2Real(int x, int l, double a, double b)
        {
            GenotypeSpace space = GenotypeSpace.FromL(l, a, b);

            double result = Utils.Int2Real(x, space);
            double expected = (b - a) / (Math.Pow(2, space.precision.l) - 1) * x + a;

            Assert.AreEqual(expected, result, $"Failed with x={x} in [{a}, {b}], l={l}");
        }

        [DataTestMethod]
        [DataRow(-2, 3, 1.5, 3)]
        [DataRow(-2, 3, 3, 3)]
        [DataRow(0, 1, 0, 0)]
        [DataRow(0, 1, 0.1, 1)]
        [DataRow(0, 1, 0.1, 2)]
        [DataRow(0, 1, 0.1, 6)]
        public void Real2Bin2Real(double a, double b, double x, int decimalPlaces)
        {
            GenotypeSpace space = GenotypeSpace.FromDecimalPlaces(decimalPlaces, a, b);

            string bin = Utils.Real2Bin(x, space);
            double real = Utils.Bin2Real(bin, space);

            Assert.AreEqual(
                Math.Round(x, decimalPlaces), 
                Math.Round(real, decimalPlaces), 
                $"Failed with decimalPlaces={decimalPlaces}");
        }
        [DataTestMethod]
        [DataRow(0, 1, "0")]
        [DataRow(0, 1, "1")]
        [DataRow(0, 1, "0000000000000000000000000000000")]
        [DataRow(0, 1, "1111111111111111111111111111111")]
        [DataRow(-5, 8, "11001010010111")]
        [DataRow(-5, 8, "10100111101000111")]
        [DataRow(-7, 23, "10000110000100010110011")]
        [DataRow(-5, 15, "01110000100010110011")]
        [DataRow(-5, 14, "01110000100010110011")]
        [DataRow(-5, 14, "01001010100010100101")]
        [DataRow(-128, 127, "00000000")]
        [DataRow(-128, 127, "00000001")]
        [DataRow(-128, 127, "10101001")]
        [DataRow(-128, 127, "10101001")]
        [DataRow(-128, 127, "11111111")]
        public void Bin2Real2Bin(double a, double b, string x)
        {
            GenotypeSpace space = GenotypeSpace.FromL(x.Length, a, b);

            double real = Utils.Bin2Real(x, space);
            string bin = Utils.Real2Bin(real, space);

            Assert.AreEqual(x, bin);
        }

    }
}
