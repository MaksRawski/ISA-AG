using System.Transactions;

namespace Core;

/// <summary>
/// Population stores genotypes of all members as well as their values of goal function.
/// </summary>
public struct Population
{
    public List<double> xs, fs;
}
public readonly struct AlgorithmStats
{
    public IReadOnlyList<double> Fmaxs { get; }
    public IReadOnlyList<double> Faves { get; }
    public IReadOnlyList<double> Fmins { get; }
    public readonly int T;

    internal AlgorithmStats(List<double> fmaxs, List<double> faves, List<double> fmins)
    {
        if (fmaxs.Count != faves.Count || faves.Count != fmins.Count)
            throw new ArgumentException($"Mismatch in the number of stats: fmaxs={fmaxs.Count}, faves={faves.Count}, fmins={fmins.Count}");

        Fmaxs = fmaxs;
        Faves = faves;
        Fmins = fmins;
        this.T = fmaxs.Count;
    }
}
internal class StatsFactory
{
    public List<double> fmaxs = new(), faves = new(), fmins = new();
    private readonly int T;

    public StatsFactory(int T)
    {
        fmaxs.Capacity = T; faves.Capacity = T; fmins.Capacity = T;
        this.T = T;
    }

    public void AddRow(double fmax, double fave, double fmin)
    {
        if (fmaxs.Count == T) 
            throw new Exception("Tried to add more rows to StatsFactory than initially assumed!");
        fmaxs.Add(fmax);
        faves.Add(fave);
        fmins.Add(fmin);
    }
    public AlgorithmStats Build()
    {
        return new AlgorithmStats(fmaxs, faves, fmins);
    }
}

public class Algorithm
{
    private Random _rand = new();
    private readonly UserInputs _inputs;
    public double fExtremeOppositeOfGoal { get; set; }

    public Algorithm(UserInputs userInputs)
    {
        _inputs = userInputs;
        _rand = new Random();
        fExtremeOppositeOfGoal = _inputs.optimizationGoal == OptimizationGoal.Max ? double.MaxValue : double.MinValue;
    }
    public Algorithm(UserInputs userInputs, int seed)
    {
        _inputs = userInputs;
        _rand = new Random(seed);
        fExtremeOppositeOfGoal = _inputs.optimizationGoal == OptimizationGoal.Max ? double.MaxValue : double.MinValue;
    }

    public void SetSeed(int seed)
    {
        _rand = new(seed);
    }

    public Population Run(out AlgorithmStats stats)
    {
        var functionGoal = _inputs.optimizationGoal;
        var population = GeneratePopulation();
        StatsFactory statsFactory = new(_inputs.T);

        // perform GA for T generations
        for (int i = 0; i < _inputs.T; i++)
        {
            // perform elitist selection if it's enabled
            var elite = _inputs.elitism ?
                population.xs
                    .Zip(population.fs, (x, f) => (x, f))
                    .OrderBy(pair => functionGoal == OptimizationGoal.Max ? -pair.f : pair.f)
                    .First()
                : default;

            List<string> xBins = Select(population.xs);
            xBins = Crossover(xBins);
            population = Mutate(xBins);

            if (_inputs.elitism && !population.xs.Contains(elite.x))
            {
                // insert the elite in a random place unless there is a better one at exactly that place
                int r = _rand.Next(0, _inputs.N);
                if (population.fs[r] <= elite.f)
                {
                    population.xs[r] = elite.x;
                    population.fs[r] = elite.f;
                }
            }
            statsFactory.AddRow(population.fs.Max(), population.fs.Average(), population.fs.Min());
        }
        stats = statsFactory.Build();

        return population;
    }
    public void GroupPopulationIntoRows(Population population, out List<TableRow> rows)
    {
        int populationSize = population.xs.Count;
        var groupedRows = population.xs
            .Zip(population.fs, (x, f) => (x, f))
            .GroupBy(item => item.x)
            .Select(group => new TableRow
            {
                XReal = group.Key,
                Fx = group.First().f,
                Percent = (double)group.Count() / populationSize
            });

        rows = (_inputs.optimizationGoal == OptimizationGoal.Max
            ? groupedRows.OrderByDescending(row => row.Fx)
            : groupedRows.OrderBy(row => row.Fx))
            .ToList();


        // format rows
        int lp = 1;
        foreach (TableRow row in rows)
        {
            row.Lp = lp++;
            row.XReal = genotypeSpaceRound(row.XReal);
            row.Fx = genotypeSpaceRound(row.Fx);
            row.XBin = Utils.Real2Bin(row.XReal, _inputs.genotypeSpace);
            row.Percent = genotypeSpaceRound(row.Percent * 100);
        }
    }

    /// <summary>
    /// Generates a <see cref="Population"/> (genotypes and goal function values at given points).
    /// Also sets fExtreme.
    /// </summary>
    public Population GeneratePopulation()
    {
        List<double> xs = new();
        List<double> fs = new();

        for (int i = 0; i < _inputs.N; i++)
        {
            double xReal = _inputs.genotypeSpace.a + (_inputs.genotypeSpace.b - _inputs.genotypeSpace.a) * _rand.NextDouble();
            xReal = genotypeSpaceRound(xReal);
            double fx = _inputs.f(xReal);
            fx = genotypeSpaceRound(fx);

            // max: g(x) = f(x) - fmins + d
            // min: g(x) = -(f(x) - fmaxs) + d
            // if goal is to max, fExtreme has to be min and if the goal is to min then fExtreme should be max
            if (_inputs.optimizationGoal == OptimizationGoal.Max ? fx < fExtremeOppositeOfGoal : fx > fExtremeOppositeOfGoal)
                fExtremeOppositeOfGoal = fx;

            xs.Add(xReal);
            fs.Add(fx);
        }

        return new Population
        {
            xs = xs,
            fs = fs
        };
    }
    /// <summary>
    /// Selects new population memebers from 'xs'.
    /// Requires fExtreme to be set (by running first <see cref="GeneratePopulation">).
    /// </summary>
    public List<string> Select(List<double> xs)
    {
        var gs = xs
            .Select(x => Utils.G(_inputs.f, x, _inputs.optimizationGoal, fExtremeOppositeOfGoal, _inputs.genotypeSpace.precision.d));
        double gsSum = gs.Sum();

        // calculate CDF (cummulative distribution function)
        List<double> qs = gs.Aggregate(Enumerable.Empty<double>(), (qs, g) =>
        {
            double p = g / gsSum;
            double q = qs.LastOrDefault(0) + p;
            return qs.Append(q);
        }).ToList();
       
        // selection
        List<string> xBins = new();
        for (int _i = 0; _i < _inputs.N; _i++)
        {
            double r = _rand.NextDouble();
            int xIndex = Utils.GetCDFIndex(r, qs);
            double xReal = xs[xIndex];
            string xBin = Utils.Real2Bin(xReal, _inputs.genotypeSpace);
            xBins.Add(xBin);
        }

        return xBins;
    }

    public List<string> Crossover(in List<string> xBins)
    {
        // find number of parents
        List<bool> parents = new();
        int numOfParents = 0;

        for (int i = 0; i < _inputs.N; i++)
        {
            bool parent = _rand.NextDouble() <= _inputs.pk;
            parents.Add(parent);
            if (parent) numOfParents++;
        }

        // a single parent can't have children...
        if (numOfParents == 1)
        {
            // it won't even be a parent
            for (int i = 0; i < parents.Count; i++)
            {
                parents[i] = false;
            }
        }

        // find a match and cross chromosomes
        List<string?> children = new();
        string? secondChild = null;

        for (int i = 0; i < _inputs.N; i++)
        {
            string? child = null;

            if (parents[i])
            {
                if (secondChild != null)
                {
                    child = secondChild;
                    secondChild = null;
                }
                else
                {
                    string R1 = xBins[i];

                    // find a match <3
                    int R2Index = (i + 1) % _inputs.N;
                    while (!parents[R2Index]) { R2Index = (R2Index + 1) % _inputs.N; }

                    string R2 = xBins[R2Index];

                    // cross chromosomes
                    int cuttingPoint = _rand.Next(1, _inputs.genotypeSpace.precision.l);
                    child = R1[..(int)cuttingPoint] + R2[(int)cuttingPoint..];
                    secondChild = R2[..(int)cuttingPoint] + R1[(int)cuttingPoint..];

                    if (R2Index == i)
                    {
                        // self breeding is not allowed!
                        child = null;
                    }

                }
            }
            children.Add(child);
        }

        List<string> populationBin = new();
        for (int i = 0; i < _inputs.N; i++)
        {
            // parents die and children end up in their place
            if (parents[i]) populationBin.Add(children[i]!);
            else populationBin.Add(xBins[i]);
        }

        return populationBin;
    }

    public Population Mutate(List<string> xBins)
    {
        List<double> xs = new(_inputs.N);
        List<double> fs = new(_inputs.N);

        for (int i = 0; i < _inputs.N; i++)
        {
            string xBin = xBins[i];
            char[] genes = xBin.ToCharArray();

            for (int g = 0; g < _inputs.genotypeSpace.precision.l; g++)
            {
                double r = _rand.NextDouble();
                if (r <= _inputs.pm)
                {
                    genes[g] = genes[g] == '0' ? '1' : '0';
                }
            }
            xBin = new string(genes);

            double x = Utils.Bin2Real(xBin, _inputs.genotypeSpace);
            double f = _inputs.f(x);

            x = genotypeSpaceRound(x);
            f = genotypeSpaceRound(f);

            xs.Add(x);
            fs.Add(f);
        }

        return new Population { xs = xs, fs = fs };
    }
    /// <summary>
    /// Rounds a real number with precision specified in an instance member '_inputs.genotypeSpace'.
    /// </summary>
    /// <param name="x">Number to round.</param>
    /// <returns>Rounded number.</returns>
    private double genotypeSpaceRound(double x)
    {
        return Math.Round(x, _inputs.genotypeSpace.precision.decimalPlaces);
    }
}
