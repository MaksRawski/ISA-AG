using Core;

namespace AlgorithmTests
{
    [TestClass]
    public class MutateTests
    {
        [TestMethod]
        public void MutateTestBasic()
        {
            double a = 0, b = 1;
            List<string> xBins = new()
            {
                "0000000000",
                "1111111111"
            };
            GenotypeSpace space = GenotypeSpace.FromL(xBins[0].Length, a, b);


            UserInputs userInputs = new()
            {
                genotypeSpace = space,
                N = xBins.Count,
                f = (x) => x,
                pm = 1,
            };

            var seed = 0;
            var algo = new Algorithm(userInputs, seed);
            var pop = algo.Mutate(xBins);

            List<double> expected = new()
            {
                1, // "1111111111"
                0, // "0000000000"
            };

            CollectionAssert.AreEqual(expected, pop.xs);
        }

        [DataTestMethod]
        [DataRow("0000000000", 0.5, 0)]
        [DataRow("1111111111", 0.5, 0)]
        [DataRow("0101010101", 0.1, 0)]
        [DataRow("0000000000", 0.5, 123)]
        [DataRow("0000000000", 0.1, 123)]
        public void MutateTestRandom(string x, double pm, int seed)
        {
            double a = 0, b = 1;

            GenotypeSpace space = GenotypeSpace.FromL(x.Length, a, b);
            UserInputs inputs = new(space, N: 1, T: 1, pk: 0, pm, elitism: false, f: (x) => x, FunctionGoal.Max);

            // use the same seed for both the test's RNG and the algorithm
            Random rand = new(seed);
            var algo = new Algorithm(inputs, seed);

            // perform the test's mutation
            List<bool> shouldMutate = new();
            for (int i = 0; i < space.precision.l; i++)
            {
                double r = rand.NextDouble();
                shouldMutate.Add(r <= inputs.pm);
            }

            // mutate with the algorithm
            var mutatedReal = algo.Mutate(new List<string> { x }).xs[0];
            string mutatedXBin = Utils.Real2Bin(mutatedReal, space);

            // compare bits
            for (int i = 0; i < x.Length; i++)
            {
                Assert.AreEqual(mutatedXBin[i] != x[i], shouldMutate[i], $"Wrong mutation on {i}th bit with seed={seed}");
            }
        }
    }
}
