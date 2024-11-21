using System.Globalization;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using Core;
using ScottPlot;
using ScottPlot.Plottables;

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

            OptimizationGoal functionGoal = 
                functionGoalComboBox.SelectedIndex == 0 ? OptimizationGoal.Max : OptimizationGoal.Min;
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
            var population = algo.Run(out var stats);
            CreateScatterPlot(stats);
            algo.GroupPopulationIntoRows(population, out var rows);

            DaneDataGrid.ItemsSource = rows;
            double rowHeight = 22;
            DaneDataGrid.MaxHeight = rowHeight * 20;
        }
        private void CreateScatterPlot(AlgorithmStats stats)
        {
            WpfPlot1.Plot.Clear();
            var axesLimits = new AxisLimits(0, stats.T + 1, stats.Fmins.Min() - 0.2, stats.Fmaxs.Max() + 0.2);
            WpfPlot1.Plot.Axes.SetLimitsX(axesLimits);
            WpfPlot1.Plot.Axes.SetLimitsY(axesLimits);
            double[] xAxis = Enumerable.Range(1, stats.T).Select(i => (double)i).ToArray();

            var plmax = WpfPlot1.Plot.Add.Scatter(xAxis, stats.Fmaxs.ToArray()); plmax.LegendText = "f max";
            var plave = WpfPlot1.Plot.Add.Scatter(xAxis, stats.Faves.ToArray()); plave.LegendText = "f ave";
            var plmin = WpfPlot1.Plot.Add.Scatter(xAxis, stats.Fmins.ToArray()); plmin.LegendText = "f min";

            WpfPlot1.Plot.XLabel("T");
            WpfPlot1.Plot.YLabel("f(x)");
            WpfPlot1.Plot.ShowLegend();

            var highlightText = WpfPlot1.Plot.Add.Text("", 0, 0);
            highlightText.LabelAlignment = Alignment.LowerLeft;
            highlightText.LabelBold = true;
            highlightText.OffsetX = 7;
            highlightText.OffsetY = -7;

            WpfPlot1.MouseMove += (s, e) =>
            {
                var mousePosition = e.GetPosition(WpfPlot1);
                Pixel mousePixel = new(mousePosition.X, mousePosition.Y);
                Coordinates mouseLocation = WpfPlot1.Plot.GetCoordinates(mousePixel);
                DataPoint nearestPoint = plmax.Data.GetNearest(mouseLocation, WpfPlot1.Plot.LastRender);

                int nearestIndex = nearestPoint.Index;

                if (nearestPoint.IsReal)
                {
                    highlightText.IsVisible = true;
                    highlightText.Location = nearestPoint.Coordinates;
                    highlightText.LabelText = $"f max: {stats.Fmaxs[nearestIndex]:0.##}\n" +
                    $"f ave: {stats.Faves[nearestIndex]:0.##}\n" +
                    $"f min: {stats.Fmins[nearestIndex]:0.##}";
                    WpfPlot1.Refresh();
                }
            };

            WpfPlot1.Refresh();
        }
        private void TabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (WpfPlot1.IsVisible)
            {
                WpfPlot1.Refresh();
            }
        }
    }
}
