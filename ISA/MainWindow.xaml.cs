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
        double fExtreme;

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
            
            //createTable();
            fillTable(a,b,d,N,l,pk,pm);
        }

        //private void createTable()
        //{
        //    dataGrid.Columns.Clear();
        //    dataGrid.ItemsSource = null;

        //    dataGrid.Columns.Add(new DataGridTextColumn { Header = "Lp", Binding = new Binding("Lp") });
        //    dataGrid.Columns.Add(new DataGridTextColumn { Header = "x real", Binding = new Binding("XReal") });
        //    dataGrid.Columns.Add(new DataGridTextColumn { Header = "f(x)", Binding = new Binding("Fx") });
        //    dataGrid.Columns.Add(new DataGridTextColumn { Header = "g(x)", Binding = new Binding("Gx") });
        //    dataGrid.Columns.Add(new DataGridTextColumn { Header = "p", Binding = new Binding("P") });
        //    dataGrid.Columns.Add(new DataGridTextColumn { Header = "q", Binding = new Binding("Q") });
        //    dataGrid.Columns.Add(new DataGridTextColumn { Header = "r", Binding = new Binding("R") });
        //    dataGrid.Columns.Add(new DataGridTextColumn { Header = "x real*", Binding = new Binding("XCrossReal") });
        //    dataGrid.Columns.Add(new DataGridTextColumn { Header = "x bin*", Binding = new Binding("XCrossBin") });
        //    //dataGrid.Columns.Add(new DataGridTextColumn { Header = "rodzice", Binding = new Binding("Parents") });
        //    AddColoredColumn("rodzice", "ParentsRed", "ParentsBlue");
        //    dataGrid.Columns.Add(new DataGridTextColumn { Header = "pc", Binding = new Binding("Pc") });
        //    //dataGrid.Columns.Add(new DataGridTextColumn { Header = "potomek", Binding = new Binding("Child") });
        //    AddColoredColumn("potomek", "ChildRed", "ChildBlue");
        //    dataGrid.Columns.Add(new DataGridTextColumn { Header = "pop. po krzyżowaniu", Binding = new Binding("PopulationPostCross") });
        //    dataGrid.Columns.Add(new DataGridTextColumn { Header = "punkt mutacji", Binding = new Binding("MutPoint") });
        //    dataGrid.Columns.Add(new DataGridTextColumn { Header = "po mutacji", Binding = new Binding("PostMutBin") });
        //    dataGrid.Columns.Add(new DataGridTextColumn { Header = "po mutacji real", Binding = new Binding("PostMutReal") });
        //    dataGrid.Columns.Add(new DataGridTextColumn { Header = "f(x) po mut", Binding = new Binding("PostMutFx") });
        //}
        
        private void fillTable(double a, double b, double d, int N, int l, double pk, double pm)
        {
            int decimalPlaces = (int)Math.Ceiling(-Math.Log10(d));
            var tableData = new List<TableRow>();
            Random rand = new Random();

            fExtreme = functionGoal == FunctionGoal.Max ? double.MinValue : double.MaxValue;

            // pre compute: x, f(x)
            List<double> xs = new List<double>();
            List<double> fs = new List<double>();

            for (int i = 0; i < N; i++)
            {
                double xReal = a + (b - a) * rand.NextDouble();
                double fx = F(xReal);

                if (functionGoal == FunctionGoal.Max ? fx > fExtreme : fx < fExtreme) 
                    fExtreme = fx;

                xs.Add(xReal);
                fs.Add(fx);
            }

            // g(x)
            List<double> gs = new List<double>();
            double gsSum = 0;

            for (int i = 0; i < N; i++)
            {
                double gx = G(xs[i], d);
                gsSum += gx;
                gs.Add(gx);
            }

            // p, q
            double q = 0;
            List<double> ps = new List<double>();
            List<double> qs = new List<double>();

            for (int i = 0; i < N; i++)
            {
                double p = gs[i] / gsSum;
                q += p;
                ps.Add(p);
                qs.Add(q);
            }

            // SELECTION
            List<double> rs = new List<double>();
            List<double> xCrossReals = new List<double>();
            List<string> xCrossBins = new List<string>();

            for (int i = 0; i < N; i++)
            {
                double r = rand.NextDouble();
                int xCrossIndex = GetCDFIndex(r, qs);
                double xCrossReal = xs[xCrossIndex];
                string xCrossBin = Real2Bin(xCrossReal, a, b, l);
                rs.Add(r);
                xCrossReals.Add(xCrossReal);
                xCrossBins.Add(xCrossBin);
            }

            // CROSSOVER
            // find number of parents
            List<bool> parents = new List<bool>();
            int numOfParents = 0;

            for (int i = 0; i < N; i++) {
                bool parent = rand.NextDouble() <= pk;
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
            List<string?> children = new List<string?>();
            List<List<int>?> cuttingPoints = new List<List<int>?>(N);
            string? secondChild = null;
            int? cuttingPoint = null;

            for (int i = 0; i < N; i++) {
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
                        string R1 = xCrossBins[i];

                        // find a match <3
                        int R2Index = (i + 1) % N;
                        while (!parents[R2Index]) { R2Index = (R2Index + 1) % N; }

                        string R2 = xCrossBins[R2Index];

                        // cross chromosomes
                        cuttingPoint = rand.Next(1, l);
                        child = R1[..(int)cuttingPoint] + R2[(int)cuttingPoint..];
                        secondChild = R2[..(int)cuttingPoint] + R1[(int)cuttingPoint..];

                        // if current parent is a paramour of R2
                        if (R2Index < i)
                        {
                            // add the current cutting point to it
                            cuttingPoints[R2Index].Add((int)cuttingPoint);
                        }
                        else if (R2Index == i)
                        {
                            // self breeding is not allowed!
                            child = null;
                        }

                    }
                    cuttingPoints.Add(new List<int> { (int)cuttingPoint });
                }
                else
                {
                    cuttingPoints.Add(null);
                }

                children.Add(child);
            }

            List<string> popPostCross = new List<string>();
            for (int i = 0; i < N; i++)
            {
                // parents die and children end up in their place
                if (parents[i]) popPostCross.Add(children[i]);
                else popPostCross.Add(xCrossBins[i]);
            }

            // filling the table
            bool evenParent = true;
            int foundParents = 0;
            for (int i = 0; i < N; i++) {
                string parentFirstPart = "-";
                string parentSecondPart = "";
                string childFirstPart = "-";
                string childSecondPart = "";
                string childFirstColor = "Black";
                string childSecondColor = "Black";
                string parentColor = "Black";

                if (parents[i])
                {
                    evenParent = !evenParent;
                    foundParents++;

                    parentFirstPart = xCrossBins[i][..cuttingPoints[i][0]];
                    parentSecondPart = xCrossBins[i][cuttingPoints[i][0]..];
                    parentColor = !evenParent ? "Red" : "Blue";
                    childFirstPart = children[i][..cuttingPoints[i][0]];
                    childSecondPart = children[i][cuttingPoints[i][0]..];
                    childFirstColor = !evenParent ? "Red" : "Blue";
                    childSecondColor = evenParent ? "Red" : "Blue";

                    if (!evenParent && foundParents == numOfParents)
                    {
                        parentColor = "Black";
                        childFirstColor = "Black";
                        childSecondColor = "Black";
                    }
                }

                tableData.Add(new TableRow
                {
                    Lp = i + 1,
                    XReal = Math.Round(xs[i], decimalPlaces),
                    Fx = Math.Round(fs[i], decimalPlaces),
                    Gx = Math.Round(gs[i], decimalPlaces),
                    P = Math.Round(ps[i], decimalPlaces),
                    Q = Math.Round(qs[i], decimalPlaces),
                    R = Math.Round(rs[i], decimalPlaces),
                    XCrossReal = Math.Round(xCrossReals[i], decimalPlaces),
                    XCrossBin = xCrossBins[i],
                    ParentFirstPart = parentFirstPart,
                    ParentSecondPart = parentSecondPart,
                    ParentColor = parentColor,
                    Pc = cuttingPoints[i] == null ? "-" : string.Join(',', cuttingPoints[i]),
                    ChildFirstPart = childFirstPart,
                    ChildSecondPart = childSecondPart,
                    ChildFirstColor = childFirstColor,
                    ChildSecondColor = childSecondColor,
                    PopulationPostCross = popPostCross[i],
                    /*MutPoint = mutPoint,
                    PostMutBin = postMutBin,
                    PostMutReal = postMutReal,
                    PostMutFx = postMutFx,*/
                });
            }

            dataGrid.ItemsSource = tableData;

            double rowHeight = 22;
            dataGrid.MaxHeight = rowHeight * 20;
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
        private static double F(double x)
        {
            return - (x + 1) * (x - 1) * (x - 2);
        }
        private static double Gmax(double x, double fMin, double d)
        {
            return F(x) - fMin + d;
        }
        private static double Gmin(double x, double fMax, double d)
        {
            return -(F(x) - fMax) + d;
        }
        private double G(double x, double d)
        {
            return functionGoal == FunctionGoal.Max ? 
                Gmax(x, fExtreme, d) : Gmin(x, fExtreme, d);
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
        public string? PostMutReal { get; set; }
        public string? PostMutFx { get; set; }
    }
}
