using Microsoft.VisualStudio.TestTools.UnitTesting;
using ISA;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace ISA.Tests
{
    [TestClass()]
    public class MainWindowTests
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
}