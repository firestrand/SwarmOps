/// ------------------------------------------------------
/// SwarmOps - Numeric and heuristic optimization for C#
/// Copyright (C) 2003-2009 Magnus Erik Hvass Pedersen.
/// Published under the GNU Lesser General Public License.
/// Please see the file license.txt for license details.
/// SwarmOps on the internet: http://www.Hvass-Labs.org/
/// ------------------------------------------------------

using System;
using SwarmOps;
using SwarmOps.Optimizers;

namespace TestCustomProblem
{
    /// <summary>
    /// Test an optimizer on a custom problem.
    /// </summary>
    class Program
    {
        // Create an object of the custom problem.
        static Problem Problem = new CustomProblem();

        // Optimization settings.
        static readonly int NumRuns = 50;
        static readonly int DimFactor = 200;
        static readonly int Dim = Problem.Dimensionality;
        static readonly int NumIterations = DimFactor * Dim;

        static IRunCondition RunCondition = new RunConditionIterations(NumIterations);
        //static IRunCondition RunCondition = new RunConditionFitness(NumIterations, Problem.AcceptableFitness);

        // Create optimizer object.
        static Optimizer Optimizer = new LUS(Problem);

        // Control parameters for optimizer.
        static readonly double[] Parameters = Optimizer.DefaultParameters;

        // Wrap the optimizer in a logger of result-statistics.
        static Statistics Statistics = new Statistics(Optimizer);

        // Wrap it again in a repeater.
        static Repeat Repeat = new RepeatSum(Statistics, NumRuns);

        static void Main(string[] args)
        {
            // Initialize PRNG.
            Globals.Random = new RandomOps.MersenneTwister();

            // Output optimization settings.
            Console.WriteLine("Optimizer: {0}", Optimizer.Name);
            Console.WriteLine("Using following parameters:");
            Tools.PrintParameters(Optimizer, Parameters);
            Console.WriteLine("Number of optimization runs: {0}", NumRuns);
            Console.WriteLine("Problem: {0}", Problem.Name);
            Console.WriteLine("\tDimensionality: {0}", Dim);
            Console.WriteLine("\tNumIterations per run, max: {0}", NumIterations);
            Console.WriteLine();

            // Create a fitness trace for tracing the progress of optimization.
            int NumMeanIntervals = 3000;
            FitnessTrace fitnessTrace = new FitnessTraceMean(NumIterations, NumMeanIntervals);

            // Assign the runcondition to the problem.
            Problem.RunCondition = RunCondition;

            // Assign the fitness trace to the optimizer.
            Optimizer.FitnessTrace = fitnessTrace;

            // Start-time.
            DateTime t1 = DateTime.Now;

            // Perform optimizations.
            double fitness = Repeat.Fitness(Parameters);

            // End-time.
            DateTime t2 = DateTime.Now;

            // Compute result-statistics.
            Statistics.Compute();

            // Output best result, as well as result-statistics.
            Console.WriteLine("Best solution found:");
            Tools.PrintParameters(Problem, Statistics.BestParameters);
            Console.WriteLine();
            Console.WriteLine("Fitness Results:");
            Console.WriteLine("\tBest: \t\t{0}", Tools.FormatNumber(Statistics.FitnessMin));
            Console.WriteLine("\tWorst: \t\t{0}", Tools.FormatNumber(Statistics.FitnessMax));
            Console.WriteLine("\tMean: \t\t{0}", Tools.FormatNumber(Statistics.FitnessMean));
            Console.WriteLine("\tStd.Dev.: \t{0}", Tools.FormatNumber(Statistics.FitnessStdDev));
            Console.WriteLine();
            Console.WriteLine("Iterations used per run:");
            Console.WriteLine("\tMean: {0}", Tools.FormatNumber(Statistics.IterationsMean));

            // Output time-usage.
            Console.WriteLine();
            Console.WriteLine("Time usage: {0}", t2 - t1);

            // Output fitness trace.
            string traceFilename = "FitnessTrace-" + Problem.Name + ".txt";
            fitnessTrace.WriteToFile(traceFilename);
        }
    }
}
