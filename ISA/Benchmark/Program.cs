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
        public readonly List<string> popXbins;

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
            popXbins = algo.Select(userInputsEz, population.xs, fExtreme);
            popXbins = algo.Crossover(userInputsEz, popXbins);
            popXs = algo.Mutate(userInputsEz, popXbins).xs;
        }

        [Benchmark]
        public Population GeneratePopulation() => algo.GeneratePopulation(userInputsEz, out double _);

        [Benchmark]
        public List<string> Select() => algo.Select(userInputsEz, popXs, fExtreme);

        [Benchmark]
        public List<string> Crossover() => algo.Crossover(userInputsEz, popXbins);

        [Benchmark]
        public Population Mutation() => algo.Mutate(userInputsEz, popXbins);

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
