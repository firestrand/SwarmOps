using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SwarmOps;
using SwarmOps.Optimizers;
using SwarmOps.Problems;

namespace SwarmOps_Test
{
    [TestClass]
    public class ProblemTests
    {
        [TestMethod]
        public void TestTripod()
        {
            // Initialize PRNG.
            Globals.Random = new RandomOps.MersenneTwister();

            Optimizer optimizer = new MTS();
            double[] parameters = optimizer.DefaultParameters;
            Problem problem = new GearTrain();
            int numIterations = 10000;
            IRunCondition runCondition = new RunConditionFitness(numIterations,problem.AcceptableFitness);
            problem.RunCondition = runCondition;
            optimizer.Problem = problem;
            Result result = optimizer.Optimize(parameters);
            Assert.IsTrue(result.Fitness <= problem.AcceptableFitness);
        }
        [TestMethod]
        public void TestRosenbrockF6()
        {
            // Initialize PRNG.
            Globals.Random = new RandomOps.MersenneTwister();

            Optimizer optimizer = new MTS();
            double[] parameters = optimizer.DefaultParameters;
            Problem problem = new RosenbrockF6();
            int numIterations = 100000;
            IRunCondition runCondition = new RunConditionFitness(numIterations, problem.AcceptableFitness);
            problem.RunCondition = runCondition;
            optimizer.Problem = problem;
            Result result = optimizer.Optimize(parameters);
            Assert.IsTrue(result.Fitness <= problem.AcceptableFitness);
        }
        [TestMethod]
        public void TestGearTrain()
        {
            // Initialize PRNG.
            Globals.Random = new RandomOps.MersenneTwister();

            Optimizer optimizer = new MTS();
            double[] parameters = optimizer.DefaultParameters;
            Problem problem = new GearTrain();
            int numIterations = 20000;
            IRunCondition runCondition = new RunConditionFitness(numIterations, problem.AcceptableFitness);
            problem.RunCondition = runCondition;
            optimizer.Problem = problem;
            Result result = optimizer.Optimize(parameters);
            Assert.IsTrue(result.Fitness <= problem.AcceptableFitness);
        }
        [TestMethod]
        public void TestRosenbrockF6GlobalMinimum()
        {
            RosenbrockF6 problem = new RosenbrockF6();
            double[]optimal = new double[problem.Dimensionality];
            for (int i = 0; i < problem.Dimensionality; i++)
            {
                optimal[i] = problem.Offset[i] + 1.0;
            }
            double expected = 390.0d;
            double result = problem.Fitness(optimal);
            Assert.AreEqual(expected,result);
        }
        [TestMethod]
        public void TestRosenbrockGlobalMinimum()
        {
            int dimensionality = 10;
            Problem problem = new Rosenbrock(dimensionality,false,null);
            double[] optimal = Enumerable.Repeat(1.0d, dimensionality).ToArray();
            double expected = 0.0d;
            double result = problem.Fitness(optimal);
            Assert.AreEqual(expected, result);
        }
        [TestMethod]
        public void TestGearTrainGlobalMinimum()
        {
            Problem problem = new GearTrain();
            double[] optimal = new[]{19.0,16.0,43.0,49.0};
            double expected = 2.7e-12d;
            double result = Math.Round(problem.Fitness(optimal),14);
            Assert.AreEqual(expected, result);
        }
    }
}
