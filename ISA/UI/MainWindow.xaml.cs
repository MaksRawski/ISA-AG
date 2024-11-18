using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using Core;

namespace UI
{
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

            fLineEdit.Text = "mod(x,1) * (cos(20*pi*x) - sin(x))";
            aLineEdit.Text = "-4";
            bLineEdit.Text = "12";
            dComboBox.SelectedIndex = 2;
            functionGoalComboBox.SelectedIndex = 0;
            NLineEdit.Text = "10";
            TLineEdit.Text = "50";
            PkLineEdit.Text = "0.75";
            PmLineEdit.Text = "0.002";
            elitismCheckbox.IsChecked = true;

            fLineEdit.TextChanged += ValidateInputs;
            aLineEdit.TextChanged += ValidateInputs;
            bLineEdit.TextChanged += ValidateInputs;
            NLineEdit.TextChanged += ValidateInputs;
            dComboBox.SelectionChanged += ValidateInputs;
            PkLineEdit.TextChanged += ValidateInputs;
            PmLineEdit.TextChanged += ValidateInputs;
            elitismCheckbox.Checked += ValidateInputs;
            elitismCheckbox.Unchecked += ValidateInputs;
        }
        private static bool TryParseDouble(string input, out double n)
        {
            return double.TryParse(input.Replace(',', '.'), NumberStyles.Number, CultureInfo.InvariantCulture, out n);
        }
        private void ValidateInputs(object sender, EventArgs e)
        {
            bool isValid = TryParseDouble(aLineEdit.Text, out double a) &&
                           TryParseDouble(bLineEdit.Text, out double b) && a < b &&
                           int.TryParse(NLineEdit.Text, out int n) && n >= 0 &&
                           int.TryParse(TLineEdit.Text, out int t) && t >= 0 &&
                           TryParseDouble(PkLineEdit.Text, out double pk) && pk >= 0 && pk <= 1 &&
                           TryParseDouble(PmLineEdit.Text, out double pm) && pm >= 0 && pm <= 1 &&
                           Utils.TryParseFunction(fLineEdit.Text, out var _) && 
                           dComboBox.SelectedItem != null;

            startButton.IsEnabled = isValid;
        }
        private void StartButton_Click(object sender, RoutedEventArgs e)
        {
            _ = TryParseDouble(aLineEdit.Text, out double a);
            _ = TryParseDouble(bLineEdit.Text, out double b);
            double d = (double)dComboBox.SelectedItem;
            _ = TryParseDouble(PkLineEdit.Text, out double pk);
            _ = TryParseDouble(PmLineEdit.Text, out double pm);

            FunctionGoal functionGoal = 
                functionGoalComboBox.SelectedIndex == 0 ? FunctionGoal.Max : FunctionGoal.Min;
            Func<double, double> f = Utils.ParseFunction(fLineEdit.Text);

            var inputs = new UserInputs(
                GenotypeSpace.FromD(d, a, b),
                N: int.Parse(NLineEdit.Text),
                T: int.Parse(TLineEdit.Text),
                pk,
                pm,
                elitism: (bool)elitismCheckbox.IsChecked!,
                f,
                functionGoal
            );
            var algo = new Algorithm(inputs);
            algo.Run(out List<TableRow> rows);
            DaneDataGrid.ItemsSource = rows;

            double rowHeight = 22;
            DaneDataGrid.MaxHeight = rowHeight * 20;

        }
    }
}
