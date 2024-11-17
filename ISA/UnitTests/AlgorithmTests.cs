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
            GenotypeSpace space = GenotypeSpace.FromDecimalPlaces(3, a, b);

            List<string> xBins = new()
            {
                "0000000000",
                "1111111111"
            };

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

        // TODO: for this it would be nice if we had genotypeSpace.FromL
        //[TestMethod]
        //public void MutateTestRandom()
        //{
        //    double a = 0, b = 1;
        //    genotypeSpace space = genotypeSpace.FromDecimalPlaces(3, a, b);

        //    List<string> xBins = new()
        //    {
        //        "0000000000",
        //    };

        //    UserInputs inputs = new()
        //    {
        //        genotypeSpace = space,
        //        N = xBins.Count,
        //        f = (x) => x,
        //        pm = 1,
        //    };

        //    // ten sam seed dla algorytmu i testu
        //    int seed = 123;
        //    Random rand = new(seed);

        //    // algorytm na tej samej zasadzie powinien decydować o mutacji poszczególnych genów
        //    List<bool> shouldMutate = new();
        //    for (int i = 0; i < space.l; i++)
        //    {
        //        double r = rand.NextDouble();
        //        rands.Add(r);
        //        shouldMutate.Add(r <= inputs.pm);
        //    }

        //    algo.SetSeed(seed);
        //    var mutatedReal = algo.Mutate(inputs, xBins).xs[0];
        //    string mutatedXBin = Utils.Real2Bin(mutatedReal, inputs.a, inputs.b, inputs.l);

        //    for (int i = 0; i < inputs.l; i++)
        //    {
        //        Assert.AreEqual(mutatedXBin[i] != xBins[0][i], shouldMutate[i], $"Wrong mutation on {i}th bit with seed={seed}");
        //    }
        //}
    }
}
