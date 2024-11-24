using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Core;

class Program
{
    static async Task Main(string[] args)
    {
        List<int> Ns = new() { 30, 35, 40, 45, 50, 55, 60, 65, 70, 75, 80 };
        List<double> pks = new() { 0.5, 0.55, 0.6, 0.65, 0.7, 0.75, 0.8, 0.85, 0.9 };
        List<double> pms = new() { 0.0001, 0.0005, 0.001, 0.005, 0.01 };
        List<int> Ts = new() { 50, 60, 70, 80, 90, 100 };

        Console.WriteLine("Starting...");
        var experimentsRunner = new ExperimentsRunner(Ns, pks, pms, Ts);

        string date = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
        string logFilePath = $"experiments_log_{date}.csv";
        using StreamWriter writer = new(logFilePath, append: true);
        writer.WriteLine("N,T,pk,pm,fx");
        writer.AutoFlush = true;

        await experimentsRunner.Run(new Progress<(ExperimentParameterSet, double)>(results =>
        {
            var (parameterSet, fx) = results;
            string logEntry = $"{parameterSet.N},{parameterSet.T},{parameterSet.pk},{parameterSet.pm},{fx}";
            writer.WriteLine(logEntry);
        }));
        Console.Beep();
        Console.WriteLine("Done!");
    }
}
