using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Exporters.Csv;
using BenchmarkDotNet.Running;
using Core;
using org.mariuszgromada.math.mxparser;

namespace Benchmarks
{
    [RPlotExporter]
    public class Algo
    {
        private readonly Algorithm algo;

        private readonly double fExtreme;
        private readonly UserInputs userInputs;

        private readonly Population population;
        private readonly SelectionResults selectionResults;
        private readonly CrossoverResults crossoverResults;

        public Algo() { 
            algo = new Algorithm();
            algo.SetSeed(0);

            License.iConfirmNonCommercialUse("John Doe");
            var f = Utils.ParseFunction("mod(x,1) * (cos(20*pi*x) - sin(x))");


            userInputs = new UserInputs
            {
                f = f,
                a = -4,
                b = 12,
                d = 0.001,
                l = 14,
                decimalPlaces = 3,
                functionGoal = FunctionGoal.Max,
                N = 10,
                pk = 0.75,
                pm = 0.002,
            };

            // init some results to use as inputs for later calls
            population = algo.GeneratePopulation(userInputs, out fExtreme);
            selectionResults = algo.Select(userInputs, population.xs, fExtreme);
            crossoverResults = algo.Crossover(userInputs, selectionResults);
            algo.Mutate(userInputs, crossoverResults.populationBin);
        }

        [Benchmark]
        public Population GeneratePopulation() => algo.GeneratePopulation(userInputs, out double _);

        [Benchmark]
        public SelectionResults Select() => algo.Select(userInputs, population.xs, fExtreme);
        
        [Benchmark]
        public CrossoverResults Crossover() => algo.Crossover(userInputs, selectionResults);

        [Benchmark]
        public MutationResults Mutation() => algo.Mutate(userInputs, crossoverResults.populationBin);

    }

    public class Program
    {
        public static void Main(string[] args)
        {
            var summary = BenchmarkRunner.Run(typeof(Program).Assembly);
        }
    }
}
