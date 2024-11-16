using Core;

namespace UnitTests
{
    [TestClass()]
    public class UnitTests
    {
        [DataTestMethod]
        [DataRow(new double[] { 0.1, 0.3, 0.6, 1.0 }, 0.2, 1)]
        [DataRow(new double[] { 0.1, 0.3, 0.6, 1.0 }, 0.5, 2)]
        [DataRow(new double[] { 0.1, 0.3, 0.6, 1.0 }, 0.8, 3)]
        [DataRow(new double[] { 0.1, 0.3, 0.6, 1.0 }, 0.1, 0)]
        [DataRow(new double[] { 0.1, 0.3, 0.6, 1.0 }, 0.3, 1)]
        [DataRow(new double[] { 0.1, 0.3, 0.6, 1.0 }, 0.6, 2)]
        [DataRow(new double[] { 0.1, 0.3, 0.6, 1.0 }, 1.0, 3)]
        public void GetCDFIndexTest(double[] qsArray, double r, int expectedIndex)
        {
            List<double> qs = new(qsArray);
            int result = Utils.GetCDFIndex(r, qs);

            // Assert
            Assert.AreEqual(
                expectedIndex,
                result,
                $"Test failed for r={r}, qs=[{string.Join(", ", qsArray)}]. Expected index: {expectedIndex}, but got: {result}."
            );
        }
        [DataTestMethod]
        [DataRow(0.3, 1)]
        [DataRow(0.3, 2)]
        [DataRow(0.1, 1)]
        [DataRow(0.1, 2)]
        public void ConversionsTest(double real, int decimalPlaces)
        {
            double a = 0, b = 1;

            double d = Math.Pow(10, -decimalPlaces);
            int l = (int)Math.Ceiling(Math.Log2((b - a) / d + 1));

            string bin = Utils.Real2Bin(real, a, b, l);
            Assert.AreEqual(real, Utils.Bin2Real(bin, a, b, l, decimalPlaces));
        }

        [TestMethod]
        public void MutateTestBasic()
        {
            List<string> xBins = new()
            {
                "0000000000",
                "1111111111"
            };
            UserInputs userInputs = new()
            {
                a = 0,
                b = 1,
                d = 3,
                l = 10,
                N = xBins.Count,
                f = (x) => x,
                pm = 1,
            };
            var algo = new Algorithm();
            var pop = algo.Mutate(userInputs, xBins);
            algo.SetSeed(0);

            List<double> expected = new()
            {
                1, // "1111111111"
                0, // "0000000000"
            };
            CollectionAssert.AreEqual(expected, pop.xs);
        }
        [TestMethod]
        public void MutateTestRandom()
        {
            var algo = new Algorithm();

            // ciąg binarny do zmutowania
            List<string> xBins = new()
            {
                "0000000000",
            };
            UserInputs userInputs = new()
            {
                a = 0,
                b = 1,
                decimalPlaces = 3,
                l = xBins[0].Length,
                N = xBins.Count,
                f = (x) => x,
                pm = 0.1, // prawd. mutacji
            };

            // ten sam seed dla algorytmu i testu
            int seed = 123;
            Random rand = new(seed);
            Random rand1 = new(seed);
            List<double> rands = new(userInputs.l);

            // algorytm na tej samej zasadzie powinien decydować o mutacji poszczególnych genów
            List<bool> shouldMutate = new(userInputs.l);
            for (int i = 0; i < userInputs.l; i++)
            {
                double r = rand.NextDouble();
                rands.Add(r);
                shouldMutate.Add(r <= userInputs.pm);
            }

            algo.SetSeed(seed);
            var mutatedReal = algo.Mutate(userInputs, xBins).xs[0];
            string mutatedXBin = Utils.Real2Bin(mutatedReal, userInputs.a, userInputs.b, userInputs.l);

            for (int i = 0; i < userInputs.l; i++)
            {
                Assert.AreEqual(mutatedXBin[i] != xBins[0][i], shouldMutate[i], $"Wrong mutation on {i}th bit with seed={seed}");
            }
        }
    }
}