using System.Windows.Controls;

namespace ISA;

internal class Algorithm
{
    struct Population
    {
        public List<double> xs, fs;
    }

    struct SelectionResults
    {
        public List<double> gs, ps, qs, rs, xReals;
        public List<string> xBins;
    }

    struct CrossoverResults
    {
        public List<bool> parents;
        public List<string?> children;
        public List<List<int>?> cuttingPoints;
        public List<string> populationBin;
        public int numOfParents;
    }

    struct MutationResults
    {
        public List<int?> mutationPoints;
        public List<string> xBins;
        public Population population;
    }

    Random rand = new();
    
    public void FillTable(UserInputs userInputs, ref DataGrid dataGrid)
    {
        int decimalPlaces = (int)Math.Ceiling(-Math.Log10(userInputs.d));
        var tableData = new List<TableRow>();

        // fx, xs
        Population population = GeneratePopulation(userInputs, out double fExtreme);

        // SELECTION -> gs, ps, qs, rs, xPreCrossReals, xPreCrossBins
        SelectionResults selection = Select(userInputs, population.xs, fExtreme);

        // CROSSOVER -> parents, cuttingPoints, children, popPostCross
        CrossoverResults crossover = Crossover(userInputs, selection);

        // MUTATION -> mutationPoints, postMutBins, postMutReals, postMutFs
        MutationResults mutation = Mutate(userInputs, crossover.populationBin);

        // filling the table
        bool evenParent = true;
        int foundParents = 0;
        for (int i = 0; i < userInputs.N; i++)
        {
            string parentFirstPart = "-";
            string parentSecondPart = "";
            string childFirstPart = "-";
            string childSecondPart = "";
            string childFirstColor = "Black";
            string childSecondColor = "Black";
            string parentColor = "Black";

            if (crossover.parents[i])
            {
                evenParent = !evenParent;
                foundParents++;

                parentFirstPart = selection.xBins[i][..crossover.cuttingPoints[i]![0]];
                parentSecondPart = selection.xBins[i][crossover.cuttingPoints[i]![0]..];
                parentColor = !evenParent ? "Red" : "Blue";
                childFirstPart = crossover.children[i]![..crossover.cuttingPoints[i]![0]];
                childSecondPart = crossover.children[i]![crossover.cuttingPoints[i]![0]..];
                childFirstColor = !evenParent ? "Red" : "Blue";
                childSecondColor = evenParent ? "Red" : "Blue";

                if (!evenParent && foundParents == crossover.numOfParents)
                {
                    parentColor = "Black";
                    childFirstColor = "Black";
                    childSecondColor = "Black";
                }
            }

            tableData.Add(new TableRow
            {
                Lp = i + 1,
                XReal = Math.Round(population.xs[i], decimalPlaces),
                Fx = Math.Round(population.fs[i], decimalPlaces),
                Gx = Math.Round(selection.gs[i], decimalPlaces),
                P = Math.Round(selection.ps[i], decimalPlaces),
                Q = Math.Round(selection.qs[i], decimalPlaces),
                R = Math.Round(selection.rs[i], decimalPlaces),
                XCrossReal = Math.Round(selection.xReals[i], decimalPlaces),
                XCrossBin = selection.xBins[i],
                ParentFirstPart = parentFirstPart,
                ParentSecondPart = parentSecondPart,
                ParentColor = parentColor,
                Pc = crossover.cuttingPoints[i] == null ? "-" : string.Join(',', crossover.cuttingPoints[i]!),
                ChildFirstPart = childFirstPart,
                ChildSecondPart = childSecondPart,
                ChildFirstColor = childFirstColor,
                ChildSecondColor = childSecondColor,
                PopulationPostCross = crossover.populationBin[i],
                MutPoint = mutation.mutationPoints[i]?.ToString() ?? "-",
                PostMutBin = mutation.xBins[i],
                PostMutReal = Math.Round(mutation.population.xs[i], decimalPlaces),
                PostMutFx = Math.Round(mutation.population.fs[i], decimalPlaces),
            });
        }

        dataGrid.ItemsSource = tableData;

        double rowHeight = 22;
        dataGrid.MaxHeight = rowHeight * 20;
    }
    private Population GeneratePopulation(UserInputs userInputs, out double fExtreme)
    {
        fExtreme = userInputs.functionGoal == FunctionGoal.Max ? double.MaxValue : double.MinValue;
        List<double> xs = new();
        List<double> fs = new();

        for (int i = 0; i < userInputs.N; i++)
        {
            double xReal = userInputs.a + (userInputs.b - userInputs.a) * rand.NextDouble();
            double fx = userInputs.f(xReal);

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
    
    private SelectionResults Select(UserInputs userInputs, List<double> xs, double fExtreme)
    {
        List<double> gs = new();
        double gsSum = 0;

        for (int i = 0; i < userInputs.N; i++)
        {
            double gx = Utils.G(userInputs.f, xs[i], userInputs.functionGoal, fExtreme, userInputs.d);
            gsSum += gx;
            gs.Add(gx);
        }

        // p, q
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
            int xCrossIndex = GetCDFIndex(r, qs);
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

    private CrossoverResults Crossover(UserInputs userInputs, SelectionResults select)
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
                    // current cutting point will be added later
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

    private MutationResults Mutate(UserInputs userInputs, List<string> popPostCross)
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
            postMutReals.Add(postMutReal);
            postMutFs.Add(userInputs.f(postMutReal));
        }
        return new MutationResults
        {
            mutationPoints = mutationPoints,
            xBins = postMutBins,
            population = new Population { xs = postMutReals, fs = postMutFs }
        };
    }
}
