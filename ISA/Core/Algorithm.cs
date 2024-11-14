namespace Core;
public struct Population
{
    public List<double> xs, fs;
}

public struct SelectionResults
{
    public List<double> gs, ps, qs, rs, xReals;
    public List<string> xBins;
}

public struct CrossoverResults
{
    public List<bool> parents;
    public List<string?> children;
    public List<List<int>?> cuttingPoints;
    public List<string> populationBin;
    public int numOfParents;
}

public struct MutationResults
{
    public List<int?> mutationPoints;
    public List<string> xBins;
    public Population population;
}

public class Algorithm
{
    Random rand = new();
    public void SetSeed(int seed)
    {
        rand = new(seed);
    }
    
    public void Run(in UserInputs userInputs, out List<TableRow> rows)
    {
        var functionGoal = userInputs.functionGoal;
        Population population = GeneratePopulation(userInputs, out double fExtreme);

        // perform GA for T generations
        for (int i = 0; i < userInputs.T; i++)
        {
            // perform elitist selection if it's enabled
            var elite = userInputs.elitism ?
                population.xs
                    .Zip(population.fs, (x, f) => (x, f))
                    .OrderBy(pair => functionGoal == FunctionGoal.Max ? -pair.f : pair.f)
                    .First()
                : default;

            SelectionResults selection = Select(userInputs, population.xs, fExtreme);

            CrossoverResults crossover = Crossover(userInputs, selection);

            MutationResults mutation = Mutate(userInputs, crossover.populationBin);

            population = mutation.population;

            if (userInputs.elitism && !population.xs.Contains(elite.x))
            {
                // insert the elite in a random place unless there is a better one at exactly that place
                int r = rand.Next(0, userInputs.N);
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

        rows = (userInputs.functionGoal == FunctionGoal.Max
            ? groupedRows.OrderByDescending(row => row.Fx)
            : groupedRows.OrderBy(row => row.Fx))
            .ToList();


        // format rows
        int lp = 1;
        foreach (TableRow row in rows)
        {
            row.Lp = lp++;
            row.XReal = Math.Round(row.XReal, userInputs.decimalPlaces);
            row.Fx = Math.Round(row.Fx, userInputs.decimalPlaces);
            row.XBin = Utils.Real2Bin(row.XReal, userInputs.a, userInputs.b, userInputs.l);
            row.Percent = Math.Round(row.Percent * 100, userInputs.decimalPlaces);
        }
    }
    public Population GeneratePopulation(in UserInputs userInputs, out double fExtreme)
    {
        fExtreme = userInputs.functionGoal == FunctionGoal.Max ? double.MaxValue : double.MinValue;
        List<double> xs = new();
        List<double> fs = new();

        for (int i = 0; i < userInputs.N; i++)
        {
            double xReal = userInputs.a + (userInputs.b - userInputs.a) * rand.NextDouble();
            xReal = Math.Round(xReal, userInputs.decimalPlaces);
            double fx = userInputs.f(xReal);
            fx = Math.Round(fx, userInputs.decimalPlaces);
            // max: g(x) = f(x) - fmin + d
            // min: g(x) = -(f(x) - fmax) + d
            if (userInputs.functionGoal == FunctionGoal.Max ? fx < fExtreme : fx > fExtreme)
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
    
    public SelectionResults Select(in UserInputs userInputs, List<double> xs, double fExtreme)
    {
        List<double> gs = new();
        double gsSum = 0;

        for (int i = 0; i < userInputs.N; i++)
        {
            double gx = Utils.G(userInputs.f, xs[i], userInputs.functionGoal, fExtreme, userInputs.d);
            gsSum += gx;
            gs.Add(gx);
        }

        double q = 0;
        List<double> ps = new();
        List<double> qs = new();

        for (int i = 0; i < userInputs.N; i++)
        {
            double p = gs[i] / gsSum;
            q += p;
            ps.Add(p);
            qs.Add(q);
        }

        List<double> rs = new();
        List<double> xReals = new();
        List<string> xBins = new();

        for (int i = 0; i < userInputs.N; i++)
        {
            double r = rand.NextDouble();
            int xCrossIndex = Utils.GetCDFIndex(r, qs);
            double xPreCrossReal = xs[xCrossIndex];
            string xPreCrossBin = Utils.Real2Bin(xPreCrossReal, userInputs.a, userInputs.b, userInputs.l);
            rs.Add(r);
            xReals.Add(xPreCrossReal);
            xBins.Add(xPreCrossBin);
        }

        return new SelectionResults
        {
            gs = gs,
            ps = ps,
            qs = qs,
            rs = rs,
            xReals = xReals,
            xBins = xBins
        };
    }

    public CrossoverResults Crossover(in UserInputs userInputs, in SelectionResults select)
    {
        // find number of parents
        List<bool> parents = new();
        int numOfParents = 0;

        for (int i = 0; i < userInputs.N; i++)
        {
            bool parent = rand.NextDouble() <= userInputs.pk;
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
        List<List<int>?> cuttingPoints = new(userInputs.N);
        List<string?> children = new();
        string? secondChild = null;
        int? cuttingPoint = null;

        for (int i = 0; i < userInputs.N; i++)
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
                    string R1 = select.xBins[i];

                    // find a match <3
                    int R2Index = (i + 1) % userInputs.N;
                    while (!parents[R2Index]) { R2Index = (R2Index + 1) % userInputs.N; }

                    string R2 = select.xBins[R2Index];

                    // cross chromosomes
                    cuttingPoint = rand.Next(1, userInputs.l);
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

        List<string> populationBin = new List<string>();
        for (int i = 0; i < userInputs.N; i++)
        {
            // parents die and children end up in their place
            if (parents[i]) populationBin.Add(children[i]!);
            else populationBin.Add(select.xBins[i]);
        }

        return new CrossoverResults
        {
            children = children,
            cuttingPoints = cuttingPoints,
            parents = parents,
            populationBin = populationBin,
            numOfParents = numOfParents
        };
    }

    public MutationResults Mutate(in UserInputs userInputs, List<string> popPostCross)
    {
        List<int?> mutationPoints = new();
        List<string> postMutBins = new();
        List<double> postMutReals = new();
        List<double> postMutFs = new();

        for (int i = 0; i < userInputs.N; i++)
        {
            double r = rand.NextDouble();
            int? mutPoint = null;
            if (r <= userInputs.pm)
            {
                mutPoint = rand.Next(0, userInputs.l);
            }
            mutationPoints.Add(mutPoint + 1);

            string postMutBin = popPostCross[i];
            if (mutPoint is not null)
            {
                char[] chars = postMutBin.ToCharArray();
                chars[mutPoint.Value] = chars[mutPoint.Value] == '0' ? '1' : '0';
                postMutBin = new string(chars);
            }
            postMutBins.Add(postMutBin);
            
            double postMutReal = Utils.Bin2Real(postMutBin, userInputs.a, userInputs.b, userInputs.l);
            postMutReal = Math.Round(postMutReal, userInputs.decimalPlaces);
            postMutReals.Add(postMutReal);

            double postMutF = userInputs.f(postMutReal);
            postMutF = Math.Round(postMutF, userInputs.decimalPlaces);
            postMutFs.Add(postMutF);

        }
        return new MutationResults
        {
            mutationPoints = mutationPoints,
            xBins = postMutBins,
            population = new Population { xs = postMutReals, fs = postMutFs }
        };
    }
}
