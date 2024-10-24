using System.Globalization;
using System.Windows;

namespace ISA
{
    struct UserInputs
    {
        public double a, b, d, pk, pm;
        public int N, l;
        public FunctionGoal functionGoal;
        public Func<double, double> f;
    }
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            dComboBox.Items.Add(0.1);
            dComboBox.Items.Add(0.01);
            dComboBox.Items.Add(0.001);
            dComboBox.Items.Add(0.0001);

            functionGoalComboBox.Items.Add("MAX");
            functionGoalComboBox.Items.Add("MIN");

            aLineEdit.Text = "-2";
            bLineEdit.Text = "3";
            dComboBox.SelectedIndex = 2;
            functionGoalComboBox.SelectedIndex = 1;
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
        private bool TryParseDouble(string input, out double n)
        {
            return double.TryParse(input.Replace(',', '.'), NumberStyles.Number, CultureInfo.InvariantCulture, out n);
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
        private void StartButton_Click(object sender, RoutedEventArgs e)
        {
            _ = TryParseDouble(aLineEdit.Text, out double a);
            _ = TryParseDouble(bLineEdit.Text, out double b);
            double d = (double)dComboBox.SelectedItem;
            int N = int.Parse(NLineEdit.Text);
            int l = (int)Math.Ceiling(Math.Log2((b - a) / d + 1));
            _ = TryParseDouble(PkLineEdit.Text, out double pk);
            _ = TryParseDouble(PmLineEdit.Text, out double pm);
            FunctionGoal functionGoal = 
                functionGoalComboBox.SelectedIndex == 0 ? FunctionGoal.Max : FunctionGoal.Min;

            var userInputs = new UserInputs
            {
                a = a,
                b = b,
                d = d,
                pk = pk,
                pm = pm,
                N = N,
                l = l,
                functionGoal = functionGoal,
                f = (double x) => -(x + 1) * (x - 1) * (x - 2),
            };
            var algo = new Algorithm();
            algo.FillTable(userInputs, dataGrid);
        }
    }
}
