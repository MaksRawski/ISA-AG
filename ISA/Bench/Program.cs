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
        private readonly UserInputs userInputsEz;
        private readonly UserInputs userInputsExtreme;

        public readonly Population population;
        public readonly List<double> popXs;
        public readonly List<string> popBin;
        public readonly SelectionResults selectionResults;
        public readonly CrossoverResults crossoverResults;

        public Algo() { 
            algo = new Algorithm();
            algo.SetSeed(0);

            License.iConfirmNonCommercialUse("John Doe");
            var f = Utils.ParseFunction("mod(x,1) * (cos(20*pi*x) - sin(x))");


            userInputsEz = new UserInputs
            {
                a = -4,
                b = 12,
                d = 0.001,
                decimalPlaces = 3,
                elitism = true,
                pk = 0.75,
                pm = 0.002,
                N = 10,
                T = 50,
                l = 14,
                functionGoal = FunctionGoal.Max,
                f = f,
            };
            userInputsExtreme = new UserInputs
            {
                a = -4,
                b = 12,
                d = 0.001,
                decimalPlaces = 3,
                elitism = true,
                pk = 0.75,
                pm = 0.002,
                N = 80,
                T = 100,
                l = 14,
                functionGoal = FunctionGoal.Max,
                f = f,
            };

            // init some results to use as inputs for later calls
            population = algo.GeneratePopulation(userInputsEz, out fExtreme);
            selectionResults = algo.Select(userInputsEz, population.xs, fExtreme);
            crossoverResults = algo.Crossover(userInputsEz, selectionResults);
            algo.Mutate(userInputsEz, crossoverResults.populationBin);
            popXs = population.xs;
            popBin = crossoverResults.populationBin;
        }

        [Benchmark]
        public Population GeneratePopulation() => algo.GeneratePopulation(userInputsEz, out double _);

        [Benchmark]
        public SelectionResults Select() => algo.Select(userInputsEz, popXs, fExtreme);

        [Benchmark]
        public CrossoverResults Crossover() => algo.Crossover(userInputsEz, selectionResults);

        [Benchmark]
        public MutationResults Mutation() => algo.Mutate(userInputsEz, popBin);

        [Benchmark]
        public void RunEz() => algo.Run(userInputsEz, out var rows);
        
        [Benchmark]
        public void RunExtreme() => algo.Run(userInputsExtreme, out var rows);
    }

    public class Program
    {
        public static void Main(string[] args)
        {
            var summary = BenchmarkRunner.Run(typeof(Program).Assembly);
        }
    }
}
