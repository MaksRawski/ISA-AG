using System.Globalization;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ISA
{
    public partial class MainWindow : Window
    {
        enum FunctionGoal
        {
            Max,
            Min,
        }
        FunctionGoal functionGoal = FunctionGoal.Min;

        struct UserInputs
        {
            public double a, b, d, pk, pm;
            public int N, l;
        }
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
        struct Mutation
        {
            public List<int?> mutationPoints;
            public List<string> xBins;
            public Population population;
        }

        Random rand = new Random();
        UserInputs userInputs;


        public MainWindow()
        {
            InitializeComponent();

            dComboBox.Items.Add(0.1);
            dComboBox.Items.Add(0.01);
            dComboBox.Items.Add(0.001);
            dComboBox.Items.Add(0.0001);

            aLineEdit.Text = "-2";
            bLineEdit.Text = "3";
            dComboBox.SelectedIndex = 2;
            NLineEdit.Text = "10";
            PkLineEdit.Text = "0.75";
            PmLineEdit.Text = "0.002";

            aLineEdit.TextChanged += ValidateInputs;
            bLineEdit.TextChanged += ValidateInputs;
            NLineEdit.TextChanged += ValidateInputs;
            dComboBox.SelectionChanged += ValidateInputs;
            PkLineEdit.TextChanged += ValidateInputs;
            PmLineEdit.TextChanged += ValidateInputs;
        }
        private void ValidateInputs(object sender, EventArgs e)
        {
            bool isValid = TryParseDouble(aLineEdit.Text, out double a) &&
                           TryParseDouble(bLineEdit.Text, out double b) && a < b &&
                           int.TryParse(NLineEdit.Text, out int n) && n >= 0 &&
                           TryParseDouble(PkLineEdit.Text, out double pk) && pk >= 0 && pk <= 1 &&
                           TryParseDouble(PmLineEdit.Text, out double pm) && pm >= 0 && pm <= 1 &&
                           dComboBox.SelectedItem != null;

            startButton.IsEnabled = isValid;
        }

        private bool TryParseDouble(string input, out double n)
        {
            return double.TryParse(input.Replace(',', '.'), NumberStyles.Number, CultureInfo.InvariantCulture, out n);
        }
        private void StartButton_Click(object sender, RoutedEventArgs e)
        {
            TryParseDouble(aLineEdit.Text, out double a);
            TryParseDouble(bLineEdit.Text, out double b);
            double d = (double)dComboBox.SelectedItem;
            int N = int.Parse(NLineEdit.Text);
            int l = (int)Math.Ceiling(Math.Log2((b - a) / d + 1));
            TryParseDouble(PkLineEdit.Text, out double pk);
            TryParseDouble(PmLineEdit.Text, out double pm);
            userInputs = new UserInputs
            {
                a = a,
                b = b,
                d = d,
                pk = pk,
                pm = pm,
                N = N,
                l = l,
            };
            fillTable(userInputs);
        }

        private void fillTable(UserInputs userInputs)
        {
            int decimalPlaces = (int)Math.Ceiling(-Math.Log10(userInputs.d));
            var tableData = new List<TableRow>();

            // fx, xs
            Population population = generatePopulation(userInputs, functionGoal, out double fExtreme);

            // SELECTION -> gs, ps, qs, rs, xPreCrossReals, xPreCrossBins
            SelectionResults selection = Select(userInputs, population.xs, fExtreme);

            // CROSSOVER -> parents, cuttingPoints, children, popPostCross
            CrossoverResults crossover = Crossover(userInputs, selection);

            // MUTATION -> mutationPoints, postMutBins, postMutReals, postMutFs
            Mutation mutation = Mutate(userInputs, crossover.populationBin);

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

        private Population generatePopulation(UserInputs userInputs, FunctionGoal functionGoal, out double fExtreme)
        {
            fExtreme = functionGoal == FunctionGoal.Max ? double.MaxValue : double.MinValue;
            List<double> xs = new List<double>();
            List<double> fs = new List<double>();

            for (int i = 0; i < userInputs.N; i++)
            {
                double xReal = userInputs.a + (userInputs.b - userInputs.a) * rand.NextDouble();
                double fx = F(xReal);

                // max: g(x) = f(x) - fmin + d
                // min: g(x) = -(f(x) - fmax) + d
                if (functionGoal == FunctionGoal.Max ? fx < fExtreme : fx > fExtreme)
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
                double gx = G(xs[i], fExtreme, userInputs.d);
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
                string xPreCrossBin = Real2Bin(xPreCrossReal, userInputs.a, userInputs.b, userInputs.l);
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

        private Mutation Mutate(UserInputs userInputs, List<string> popPostCross)
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
                double postMutReal = Bin2Real(postMutBin, userInputs.a, userInputs.b, userInputs.l);
                postMutReals.Add(postMutReal);
                postMutFs.Add(F(postMutReal));
            }
            return new Mutation {
                mutationPoints = mutationPoints,
                xBins = postMutBins,
                population = new Population { xs = postMutReals, fs = postMutFs }
            };
        }
        // TODO refactor static functions into a different file
        public static int Bin2Int(string binaryString)
        {
            return Convert.ToInt32(binaryString, 2);
        }
        public static string Int2Bin(int x, int l)
        {
            return Convert.ToString(x, 2).PadLeft(l, '0');
        }

        public static double Int2Real(int x, double a, double b, int l)
        {
            return x * (b - a) / (Math.Pow(2, l) - 1) + a;
        }
        public static int Real2Int(double x, double a, double b, int l)
        {
            return (int)((x - a) / (b - a) * (Math.Pow(2, l) - 1));
        }

        public static string Real2Bin(double x, double a, double b, int l)
        {
            return Int2Bin(Real2Int(x, a, b, l), l);
        }
        public static double Bin2Real(string x, double a, double b, int l)
        {
            return Int2Real(Bin2Int(x), a, b, l);
        }

        private static double F(double x)
        {
            return -(x + 1) * (x - 1) * (x - 2);
        }
        private static double Gmax(double x, double fMin, double d)
        {
            return F(x) - fMin + d;
        }
        private static double Gmin(double x, double fMax, double d)
        {
            return -(F(x) - fMax) + d;
        }
        private double G(double x, double fExtreme, double d)
        {
            return functionGoal == FunctionGoal.Max ?
                Gmax(x, fExtreme, d) : Gmin(x, fExtreme, d);
        }

        /// <summary>
        /// Finds the index of the smallest element in the sorted list 'qs' that is greater than or equal to the given value 'r'.
        /// This function performs a binary search on the list to achieve O(log n) time complexity.
        /// </summary>
        /// <param name="r">The threshold value to compare against the elements in 'qs'.</param>
        /// <param name="qs">A sorted list of double values representing cumulative distribution function (CDF) values.</param>
        /// <returns>The index of the first element in 'qs' that is greater than or equal to 'r'.</returns>
        public static int GetCDFIndex(double r, List<double> qs)
        {
            int low = 0, high = qs.Count - 1;

            while (low < high)
            {
                int mid = (low + high) / 2;

                if (qs[mid] < r)
                    low = mid + 1;
                else
                    high = mid;
            }

            return low;
        }
    }

    public class TableRow
    {
        public int Lp { get; set; }
        public double XReal { get; set; }
        public double Fx { get; set; }
        public double Gx { get; set; }
        public double P { get; set; }
        public double Q { get; set; }
        public double R { get; set; }
        public double XCrossReal { get; set; }
        public string? XCrossBin { get; set; }
        public string? ParentFirstPart { get; set; }
        public string? ParentSecondPart { get; set; }
        public string? ParentColor { get; set; }
        public string? Pc { get; set; }
        public string? ChildFirstPart { get; set; }
        public string? ChildSecondPart { get; set; }
        public string? ChildFirstColor { get; set; }
        public string? ChildSecondColor { get; set; }
        public string? PopulationPostCross { get; set; }
        public string? MutPoint { get; set; }
        public string? PostMutBin { get; set; }
        public double PostMutReal { get; set; }
        public double PostMutFx { get; set; }
    }
}
