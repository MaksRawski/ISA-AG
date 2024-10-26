using System.Linq.Expressions;

namespace ISA;
public static class FunctionParser
{
    public static Func<double, double> Parse(string expression)
    {
        ParameterExpression param = Expression.Parameter(typeof(double), "x");

        var lambda = System.Linq.Dynamic.Core.DynamicExpressionParser
            .ParseLambda(new[] { param }, typeof(double), expression);

        var f = (Func<double, double>)lambda.Compile();
        
        return f;
    }
}
