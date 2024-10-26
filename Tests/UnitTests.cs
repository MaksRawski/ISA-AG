using Microsoft.VisualStudio.TestTools.UnitTesting;
using ISA;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Linq.Dynamic.Core.Parser;

namespace ISA.Tests
{
    [TestClass()]
    public class UtilsTests
    {
        [DataTestMethod]
        [DataRow(new double[] { 0.1, 0.3, 0.6, 1.0 }, 0.2, 1)]
        [DataRow(new double[] { 0.1, 0.3, 0.6, 1.0 }, 0.5, 2)]
        [DataRow(new double[] { 0.1, 0.3, 0.6, 1.0 }, 0.8, 3)]
        [DataRow(new double[] { 0.1, 0.3, 0.6, 1.0 }, 0.1, 0)]
        [DataRow(new double[] { 0.1, 0.3, 0.6, 1.0 }, 0.3, 1)]
        [DataRow(new double[] { 0.1, 0.3, 0.6, 1.0 }, 0.6, 2)]
        [DataRow(new double[] { 0.1, 0.3, 0.6, 1.0 }, 1.0, 3)]
        public void GetCDFIndexTest(double[] qsArray, double r, int expectedIndex)
        {
            List<double> qs = new List<double>(qsArray);
            int result = Utils.GetCDFIndex(r, qs);

            // Assert
            Assert.AreEqual(
                expectedIndex,
                result,
                $"Test failed for r={r}, qs=[{string.Join(", ", qsArray)}]. Expected index: {expectedIndex}, but got: {result}."
            );
        }
    }

    [TestClass()]
    public class FunctionParserTests { 
        [DataTestMethod]
        [DataRow("(x + 2) * 2", 3, 10.0)]
        [DataRow("x + 2 * 2", 3, 7.0)]
        [DataRow("(x - 3) / 2", 6, 1.5)]
        [DataRow("x^3", -2, -8.0)]
        [DataRow("sin(x)", Math.PI/2.0, 1.0)]
        [DataRow("cos(x)", 0, 1.0)]
        [DataRow("-x", 1, -1.0)]
        [DataRow("-(x+1)", 1, -2.0)]
        [DataRow("(x + 2) * ", 123, null)]
        [DataRow("", 123, null)]
        public void ParseFunctionTest(string expression, double arg, double? expectedValue)
        {
            if (expectedValue is null){
                bool parsedSuccesfully = FunctionParser.TryParse(expression, out var f);
                Assert.AreEqual(false, parsedSuccesfully); 

            } else
            {
                var f = FunctionParser.Parse(expression);
                Assert.AreEqual(f!(arg), expectedValue);
            }
        }
    }
}