using org.mariuszgromada.math.mxparser;

namespace Core;

public struct UserInputs
{
    public GenomeSpace genomeSpace;
    public int N, T;
    public double pk, pm;
    public bool elitism;
    public Func<double, double> f;
    public FunctionGoal functionGoal;
}

// the following may seem like quite a monster but it's only just a struct
// with 5 attributes where 3 of them are nested in another struct
// so it has tons of comments to explain why it's like that

/// <summary>
/// The space of all the possible genomes described by an inclusive range 
/// [<see cref="a"/>, <see cref="b"/>] and a <see cref="Precision"/>.
/// </summary>
public readonly struct GenomeSpace
{
    /// <summary>
    /// Lower boundary of the space range (inclusive).
    /// </summary>
    public readonly double a;
    /// <summary>
    /// Upper boundary of the space range (inclusive).
    /// </summary>
    public readonly double b;

    /// <summary>
    /// Defines how detailed a value can be through
    /// <see cref="d"/> - the smallest decimal step size,
    /// <see cref="l"/> - information capacity as a number of bits and 
    /// <see cref="decimalPlaces"/> - decimal precision of the number.
    /// </summary>
    public readonly struct Precision
    {
        /// <summary>
        /// Smallest decimal step size e.g. 0.001.
        /// </summary>
        public readonly double d;
        /// <summary>
        /// Length of the binary representation of the genome.
        /// </summary>
        public readonly int l;
        /// <summary>
        /// Decimal places to be used for rounding the real number representation.
        /// </summary>
        public readonly int decimalPlaces;

        /// Internal constructor needed for the factory inside <see cref="GenomeSpace"/> to be working.
        internal Precision(double d, int l, int decimalPlaces)
        {
            this.d = d;
            this.l = l;
            this.decimalPlaces = decimalPlaces;
        }
    }

    /// <summary>
    /// An instance member holding the <see cref="Precision"/> structure.
    /// </summary>
    public readonly Precision precision;

    /// Private constructor needed for the entire <see cref="GenomeSpace"/> factory to be working.
    private GenomeSpace(double a, double b, Precision precision)
    {
        // TODO: check if with this parameters we can have rounding errors
        this.a = a;
        this.b = b;
        this.precision = precision;
    }

    // TODO: add checks whether either d or decimalPlaces is a sane value
    // it must be such that rounding errors don't yet occur at that precision

    /// <summary>
    /// Creates a 'GenomeSpace' instance using step size 'd'. 
    /// </summary>
    /// <param name="d">Step size e.g. 0.001.</param>
    /// <param name="a">The lower (inclusive) boundary of the range.</param>
    /// <param name="b">The upper (inclusive) boundary of the range.</param>
    /// <returns>A GenomeSpace instance with all the fields.</returns>
    public static GenomeSpace FromD(double d, double a, double b)
    {
        int decimalPlaces = (int)Math.Ceiling(-Math.Log10(d));
        int l = (int)Math.Ceiling(Math.Log2((b - a) / d + 1));
        Precision precision = new(d, l, decimalPlaces);

        return new GenomeSpace(a, b, precision);
    }

    /// <summary>
    /// Creates a 'GenomeSpace' instance using 'decimalPlaces'. 
    /// </summary>
    /// <param name="decimalPlaces">Decimal places to be used for rounding the real number representation.</param>
    /// <param name="a">The lower (inclusive) boundary of the range.</param>
    /// <param name="b">The upper (inclusive) boundary of the range.</param>
    /// <returns>A GenomeSpace instance with all the fields.</returns>
    public static GenomeSpace FromDecimalPlaces(int decimalPlaces, double a, double b)
    {
        double d = Math.Pow(10, -decimalPlaces);
        int l = (int)Math.Ceiling(Math.Log2((b - a) / d + 1));
        Precision precision = new(d, l, decimalPlaces);

        return new GenomeSpace(a, b, precision);
    }

    ///// <summary>
    ///// Creates a 'GenomeSpace' instance using genome length 'l'. 
    ///// </summary>
    ///// <param name="l">Genome length - number of bits the binary representation needs.</param>
    ///// <param name="a">The lower (inclusive) boundary of the range.</param>
    ///// <param name="b">The upper (inclusive) boundary of the range.</param>
    ///// <returns>A GenomeSpace instance with all the fields.</returns>
    //public static GenomeSpace FromL(int l, double a, double b)
    //{
    //    // l = ceil( log2((b-a)/d + 1) )
    //    double d = ??
    //    int decimalPlaces = (int)Math.Ceiling(-Math.Log10(d));
    //    Precision precision = new(d, l, decimalPlaces);
    //
    //    return new GenomeSpace(d, l, decimalPlaces);
    //}
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

    public static double Int2Real(int x, GenomeSpace space)
    {
        return x * (space.b - space.a) / (Math.Pow(2, space.precision.l) - 1) + space.a;
    }
    public static int Real2Int(double x, GenomeSpace space)
    {
        return (int)Math.Round( (x - space.a) / (space.b - space.a) * (Math.Pow(2, space.precision.l) - 1) );
    }
    /// <summary>
    /// Converts a Real representation of a genome to Binary within the <see cref="GenomeSpace"/> space.
    /// </summary>
    /// <param name="x">Real number representation of the genome.</param>
    /// <param name="space">A <see cref="GenomeSpace"/> within which to change the representation.</param>
    /// <returns>A real number representation of the genome.</returns>
    public static string Real2Bin(double x, GenomeSpace space)
    {
        return Int2Bin(Real2Int(x, space), space.precision.l);
    }
    /// <summary>
    /// Converts a Binary representation of a genome to a Real number within the <see cref="GenomeSpace"/> space.
    /// </summary>
    /// <param name="x">Binary representation of the genome.</param>
    /// <param name="space">A <see cref="GenomeSpace"/> within which to change the representation.</param>
    /// <returns>A real number representation of the genome.</returns>
    public static double Bin2Real(string x, GenomeSpace space)
    {
        return Math.Round(Int2Real(Bin2Int(x), space), space.precision.decimalPlaces);
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