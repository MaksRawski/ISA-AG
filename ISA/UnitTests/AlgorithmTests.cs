using Core;

namespace AlgorithmTests
{
    [TestClass]
    public class GeneratePopulationTests
    {
        [DataTestMethod]
        [DataRow(10, -2, 3, 13, 0)]
        [DataRow(10, -2, 3, 13, 1)]
        [DataRow(10, -2, 3, 13, 2)]
        [DataRow(10, -2, 3, 13, 123)]
        [DataRow(1024, 0, 1, 10, 1)]
        [DataRow(10000, 0, 1, 10, 1)]
        public void GeneratePopulationRandomTest(int N, double a, double b, int l, int seed)
        {
            GenotypeSpace space = GenotypeSpace.FromL(l, a, b);
            var inputs = new UserInputs(space, N, T: 1, pk: 0, pm: 0, elitism: false, f: (x) => x, OptimizationGoal.Min);

            var algo = new Algorithm(inputs, seed);
            var population = algo.GeneratePopulation();
            var fMax = algo.fExtremeOppositeOfGoal;

            inputs.optimizationGoal = OptimizationGoal.Max;
            algo = new Algorithm(inputs, seed);
            population = algo.GeneratePopulation();
            var fMin = algo.fExtremeOppositeOfGoal;

            // test fExtreme
            Assert.AreEqual(population.fs.Min(), fMin, "Different minimum found than GeneratePopulation returned.");
            Assert.AreEqual(population.fs.Max(), fMax, "Different maximum found than GeneratePopulation returned.");

            // test population range
            for (int i = 0; i < N; i++)
            {
                Assert.IsTrue(population.xs[i] <= b && population.xs[i] >= a,
                    $"Invalid x=xs[{i}]={population.xs[i]} generated with seed={seed} for range [{a}, {b}] with l={l}");
            }
        }

    }
    [TestClass]
    public class SelectTests
    {
        // elite is selected before Select and put after Mutate so this doesn't apply here
        //[DataTestMethod]
        //[DataRow(1, 0, 1, 3, OptimizationGoal.Min, 0)]
        //[DataRow(1, 0, 1, 3, OptimizationGoal.Max, 0)]
        //[DataRow(2, 0, 5, 3, OptimizationGoal.Max, 0)]
        //[DataRow(2, 0, 5, 3, OptimizationGoal.Min, 0)]
        //[DataRow(9, 0, 1, 3, OptimizationGoal.Max, 0)]
        //[DataRow(10, 0, 1, 3, OptimizationGoal.Max, 0)]
        //[DataRow(10, -3, 1, 3, OptimizationGoal.Min, 0)]
        //[DataRow(10, -3, 1, 3, OptimizationGoal.Max, 0)]
        //[DataRow(10, -3, -1, 3, OptimizationGoal.Max, 0)]
        //[DataRow(10, -3, -1, 3, OptimizationGoal.Min, 0)]
        //public void EliteTest(int N, double a, double b, int decimalPlaces, OptimizationGoal functionGoal, int seed)
        //{
        //    GenotypeSpace space = GenotypeSpace.FromDecimalPlaces(decimalPlaces, a, b);

        //    // try to find elite for simple increasing function
        //    var inputs = new UserInputs(space, N, T: 1, pk: 0, pm: 0, elitism: true, f: (x) => x, functionGoal);
        //    var algo = new Algorithm(inputs, seed);
        //    var population = algo.GeneratePopulation();
        //    var postSelectPopulation = algo.Select(population.xs);

        //    bool foundFExtreme = postSelectPopulation.Any((x) => 
        //        Math.Round(inputs.f(Utils.Bin2Real(x, space)), decimalPlaces) == algo.fExtremeOppositeOfGoal);
        //    Assert.IsTrue(foundFExtreme, $"Failed to find {functionGoal} = {algo.fExtremeOppositeOfGoal} elite. " +
        //        $"N={N}, f: (x) => x, [{a}, {b}]");

        //    // try to find elite for simple decreasing function
        //    inputs = new UserInputs(space, N, T: 1, pk: 0, pm: 0, elitism: true, f: (x) => -x, functionGoal);
        //    algo = new Algorithm(inputs, seed);
        //    population = algo.GeneratePopulation();
        //    postSelectPopulation = algo.Select(population.xs);

        //    foundFExtreme = postSelectPopulation.Any((x) => 
        //        Math.Round(inputs.f(Utils.Bin2Real(x, space)), decimalPlaces) == algo.fExtremeOppositeOfGoal);
        //    Assert.IsTrue(foundFExtreme, $"Failed to find elite with f: (x) => -x");
        //}

        [DataTestMethod]
        [DataRow(1, 0, 1, 3, 0)]
        [DataRow(10, 0, 1, 3, 0)]
        [DataRow(10, -2, 1, 3, 0)]
        [DataRow(10, -2, 4, 3, 0)]
        public void SelectsFromInputTest(int N, double a, double b, int decimalPlaces, int seed)
        {
            GenotypeSpace space = GenotypeSpace.FromDecimalPlaces(decimalPlaces, a, b);
            // NOTE: elititsm doesn't apply here!
            var inputs = new UserInputs(space, N, T: 1, pk: 0, pm: 0, elitism: true, f: (x) => x, OptimizationGoal.Min);
            var algo = new Algorithm(inputs, seed);

            var population = algo.GeneratePopulation();
            var postSelectPopulation = algo.Select(population.xs);

            // check whether all Select results are from the initial population
            // NOTE: this function has O(n^2) because of the nested loop below
            // but since it's just a test i don't care
            foreach (var xPost in postSelectPopulation)
            {
                var xPostReal = Math.Round(Utils.Bin2Real(xPost, space), decimalPlaces);
                bool found = population.xs.Any((x) => x == xPostReal);
                Assert.IsTrue(found, $"Select made up x={xPostReal} that doesn't exist in the initial population!");
            }
        }


        [DataTestMethod]
        [DataRow(2, -2, 2, 0, 0)]
        [DataRow(2, -2, 2, 0, 1)]
        [DataRow(10, -2, 2, 4, 0)]
        [DataRow(10, 0, 2, 4, 0)]
        public void SelectsBasedOnF(int N, double a, double b, int decimalPlaces, int seed)
        {
            GenotypeSpace space = GenotypeSpace.FromDecimalPlaces(decimalPlaces, a, b);

            // Select with f(x) = x
            var inputs = new UserInputs(space, N, T: 1, pk: 0, pm: 0, elitism: false, f: (x) => x, OptimizationGoal.Max);
            var algo = new Algorithm(inputs, seed);

            var pop = algo.GeneratePopulation().xs;
            var popId = algo.Select(pop);

            // Select with f(x) = -x but reverse the optimization goal, essentially giving the same thing as 
            // the invocation above
            inputs.f = (x) => -x;
            inputs.optimizationGoal = OptimizationGoal.Min;

            algo = new Algorithm(inputs, seed);
            pop = algo.GeneratePopulation().xs; // regenerate the population to set fExtremeOppositeOfGoal
            var popNId = algo.Select(pop);

            CollectionAssert.AreEquivalent(popId, popNId,
                "Population after Select with f(x) = -x, goal=Min is different from the one with f(x) = x and goal=Max.");
        }
    }
    [TestClass]
    public class CrossoverTests
    {

        [DataTestMethod]
        [DataRow("0101", "1010", 0)]
        [DataRow("0101", "1010", 1)]
        [DataRow("0101", "1010", 2)]
        [DataRow("0101001", "1101010", 0)]
        [DataRow("0101001", "1101010", 1)]
        [DataRow("0000000000000", "00001001001001", 0)]
        [DataRow("0000000000000", "00001001001001", 1)]
        [DataRow("0000000000000", "00001001001001", 2)]
        public void CrossoverTest(string R1, string R2, int seed)
        {
            GenotypeSpace space = GenotypeSpace.FromL(R1.Length, 0, 1);
            UserInputs inputs = new(space, N: 2, T: 1, pk: 1, pm: 0, elitism: false, f: x => x, OptimizationGoal.Max);

            var algo = new Algorithm(inputs, seed);
            List<string> pop = new() { R1, R2 };
            List<string> C = algo.Crossover(pop);

            // try to find the cutting point
            int cuttingPointC1 = R1
                .ToCharArray()
                .Zip(C[0].ToCharArray(), (r, c) => r == c)
                .TakeWhile(match => match)
                .Count();
            int cuttingPointC2 = R2
                .ToCharArray()
                .Zip(C[1].ToCharArray(), (r, c) => r == c)
                .TakeWhile(match => match)
                .Count();

            // choose the one that's smaller, this ensures that first n bits will be the same for both R1 and C1 and R2 and C2
            int cuttingPoint = Math.Min(cuttingPointC1, cuttingPointC2);
            string C1 = R1[..cuttingPoint] + R2[cuttingPoint..];
            string C2 = R2[..cuttingPoint] + R1[cuttingPoint..];

            Assert.AreEqual(C1, C[0], $"First child must have a different cutting point than second one!");
            Assert.AreEqual(C2, C[1], $"Second child must have a different cutting point than first one!");
        }
    }
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
            UserInputs inputs = new(space, N: 1, T: 1, pk: 0, pm, elitism: false, f: (x) => x, OptimizationGoal.Max);

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
    [TestClass]
    public class RunTests
    {
        [DataTestMethod]
        [DataRow(3, -2, 3, 1, 0)]
        [DataRow(3, -2, 3, 10, 0)]
        [DataRow(3, -2, 3, 10, 1)]
        [DataRow(5, -2, 3, 100, 1)]
        public void EliteTests(int decimalPlaces, double a, double b, int N, int seed)
        {
            GenotypeSpace space = GenotypeSpace.FromDecimalPlaces(decimalPlaces, a, b);
            // mutate all genes to be sure that the elite that was chosen before run is not there after it
            UserInputs inputs = new(space, N, T: 1, pk: 0, pm: 1, elitism: true, f: x => x, OptimizationGoal.Max);

            // get the initial population
            var algo = new Algorithm(inputs, seed);
            var pop = algo.GeneratePopulation();
            double elite = pop.xs.Max();

            // run the entire algorithm for 1 generation
            algo = new(inputs, seed);
            pop = algo.Run();

            bool containsElite = pop.xs.Contains(elite);
            Assert.IsTrue(containsElite, $"Population after Run didn't contain elite!");
        }
    }
}
