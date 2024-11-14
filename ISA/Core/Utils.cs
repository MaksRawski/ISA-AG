using org.mariuszgromada.math.mxparser;

namespace Core;

public struct UserInputs
{
    public bool elitism;
    public double a, b, d, pk, pm;
    public int decimalPlaces, l;
    public int N, T;
    public FunctionGoal functionGoal;
    public Func<double, double> f;
}

public enum FunctionGoal
{
    Max,
    Min,
}
public class TableRow
{
    public int Lp { get; set; }
    public double XReal { get; set; }
    public string? XBin { get; set; }
    public double Fx { get; set; }
    public double Percent { get; set; }
   
}

public class Utils
{
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
    private static double Gmax(Func<double, double> f, double x, double fMin, double d)
    {
        return f(x) - fMin + d;
    }
    private static double Gmin(Func<double, double> f, double x, double fMax, double d)
    {
        return -(f(x) - fMax) + d;
    }
    public static double G(Func<double, double> f, double x, FunctionGoal functionGoal, double fExtreme, double d)
    {
        return functionGoal == FunctionGoal.Max ?
            Gmax(f, x, fExtreme, d) : Gmin(f, x, fExtreme, d);
    }
    /// <summary>
    /// Finds the index of the smallest element in the sorted list 'qs' that is greater than or equal to the given value 'r' using Binary Search.
    /// </summary>
    /// <param name="r">The threshold value to compare against the elements in 'qs'.</param>
    /// <param name="qs">A sorted list of double values representing cumulative distribution function (CDF).</param>
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
    public static bool TryParseFunction(string expression, out Func<double, double>? f)
    {
        Argument xArg = new("x");
        Expression e = new(expression);
        e.addArguments(xArg);
        bool isFunctionValid = e.checkSyntax();

        f = null;
        if (isFunctionValid)
        {
            f = (x) =>
            {
                e.setArgumentValue("x", x);
                return e.calculate();
            };
        }

        return isFunctionValid;
    }
    public static Func<double, double> ParseFunction(string expression)
    {
        Argument xArg = new("x");
        Expression e = new(expression);
        e.addArguments(xArg);

        return (x) =>
        {
            e.setArgumentValue("x", x);
            return e.calculate();
        };
    }
}