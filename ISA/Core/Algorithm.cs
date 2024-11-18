namespace Core;

/// <summary>
/// Population stores genotypes of all members as well as their values of goal function.
/// </summary>
public struct Population
{
    public List<double> xs, fs;
}
public class Algorithm
{
    private Random _rand = new();
    private UserInputs _inputs;

    public Algorithm(UserInputs userInputs)
    {
        _inputs = userInputs;
        _rand = new Random();
    }
    public Algorithm(UserInputs userInputs, int seed)
    {
        _inputs = userInputs;
        _rand = new Random(seed);
    }

    public void SetSeed(int seed)
    {
        _rand = new(seed);
    }

    public void Run(out List<TableRow> rows)
    {
        var functionGoal = _inputs.functionGoal;
        Population population = GeneratePopulation(out double fExtreme);

        // perform GA for T generations
        for (int i = 0; i < _inputs.T; i++)
        {
            // perform elitist selection if it's enabled
            var elite = _inputs.elitism ?
                population.xs
                    .Zip(population.fs, (x, f) => (x, f))
                    .OrderBy(pair => functionGoal == FunctionGoal.Max ? -pair.f : pair.f)
                    .First()
                : default;

            List<string> xBins = Select(population.xs, fExtreme);
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
        }

        // group final population members
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

        rows = (_inputs.functionGoal == FunctionGoal.Max
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
    public Population GeneratePopulation(out double fExtreme)
    {
        fExtreme = _inputs.functionGoal == FunctionGoal.Max ? double.MaxValue : double.MinValue;
        List<double> xs = new();
        List<double> fs = new();

        for (int i = 0; i < _inputs.N; i++)
        {
            double xReal = _inputs.genotypeSpace.a + (_inputs.genotypeSpace.b - _inputs.genotypeSpace.a) * _rand.NextDouble();
            xReal = genotypeSpaceRound(xReal);
            double fx = _inputs.f(xReal);
            fx = genotypeSpaceRound(fx);

            // max: g(x) = f(x) - fmin + d
            // min: g(x) = -(f(x) - fmax) + d
            if (_inputs.functionGoal == FunctionGoal.Max ? fx < fExtreme : fx > fExtreme)
                fExtreme = fx;

            xs.Add(xReal);
            fs.Add(fx);
        }

        return new Population
        {
            xs = xs,
            fs = fs
        };
    }

    public List<string> Select(List<double> xs, double fExtreme)
    {
        List<double> gs = new();
        double gsSum = 0;

        for (int i = 0; i < _inputs.N; i++)
        {
            double gx = Utils.G(_inputs.f, xs[i], _inputs.functionGoal, fExtreme, _inputs.genotypeSpace.precision.d);
            gsSum += gx;
            gs.Add(gx);
        }

        double q = 0;
        List<double> ps = new();
        List<double> qs = new();

        for (int i = 0; i < _inputs.N; i++)
        {
            double p = gs[i] / gsSum;
            q += p;
            ps.Add(p);
            qs.Add(q);
        }

        List<double> rs = new();
        List<double> xReals = new();
        List<string> xBins = new();

        for (int i = 0; i < _inputs.N; i++)
        {
            double r = _rand.NextDouble();
            int xCrossIndex = Utils.GetCDFIndex(r, qs);
            double xPreCrossReal = xs[xCrossIndex];
            string xPreCrossBin = Utils.Real2Bin(xPreCrossReal, _inputs.genotypeSpace);
            rs.Add(r);
            xReals.Add(xPreCrossReal);
            xBins.Add(xPreCrossBin);
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
        List<List<int>?> cuttingPoints = new(_inputs.N);
        List<string?> children = new();
        string? secondChild = null;
        int? cuttingPoint = null;

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
                    cuttingPoint = _rand.Next(1, _inputs.genotypeSpace.precision.l);
                    child = R1[..(int)cuttingPoint] + R2[(int)cuttingPoint..];
                    secondChild = R2[..(int)cuttingPoint] + R1[(int)cuttingPoint..];

                    // if current parent is a paramour of R2
                    if (R2Index < i)
                    {
                        // add the current cutting point to it
                        cuttingPoints[R2Index]!.Add((int)cuttingPoint);
                    }
                    else if (R2Index == i)
                    {
                        // self breeding is not allowed!
                        child = null;
                    }

                }
                cuttingPoints.Add(new List<int> { (int)cuttingPoint! });
            }
            else
            {
                cuttingPoints.Add(null);
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
        List<double> rands = new(_inputs.genotypeSpace.precision.l);

        for (int i = 0; i < _inputs.N; i++)
        {
            string xBin = xBins[i];
            char[] genes = xBin.ToCharArray();

            for (int g = 0; g < _inputs.genotypeSpace.precision.l; g++)
            {
                double r = _rand.NextDouble();
                rands.Add(r);
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
