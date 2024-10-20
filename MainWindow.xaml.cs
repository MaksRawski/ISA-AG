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

namespace Lab1
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

            aLineEdit.TextChanged += ValidateInputs;
            bLineEdit.TextChanged += ValidateInputs;
            NLineEdit.TextChanged += ValidateInputs;
            dComboBox.SelectionChanged += ValidateInputs;
        }
        private void ValidateInputs(object sender, EventArgs e)
        {
            bool isValid = ValidateInput(aLineEdit.Text) &&
                           ValidateInput(bLineEdit.Text) &&
                           ValidateInput(NLineEdit.Text) &&
                           dComboBox.SelectedItem != null;
            if (isValid)
            {
                double a = double.Parse(aLineEdit.Text);
                double b = double.Parse(bLineEdit.Text);
                if (a >= b) isValid = false;
            }

            startButton.IsEnabled = isValid;
        }

        private bool ValidateInput(string input)
        {
            return double.TryParse(input, NumberStyles.Any, CultureInfo.InvariantCulture, out _);
        }
        private void StartButton_Click(object sender, RoutedEventArgs e)
        {
            double a = double.Parse(aLineEdit.Text);
            double b = double.Parse(bLineEdit.Text);
            double d = (double)dComboBox.SelectedItem;
            int N = int.Parse(NLineEdit.Text);
            int l = (int)Math.Ceiling(Math.Log2((b - a) / d + 1));
            int decimalPlaces = (int)Math.Ceiling(-Math.Log10(d));
            
            createTable();
            fillTable(a,b,d,N,l,decimalPlaces);
        }

        private void createTable()
        {
            dataGrid.Columns.Clear();
            dataGrid.ItemsSource = null;

            dataGrid.Columns.Add(new DataGridTextColumn { Header = "Lp", Binding = new Binding("Lp") });
            dataGrid.Columns.Add(new DataGridTextColumn { Header = "x real", Binding = new Binding("xReal") });
            dataGrid.Columns.Add(new DataGridTextColumn { Header = "f(x)", Binding = new Binding("fx") });
            dataGrid.Columns.Add(new DataGridTextColumn { Header = "g(x)", Binding = new Binding("gx") });
            dataGrid.Columns.Add(new DataGridTextColumn { Header = "p", Binding = new Binding("p") });
            dataGrid.Columns.Add(new DataGridTextColumn { Header = "q", Binding = new Binding("q") });
            dataGrid.Columns.Add(new DataGridTextColumn { Header = "r", Binding = new Binding("r") });
            dataGrid.Columns.Add(new DataGridTextColumn { Header = "x real*", Binding = new Binding("xReal1") });
            dataGrid.Columns.Add(new DataGridTextColumn { Header = "x bin*", Binding = new Binding("xBin1") });
            dataGrid.Columns.Add(new DataGridTextColumn { Header = "rodzice", Binding = new Binding("rodzice") });
            dataGrid.Columns.Add(new DataGridTextColumn { Header = "pc", Binding = new Binding("pc") });
            dataGrid.Columns.Add(new DataGridTextColumn { Header = "potomek", Binding = new Binding("potomek") });
            dataGrid.Columns.Add(new DataGridTextColumn { Header = "pop. po krzyżowaniu", Binding = new Binding("popPostCross") });
            dataGrid.Columns.Add(new DataGridTextColumn { Header = "punkt mutacji", Binding = new Binding("mutPoint") });
            dataGrid.Columns.Add(new DataGridTextColumn { Header = "po mutacji", Binding = new Binding("postMutBin") });
            dataGrid.Columns.Add(new DataGridTextColumn { Header = "po mutacji real", Binding = new Binding("postMutReal") });
            dataGrid.Columns.Add(new DataGridTextColumn { Header = "f(x) po mut", Binding = new Binding("postMutFx") });
        }
        private void fillTable(double a, double b, double d, int N, int l, int decimalPlaces)
        {
            var tableData = new List<TableRow>();
            Random rand = new Random();

            fExtreme = functionGoal == FunctionGoal.Max ? double.MinValue : double.MaxValue;

            // pre compute x, f(x), g(x)
            List<double> xs = new List<double>();
            List<double> fs = new List<double>();
            List<double> gs = new List<double>();

            for (int i = 0; i < N; i++)
            {
                double xReal = a + (b - a) * rand.NextDouble();
                double fx = F(xReal);
                double gx = G(xReal, d);

                if (functionGoal == FunctionGoal.Max ? fx > fExtreme : fx < fExtreme) 
                    fExtreme = fx;

                xs.Add(xReal);
                fs.Add(fx);
                gs.Add(gx);

            }

            // p, q
            double q = 0;
            List<double> ps = new List<double>();
            List<double> qs = new List<double>();

            for (int i = 0; i < N; i++)
            {
                double p = gs[i] / gs.Sum();
                q += p;
                ps.Add(p);
                qs.Add(q);
            }

            // filling the table
            for (int i = 0; i < N; i++) {
                double xReal = xs[i];
                double fx = fs[i];
                double gx = gs[i];
                double p = ps[i];


                double r = rand.NextDouble();
         
                int xCrossIndex = GetCDFIndex(r, qs);
                double xCrossReal = xs[xCrossIndex];
                string xCrossBin = Real2Bin(xCrossReal, a, b, l);


                tableData.Add(new TableRow
                {
                    Lp = i + 1,
                    XReal = xReal,
                    Fx = fx,
                    Gx = gx,
                    P = p,
                    Q = q,
                    R = r,
                    XCrossReal = xCrossReal,
                    XCrossBin = xCrossBin,
                    Rodzice = rodzice,
                    Pc = pc,
                    Potomek = potomek,
                    PopPostCross = popPostCross,
                    MutPoint = mutPoint,
                    PostMutBin = postMutBin,
                    PostMutReal = postMutReal,
                    PostMutFx = postMutFx,
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
        private double F(double x)
        {
            return - (x + 1) * (x - 1) * (x - 2);
        }
        private double Gmax(double x, double fMin, double d)
        {
            return F(x) - fMin + d;
        }
        private double Gmin(double x, double fMax, double d)
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
        public string? Rodzice { get; set; }
        public int Pc { get; set; }
        public string? Potomek { get; set; }
        public string? PopPostCross { get; set; }
        public string? MutPoint { get; set; }
        public string? PostMutBin { get; set; }
        public string? PostMutReal { get; set; }
        public string? PostMutFx { get; set; }
    }
}
