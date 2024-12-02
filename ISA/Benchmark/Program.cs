using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Exporters.Csv;
using BenchmarkDotNet.Running;
using Core;
using org.mariuszgromada.math.mxparser;
using System.Diagnostics;

namespace Benchmarks
{
    //[RPlotExporter]
    public class Algo
    {
        private readonly Algorithm algo, bestAlgo;

        public readonly Population population;
        public readonly List<string> popXbins;

        public Algo()
        {
            double a = -12, b = -4;
            GenotypeSpace space = GenotypeSpace.FromDecimalPlaces(3, a, b);

            License.iConfirmNonCommercialUse("John Doe");
            var f = Utils.ParseFunction("mod(x,1) * (cos(20*pi*x) - sin(x))");

            var inputs = new UserInputs(space, N: 10, T: 20, pk: 0.75, pm: 0.002, elitism: true, f, OptimizationGoal.Max);
            var bestInputs = new UserInputs(space, N: 75, T: 90, pk: 0.9, pm: 0.01, elitism: true, f, OptimizationGoal.Max);

            int seed = 0;
            algo = new Algorithm(inputs, seed);
            bestAlgo = new Algorithm(inputs, seed);

            // init some results to use as inputs for later calls
            population = algo.GeneratePopulation();
            popXbins = algo.Select(population.xs);
        }

        [Benchmark]
        public Population GeneratePopulation() => algo.GeneratePopulation();

        [Benchmark]
        public List<string> Select() => algo.Select(population.xs);

        [Benchmark]
        public List<string> Crossover() => algo.Crossover(popXbins);

        [Benchmark]
        public Population Mutation() => algo.Mutate(popXbins);

        [Benchmark]
        public Population RunEz() => algo.Run(out _);

        [Benchmark]
        public Population RunBest() => bestAlgo.Run(out _);
    }

    public class Program
    {
        static void Main(string[] _) => BenchmarkRunner.Run<Algo>();
    }
}
