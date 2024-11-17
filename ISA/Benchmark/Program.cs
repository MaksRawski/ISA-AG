using BenchmarkDotNet.Attributes;
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
        private readonly Algorithm algo;
        private readonly double fExtreme;

        public readonly Population population;
        public readonly List<double> popXs;
        public readonly List<string> popXbins;

        public Algo()
        {
            double a = -4, b = -12;
            GenotypeSpace space = GenotypeSpace.FromDecimalPlaces(3, a, b);

            License.iConfirmNonCommercialUse("John Doe");
            var f = Utils.ParseFunction("mod(x,1) * (cos(20*pi*x) - sin(x))");

            var inputs = new UserInputs
            {
                genotypeSpace = space,
                elitism = true,
                pk = 0.75,
                pm = 0.002,
                N = 10,
                T = 50,
                functionGoal = FunctionGoal.Max,
                f = f,
            };

            algo = new Algorithm(inputs);
            
            // init some results to use as inputs for later calls
            population = algo.GeneratePopulation(out fExtreme);
            popXbins = algo.Select(population.xs, fExtreme);
            popXbins = algo.Crossover(popXbins);
            popXs = algo.Mutate(popXbins).xs;
        }

        [Benchmark]
        public Population GeneratePopulation() => algo.GeneratePopulation(out double _);

        [Benchmark]
        public List<string> Select() => algo.Select(popXs, fExtreme);

        [Benchmark]
        public List<string> Crossover() => algo.Crossover(popXbins);

        [Benchmark]
        public Population Mutation() => algo.Mutate(popXbins);

        //[Benchmark]
        //public void RunEz() => algo.Run(userInputsEz, out var rows);

        //[Benchmark]
        //public void RunExtreme() => algo.Run(userInputsExtreme, out var rows);
    }

    public class Program
    {
        public static void Main(string[] args)
        {
            var summary = BenchmarkRunner.Run(typeof(Program).Assembly);
        }
    }
}
