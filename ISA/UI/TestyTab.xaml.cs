using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using Core;
using System.IO;
using System.Collections.ObjectModel;

namespace UI
{
    public partial class TestyTab : UserControl, INotifyPropertyChanged
    {
        private double progressPercentage;
        private readonly ExperimentsRunner experimentsRunner;
        private readonly Stopwatch stopwatch = new();

        private class TableRow
        {
            public int Lp { get; set; }
            public string ParametersSet { get; set; }
            public double Fave { get; set; }

            public TableRow(int lp, string parameterSet, double fave)
            {
                Lp = lp;
                ParametersSet = parameterSet;
                Fave = fave;
            }
        }
        private ObservableCollection<TableRow> TableRows { get; } = new();
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
        private string elapsedTime;
        public string ElapsedTime
        {
            get => elapsedTime;
            set { elapsedTime = value; OnPropertyChanged(); }
        }
        private string remainingTime;
        public string RemainingTime
        {
            get => remainingTime;
            set { remainingTime = value; OnPropertyChanged(); }
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
            CompletedExperiments = 0;
            remainingTime = "";
            elapsedTime = "";
        }

        private void Start(object sender, RoutedEventArgs e)
        {
            TestyStartButton.IsEnabled = false;
            CompletedExperiments = 0;
            ProgressPercentage = 0;
            TableRows.Clear();
            stopwatch.Restart();

            ExperimentResultsDataGrid.ItemsSource = TableRows;
            ExperimentResultsDataGrid.MaxHeight = 22 * 20;

            TableRows.CollectionChanged += (s, e) =>
            {
                ExperimentResultsDataGrid.Items.SortDescriptions.Clear();

                ExperimentResultsDataGrid.Items.SortDescriptions.Add(
                    new SortDescription("Fave", ListSortDirection.Descending));

                ExperimentResultsDataGrid.Items.SortDescriptions.Add(
                    new SortDescription("ParameterSet", ListSortDirection.Ascending));

                ExperimentResultsDataGrid.Items.Refresh();
            };

            Progress<(ExperimentParameterSet, double)> progress = new (result =>
            {
                var (parameterSet, fx) = result;

                CompletedExperiments++;
                ProgressPercentage = (double)CompletedExperiments / TotalExperiments * 100;

                Application.Current.Dispatcher.Invoke(() =>
                {
                    string parameters = $"{parameterSet.N},{parameterSet.pk},{parameterSet.pm},{parameterSet.T}";
                    TableRows.Add(new TableRow(CompletedExperiments, parameters, fx));
                });
                if (CompletedExperiments > 0)
                {
                    double averageTimePerExperiment = stopwatch.Elapsed.TotalSeconds / CompletedExperiments;
                    double remainingSeconds = averageTimePerExperiment * (TotalExperiments - CompletedExperiments);
                    TimeSpan remainingTime = TimeSpan.FromSeconds(remainingSeconds);
                    ElapsedTime = $"{stopwatch.Elapsed:hh\\:mm\\:ss}";
                    RemainingTime = $"{remainingTime:hh\\:mm\\:ss}";
                }
            });

            experimentsRunner.Run(progress, onComplete: () =>
            {
                TestyStartButton.IsEnabled = true;
            });
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}