using NUnit.Framework;
using AndroidSample.Core;
using System.Collections.Generic;

namespace Tests
{
    public class Test_MVC
    {
        MainModel model;

        [SetUp]
        public void Setup()
        {
            model = MainModel.Instance;
        }

        [Test]
        public void Calculate_mvc_correct_single()
        {
            double[][] s = new double[][] { new double[] { 1, 1 } };

            List<double> mvc = model.Calculate_MVC(s);
            List <double> expected = new List<double>() { 1 };
            Assert.AreEqual(mvc, expected);
        }
        [Test]
        public void Calculate_mvc_correct_single_difficult()
        {
            double[][] s = new double[][] { new double[] {0, 1, 0 } };

            List<double> mvc = model.Calculate_MVC(s);
            List<double> expected = new List<double>() { 0.577 };
            Assert.AreEqual(mvc[0], expected[0], 0.05);
        }

        [Test]
        public void Calculate_mvc_correct_two_sensors()
        {
            double[][] s = new double[][] { new double[] { 2,2,2,2 }, new double[] { 2, 2,2 ,2 } };

            List<double> mvc = model.Calculate_MVC(s);
            List<double> expected = new List<double>() { 2, 2 };

            Assert.AreEqual(mvc, expected);
        }
    }
}