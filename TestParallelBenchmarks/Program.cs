﻿/// ------------------------------------------------------
/// SwarmOps - Numeric and heuristic optimization for C#
/// Copyright (C) 2003-2010 Magnus Erik Hvass Pedersen.
/// Published under the GNU Lesser General Public License.
/// Please see the file license.txt for license details.
/// SwarmOps on the internet: http://www.Hvass-Labs.org/
/// ------------------------------------------------------

using System;
using System.Diagnostics;
using SwarmOps;
using SwarmOps.Optimizers;
using SwarmOps.Problems;

namespace TestParallelBenchmarks
{
    /// <summary>
    /// Test a parallel optimizer on various benchmark problems.
    /// This is essentially the same as TestBenchmarks only the
    /// QuarticNoise problem cannot be used because it is not
    /// thread-safe, unless a thread-safe PRNG is used.
    /// A simulation of a time-consuming problem SphereSleep is
    /// also included here.
    /// </summary>
    class Program
    {
        // Create optimizer object.
        static Optimizer Optimizer = new SwarmOps.Optimizers.Parallel.MOL();

        // Control parameters for optimizer.
        static readonly double[] Parameters = Optimizer.DefaultParameters;

        // Optimization settings.
        static readonly int NumRuns = 50;
        static readonly int Dim = 30;
        static readonly int DimFactor = 2000;
        static readonly int NumIterations = DimFactor * Dim;
        static readonly bool DisplaceOptimum = true;
        static IRunCondition RunCondition = new RunConditionIterations(NumIterations);

        /// <summary>
        /// Optimize the given problem and output result-statistics.
        /// </summary>
        static void Optimize(Problem problem)
        {
            // Create a fitness trace for tracing the progress of optimization, mean.
            int NumMeanIntervals = 3000;
            FitnessTrace fitnessTraceMean = new FitnessTraceMean(NumIterations, NumMeanIntervals);

            // Create a fitness trace for tracing the progress of optimization, quartiles.
            // Note that fitnessTraceMean is chained to this object by passing it to the
            // constructor, this causes both fitness traces to be used.
            int NumQuartileIntervals = 10;
            FitnessTrace fitnessTraceQuartiles = new FitnessTraceQuartiles(NumRuns, NumIterations, NumQuartileIntervals, fitnessTraceMean);

            // Assign the problem etc. to the optimizer.
            Optimizer.Problem = problem;
            Optimizer.RunCondition = RunCondition;
            Optimizer.FitnessTrace = fitnessTraceQuartiles;

            // Wrap the optimizer in a logger of result-statistics.
            Statistics Statistics = new Statistics(Optimizer);

            // Wrap it again in a repeater.
            Repeat Repeat = new RepeatSum(Statistics, NumRuns);

            // Perform the optimization runs.
            double fitness = Repeat.Fitness(Parameters);

            // Compute result-statistics.
            Statistics.Compute();

            // Output result-statistics.
            Console.WriteLine("{0} & {1} & {2} & {3} & {4} & {5} & {6} & {7} \\\\",
                problem.Name,
                Tools.FormatNumber(Statistics.FitnessMean),
                Tools.FormatNumber(Statistics.FitnessStdDev),
                Tools.FormatNumber(Statistics.FitnessQuartiles.Min),
                Tools.FormatNumber(Statistics.FitnessQuartiles.Q1),
                Tools.FormatNumber(Statistics.FitnessQuartiles.Median),
                Tools.FormatNumber(Statistics.FitnessQuartiles.Q3),
                Tools.FormatNumber(Statistics.FitnessQuartiles.Max));

            // Output fitness trace, mean.
            string traceFilenameMean = Optimizer.Name + "-FitnessTraceMean-" + problem.Name + ".txt";
            fitnessTraceMean.WriteToFile(traceFilenameMean);

            // Output fitness trace, quartiles.
            string traceFilenameQuartiles = Optimizer.Name + "-FitnessTraceQuartiles-" + problem.Name + ".txt";
            fitnessTraceQuartiles.WriteToFile(traceFilenameQuartiles);
        }

        static void Main(string[] args)
        {
            // Initialize PRNG.
            // If optimization problem doesn't use Globals.Random then it doesn't
            // have to be thread-safe.
            Globals.Random = new RandomOps.MersenneTwister();
            // Otherwise use a fast and thread-safe PRNG, like so:
            // Globals.Random = new RandomOps.ThreadSafe.CMWC4096();

            // Set max number of threads allowed.
            Globals.ParallelOptions.MaxDegreeOfParallelism = 8;

            // Output optimization settings.
            Console.WriteLine("Benchmark-tests. (Parallel)");
            Console.WriteLine("Optimizer: {0}", Optimizer.Name);
            Console.WriteLine("Using following parameters:");
            Tools.PrintParameters(Optimizer, Parameters);
            Console.WriteLine("Number of runs per problem: {0}", NumRuns);
            Console.WriteLine("Dimensionality: {0}", Dim);
            Console.WriteLine("Dim-factor: {0}", DimFactor);
            Console.WriteLine("Displace global optimum: {0}", (DisplaceOptimum) ? ("Yes") : ("No"));
            Console.WriteLine();
            Console.WriteLine("Problem & Mean & Std.Dev. & Min & Q1 & Median & Q3 & Max \\\\");
            Console.WriteLine("\\hline");

            // Starting-time.
            Stopwatch swTimer = new Stopwatch();
            swTimer.Start();
            // Simulates a time-consuming optimization problem.
            //Optimize(new SphereSleep(1, Dim, DisplaceOptimum, RunCondition));

            // Thread-safe benchmark problems.
            Optimize(new Ackley(Dim, DisplaceOptimum, RunCondition));
            Optimize(new Griewank(Dim, DisplaceOptimum, RunCondition));
            Optimize(new Penalized1(Dim, DisplaceOptimum, RunCondition));
            Optimize(new Penalized2(Dim, DisplaceOptimum, RunCondition));
            Optimize(new Rastrigin(Dim, DisplaceOptimum, RunCondition));
            Optimize(new Rosenbrock(Dim, DisplaceOptimum, RunCondition));
            Optimize(new Schwefel12(Dim, DisplaceOptimum, RunCondition));
            Optimize(new Schwefel221(Dim, DisplaceOptimum, RunCondition));
            Optimize(new Schwefel222(Dim, DisplaceOptimum, RunCondition));
            Optimize(new Sphere(Dim, DisplaceOptimum, RunCondition));
            Optimize(new Step(Dim, DisplaceOptimum, RunCondition));

            // Benchmark problem using Globals.Random (see note above.)
            //Optimize(new QuarticNoise(Dim, DisplaceOptimum, RunCondition));

            // End-time.
            swTimer.Stop();

            // Output time-usage.
            Console.WriteLine();
            Console.WriteLine("Time usage: {0}", swTimer.Elapsed);
            Console.WriteLine("Press Enter to Exit");
            Console.ReadLine();
        }
    }
}