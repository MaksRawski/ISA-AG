using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Core;
using System.IO;

namespace UI
{
    public partial class TestyTab : UserControl, INotifyPropertyChanged
    {
        private double progressPercentage;
        private readonly ExperimentsRunner experimentsRunner;
        private class TableRow
        {
            public int Lp;
            public string ParameterSet;
            public double Fave;

            public TableRow(int lp, string parameterSet, double fave)
            {
                Lp = lp;
                ParameterSet = parameterSet;
                Fave = fave;
            }
        }
        private class TableRowsFactory
        {
            private int Lp = 1;
            private readonly List<TableRow> Rows = new();

            public void AddRow(ExperimentParameterSet parameterSet, double fAve)
            {
                string parameters = $"{parameterSet.N},{parameterSet.pk},{parameterSet.pm},{parameterSet.T}";
                var row = new TableRow(Lp++, parameters, fAve);
                Rows.Add(row);
            }

            public List<TableRow> Build() {
                return Rows;
            }
        }
        public double ProgressPercentage
        {
            get => progressPercentage;
            set
            {
                progressPercentage = value;
                OnPropertyChanged(nameof(ProgressPercentage)); 
            }
        }

        private int completedExperiments;
        public int CompletedExperiments
        {
            get => completedExperiments;
            set { completedExperiments = value; OnPropertyChanged(); }
        }

        private int totalExperiments;
        public int TotalExperiments
        {
            get => totalExperiments;
            set { totalExperiments = value; OnPropertyChanged(); }
        }

        public TestyTab()
        {
            InitializeComponent();
            DataContext = this;

            // parameters to test
            List<int> Ns = new() { 30, 35, 40, 45, 50, 55, 60, 65, 70, 75, 80 };
            List<double> pks = new() { 0.5, 0.55, 0.6, 0.65, 0.7, 0.75, 0.8, 0.85, 0.9 };
            List<double> pms = new() { 0.0001, 0.0005, 0.001, 0.005, 0.01 };
            List<int> Ts = new() { 50, 60, 70, 80, 90, 100 };

            experimentsRunner = new ExperimentsRunner(Ns, pks, pms, Ts);
            totalExperiments = experimentsRunner.totalCombinations;
        }

        private async void Start(object sender, RoutedEventArgs e)
        {
            TestyStartButton.IsEnabled = false;

            CompletedExperiments = 0; // Reset the counter
            TableRowsFactory tableRowsFactory = new();

            var progress = new Progress<(ExperimentParameterSet, double)>(result =>
            {
                var (parameterSet, fx) = result;

                CompletedExperiments++;
                ProgressPercentage = (double)CompletedExperiments / TotalExperiments * 100;

                Debug.WriteLine($"Progress report: {ProgressPercentage:F2}%");
                tableRowsFactory.AddRow(parameterSet, fx);
            });

            // Await the Run method
            await experimentsRunner.Run(progress);

            ExperimentResultsDataGrid.ItemsSource = tableRowsFactory.Build();
            TestyStartButton.IsEnabled = true;
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}