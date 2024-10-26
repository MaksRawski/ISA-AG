using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text.RegularExpressions;
namespace ISA;
public static class FunctionParser
{
    private static readonly Dictionary<string, Func<Expression, Expression, Expression>> Operators = new()
    {
        { "+", Expression.Add }, { "-", Expression.Subtract },
        { "*", Expression.Multiply }, { "/", Expression.Divide },
        { "^", (a, b) => Expression.Call(typeof(Math).GetMethod("Pow")!, a, b) }
    };

    private static readonly Dictionary<string, Func<Expression, Expression>> Functions = new()
    {
        { "sqrt", arg => Expression.Call(typeof(Math).GetMethod("Sqrt")!, arg) },
        { "abs", arg => Expression.Call(typeof(Math).GetMethod("Abs")!, arg) },
        { "round", arg => Expression.Call(typeof(Math).GetMethod("Round")!, arg) }, // NOTE: rounds midpoint values towards even numbers
        { "sin", arg => Expression.Call(typeof(Math).GetMethod("Sin")!, arg) },
        { "cos", arg => Expression.Call(typeof(Math).GetMethod("Cos")!, arg) },
        { "log", arg => Expression.Call(typeof(Math).GetMethod("Log")!, arg) },
        { "log2", arg => Expression.Call(typeof(Math).GetMethod("Log2")!, arg) },
        { "log10", arg => Expression.Call(typeof(Math).GetMethod("Log10")!, arg) }
    };
    public static bool TryParse(string expression, out Func<double, double>? f)
    {
        f = null;
        try
        {
            f = Parse(expression);
        }
        catch
        {
            return false;
        }
        return true;
    }
    //public static Func<double, double> Parse(string expression)
    //{
    //    // Define the parameter x
    //    var parameter = Expression.Parameter(typeof(double), "x");

    //    // Parse the expression string into an Expression
    //    var body = ParseExpression(expression, parameter);

    //    // Create the lambda expression
    //    return Expression.Lambda<Func<double, double>>(body, parameter).Compile();
    //}
    public static Func<double, double> Parse(string expression)
    {
        ParameterExpression param = Expression.Parameter(typeof(double), "x");

        var lambda = System.Linq.Dynamic.Core.DynamicExpressionParser
            .ParseLambda(new[] { param }, typeof(double), expression);

        var f = (Func<double, double>)lambda.Compile();
        return f;
    }

    private static Expression ParseExpression(string expression, ParameterExpression parameter)
    {
        // Handle sqrt function
        if (expression.StartsWith("sqrt(") && expression.EndsWith(")"))
        {
            var innerExpression = expression.Substring(5, expression.Length - 6);
            var inner = ParseExpression(innerExpression, parameter);
            return Expression.Call(typeof(Math).GetMethod("Sqrt", new[] { typeof(double) })!, inner);
        }

        // Handle exponentiation
        int index = FindOperatorIndex(expression, "^");
        if (index != -1)
        {
            var baseExpr = ParseExpression(expression.Substring(0, index), parameter);
            var exponentExpr = ParseExpression(expression.Substring(index + 1), parameter);
            return Expression.Power(baseExpr, exponentExpr);
        }

        // Handle subtraction (for simplicity, assuming it appears in the form of "x-2" and not "-x")
        index = FindOperatorIndex(expression, "-");
        if (index != -1)
        {
            var left = ParseExpression(expression.Substring(0, index), parameter);
            var right = ParseExpression(expression.Substring(index + 1), parameter);
            return Expression.Subtract(left, right);
        }

        // Parse variable "x" or constant number
        if (expression == "x")
            return parameter;

        if (double.TryParse(expression, out double constantValue))
            return Expression.Constant(constantValue);

        throw new ArgumentException("Invalid expression format.");
    }

    // Helper method to find the index of an operator, respecting parentheses
    private static int FindOperatorIndex(string expression, string op)
    {
        int parenthesesCount = 0;
        for (int i = 0; i < expression.Length; i++)
        {
            if (expression[i] == '(') parenthesesCount++;
            else if (expression[i] == ')') parenthesesCount--;
            else if (parenthesesCount == 0 && expression.Substring(i, op.Length) == op)
                return i;
        }
        return -1;
    }
}
