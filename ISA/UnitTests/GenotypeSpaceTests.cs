using Core;
using System.Numerics;

namespace UtilsTests
{
    // Precision is always determined by a number of decimal places and since you
    // can't provide a wrong number of decimal places (though it must be positive and not /too/ big)
    // that's why all test procedures will take only it and generate the other two
    // precision parameters d and l from it.
    [TestClass()]
    public class GenotypeSpaceTests
    {
        [DataTestMethod]
        [DataRow(0, 1, 0)]
        [DataRow(0, 1, 3)]
        [DataRow(0, 1, 8)]
        [DataRow(-2, 3, 3)] 
        [DataRow(-4, 12, 4)] 
        [DataRow(-4, 12, 6)] 
        public void FromD(double a, double b, int decimalPlaces)
        {
            double d = Math.Pow(10, -decimalPlaces);
            GenotypeSpace spaceFromD = GenotypeSpace.FromD(d, a, b);

            // This is the most obvious way of calculating the other parameters.
            int numberOfSolutions = (int)(b - a) * (int)Math.Pow(10, decimalPlaces) + 1;
            int storedNumberOfSolutions = (int)BitOperations.RoundUpToPowerOf2((uint)numberOfSolutions);

            int l = (int)Math.Log2(storedNumberOfSolutions);

            Assert.AreEqual(decimalPlaces, spaceFromD.precision.decimalPlaces, 
                $"Wrong decimalPlaces in [{a}, {b}] with d={d}");

            Assert.AreEqual(l, spaceFromD.precision.l, $"Wrong l in [{a}, {b}], with d={d}");
        }

        [DataTestMethod]
        [DataRow(0, 1, 0)]
        [DataRow(0, 1, 3)]
        [DataRow(0, 1, 8)]
        [DataRow(-2, 3, 3)]
        [DataRow(-4, 12, 4)]
        [DataRow(-4, 12, 6)]
        [DataRow(-4, 4, 3)]
        public void FromL(double a, double b, int decimalPlaces)
        {
            // This is the most obvious way of calculating l.
            int numberOfSolutions = (int)(b - a) * (int)Math.Pow(10, decimalPlaces) + 1;
            int storedNumberOfSolutions = (int)BitOperations.RoundUpToPowerOf2((uint)numberOfSolutions);
            int l = (int)Math.Log2(storedNumberOfSolutions);

            GenotypeSpace spaceFromL = GenotypeSpace.FromL(l, a, b);

            double d = Math.Pow(10, -decimalPlaces);

            Assert.AreEqual(d, spaceFromL.precision.d, $"Wrong d in [{a}, {b}] with " +
                $"l={l}.");

            Assert.AreEqual(decimalPlaces, spaceFromL.precision.decimalPlaces,
                $"Wrong decimalPlaces in [{a}, {b}] with l={l}");
        }

        [DataTestMethod]
        [DataRow(0, 1, 0)]
        [DataRow(0, 1, 3)]
        [DataRow(0, 1, 8)]
        [DataRow(-2, 3, 3)]
        [DataRow(-4, 12, 4)]
        [DataRow(-4, 12, 6)]
        public void FromDecimalPlaces(double a, double b, int decimalPlaces)
        {
            GenotypeSpace spaceFromDecimalPlaces = GenotypeSpace.FromDecimalPlaces(decimalPlaces, a, b);

            // This is the most obvious way of calculating l and d.
            int numberOfSolutions = (int)(b - a) * (int)Math.Pow(10, decimalPlaces) + 1;
            int storedNumberOfSolutions = (int)BitOperations.RoundUpToPowerOf2((uint)numberOfSolutions);
            int l = (int)Math.Log2(storedNumberOfSolutions);
            double d = Math.Pow(10, -decimalPlaces);

            Assert.AreEqual(l, spaceFromDecimalPlaces.precision.l,
                $"Wrong l in [{a}, {b}] with {decimalPlaces} decimal places of precision.");

            Assert.AreEqual(d, spaceFromDecimalPlaces.precision.d, $"Wrong d in [{a}, {b}] with " +
                $"{decimalPlaces} decimal places of precision.");
        }
    }
}
