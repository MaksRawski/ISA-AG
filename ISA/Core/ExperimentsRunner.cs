using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualBasic;
using org.mariuszgromada.math.mxparser;


namespace Core
{
    public class Experiment
    {
        readonly static int repetitions = 100;

        /// <returns>Average of the best results f(x) from each generation.</returns>
        public static double Run(UserInputs inputs)
        {
            Console.WriteLine($"Running experiment with N={inputs.N}, T={inputs.T}, pk={inputs.pk}, pm={inputs.pm}");

            var average = Enumerable.Range(1, repetitions)
                .Aggregate(Enumerable.Empty<double>(), (bestResults, _i) =>
                {
                    var algo = new Algorithm(inputs);
                    var finalPopulation = algo.Run(out _);
                    return bestResults.Append(finalPopulation.fs.Max());
                })
                .Average();
            Console.WriteLine($"f(x) = {average} for N={inputs.N}, T={inputs.T}, pk={inputs.pk}, pm={inputs.pm}");

            return average;
        }
    }
    public struct ExperimentParameterSet
    {
        public int N, T;
        public double pk, pm;
        public ExperimentParameterSet(int N, double pk, double pm, int T)
        {
            this.N = N;
            this.pm = pm;
            this.pk = pk;
            this.T = T;
        }
    }
    public class ExperimentsRunner
    {
        readonly int totalCombinations;

        private readonly UserInputs defaultInput;
        private readonly List<int> N, T;
        private readonly List<double> pk, pm;
        public ExperimentsRunner(List<int> Ns, List<double> pks, List<double> pms, List<int> Ts)
        {
            GenotypeSpace space = GenotypeSpace.FromDecimalPlaces(3, -4, 12);
            License.iConfirmNonCommercialUse("John Doe");
            var f = Utils.ParseFunction("mod(x,1) * (cos(20*pi*x) - sin(x))");

            defaultInput = new(space, Ns[0], Ts[0], pks[0], pms[0], elitism: true, f, OptimizationGoal.Max);
            totalCombinations = Ns.Count * pks.Count * pms.Count * Ts.Count;

            this.N = Ns;
            this.pk = pks;
            this.pm = pms;
            this.T = Ts;
        }
        public IEnumerable<ExperimentParameterSet> GetAllParameterSets()
        {
            for (int combination = 0; combination < totalCombinations; combination++)
            {
                int nIndex = combination % N.Count;
                int pkIndex = (combination / N.Count) % pk.Count;
                int pmIndex = (combination / (N.Count * pk.Count)) % pm.Count;
                int tIndex = (combination / (N.Count * pk.Count * pm.Count)) % T.Count;

                yield return new ExperimentParameterSet(N[nIndex], pk[pkIndex], pm[pmIndex], T[tIndex]);
            }
        }
        public UserInputs UserInputFromParameterSet(ExperimentParameterSet parameters)
        {
            UserInputs res = defaultInput;
            res.T = parameters.T;
            res.pk = parameters.pk;
            res.pm = parameters.pm;
            res.N = parameters.N;
            return res;
        }
        /// <param name="progress">
        /// <see cref="IProgress"/> object to which to report 
        /// <see cref="ExperimentParameterSet"/> as well as the experiment result (double)
        /// </param>
        public async Task Run(IProgress<(ExperimentParameterSet, double)> progress)
        {
            int maxConcurrentThreads = Environment.ProcessorCount;
            SemaphoreSlim semaphore = new(maxConcurrentThreads);

            var tasks = new List<Task>();

            foreach (var parameterSet in GetAllParameterSets())
            {
                await semaphore.WaitAsync();

                //var task = Task.Run(() =>
                //{
                    var fx = Experiment.Run(UserInputFromParameterSet(parameterSet));
                    Console.WriteLine($"Finished experiment with result: fx={fx}");
                    progress.Report((parameterSet, fx));
                    semaphore.Release();
                //});

                //tasks.Add(task);
            }
            await Task.WhenAll(tasks);
        }
    }
}
