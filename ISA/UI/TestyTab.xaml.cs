using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Core;
using System.IO;
using OpenTK.Audio.OpenAL.Extensions.Creative.EFX;

namespace UI
{
    public partial class TestyTab : UserControl, INotifyPropertyChanged
    {
        private double progressPercentage;
        private readonly ExperimentsRunner experimentsRunner;
        private readonly string logFilePath = "TestyLogs.txt"; 
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
        }

        private async void Start(object sender, RoutedEventArgs e)
        {
            TestyStartButton.IsEnabled = false;
            using StreamWriter writer = new(logFilePath, append: true);
            writer.WriteLine("N,T,pk,pm,fx");

            var progress = new Progress<(ExperimentParameterSet, double)>(result =>
            {
                var (parameterSet, fx) = result;
                CompletedExperiments++;
                ProgressPercentage = (double)CompletedExperiments / TotalExperiments * 100;

                string logEntry = $"{parameterSet.N},{parameterSet.T},{parameterSet.pk},{parameterSet.pm},{fx}";
                writer.WriteLine(logEntry);
            });

            await experimentsRunner.Run(progress);
            TestyStartButton.IsEnabled = true;
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}