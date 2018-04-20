/// ------------------------------------------------------
/// SwarmOps - Numeric and heuristic optimization for C#
/// Copyright (C) 2003-2009 Magnus Erik Hvass Pedersen.
/// Published under the GNU Lesser General Public License.
/// Please see the file license.txt for license details.
/// SwarmOps on the internet: http://www.Hvass-Labs.org/
/// ------------------------------------------------------

using System;
using System.IO;
using System.Text;
using SwarmOps;
using SwarmOps.Optimizers;
using SwarmOps.Problems;
using System.Diagnostics;

namespace TestBenchmarks
{
    /// <summary>
    /// Test an optimizer on various benchmark problems.
    /// </summary>
    class Program
    {
        // Create optimizer object.
        static Optimizer _optimizer = new PSO();
        // Control parameters for optimizer.
        private static readonly double[] Parameters = _optimizer.DefaultParameters;
        
        //static readonly double[] Parameters = MOL.Parameters.HandTuned;

        // Optimization settings.
        static readonly int NumRuns = 10;
        static readonly int Dim = 1000;
        static readonly long DimFactor = 2000;
        private static readonly long NumIterations = 50000; //DimFactor* Dim; //Really the number of function evaluations
        static readonly bool DisplaceOptimum = true;
        static IRunCondition RunCondition = new RunConditionIterations(NumIterations);
        static StringBuilder _resultSb = new StringBuilder();

        /// <summary>
        /// Optimize the given problem and output result-statistics.
        /// </summary>
        static void Optimize(Problem problem)
        {
            Parameters[0] = 200;
            // Create a fitness trace for tracing the progress of optimization, mean.
            int NumMeanIntervals = 3000;
            FitnessTrace fitnessTraceMean = new FitnessTraceMean(NumIterations, NumMeanIntervals);

            // Create a fitness trace for tracing the progress of optimization, quartiles.
            // Note that fitnessTraceMean is chained to this object by passing it to the
            // constructor, this causes both fitness traces to be used.
            int NumQuartileIntervals = 10;
            FitnessTrace fitnessTraceQuartiles = new FitnessTraceQuartiles(NumRuns, NumIterations, NumQuartileIntervals, fitnessTraceMean);

            // Assign the problem etc. to the optimizer.
            _optimizer.Problem = problem;
            _optimizer.RunCondition = RunCondition;
            _optimizer.FitnessTrace = fitnessTraceQuartiles;

            // Wrap the optimizer in a logger of result-statistics.
            Statistics Statistics = new Statistics(_optimizer);

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
            //string traceFilenameMean = _optimizer.Name + "-FitnessTraceMean-" + problem.Name + ".txt";
            //fitnessTraceMean.WriteToFile(traceFilenameMean);

            // Output fitness trace, quartiles.
            //string traceFilenameQuartiles = _optimizer.Name + "-FitnessTraceQuartiles-" + problem.Name + ".txt";
            //fitnessTraceQuartiles.WriteToFile(traceFilenameQuartiles);

            //Add to result summary
            _resultSb.AppendLine(String.Format("{0}\t{1}\t{2}\t{3}\t{4}\t{5}\t{6}\t{7}",
                                               problem.Name,
                                               Tools.FormatNumber(Statistics.FitnessMean),
                                               Tools.FormatNumber(Statistics.FitnessStdDev),
                                               Tools.FormatNumber(Statistics.FitnessQuartiles.Min),
                                               Tools.FormatNumber(Statistics.FitnessQuartiles.Q1),
                                               Tools.FormatNumber(Statistics.FitnessQuartiles.Median),
                                               Tools.FormatNumber(Statistics.FitnessQuartiles.Q3),
                                               Tools.FormatNumber(Statistics.FitnessQuartiles.Max)));
        }
        static void Main(string[] args)
        {
            // Initialize PRNG.
            Globals.Random = new RandomOps.RanSystem();

            // Output optimization settings.
            Console.WriteLine("Benchmark-tests.");
            Console.WriteLine("_optimizer: {0}", _optimizer.Name);
            Console.WriteLine("Using following parameters:");
            Tools.PrintParameters(_optimizer, Parameters);
            Console.WriteLine("Number of runs per problem: {0}", NumRuns);
            Console.WriteLine("Dimensionality: {0}", Dim);
            Console.WriteLine("Dim-factor: {0}", DimFactor);
            Console.WriteLine("Displace global optimum: {0}", (DisplaceOptimum) ? ("Yes") : ("No"));
            Console.WriteLine();
            Console.WriteLine("Problem & Mean & Std.Dev. & Min & Q1 & Median & Q3 & Max \\\\");
            _resultSb.AppendLine("Problem\tMean\tStd.Dev.\tMin\tQ1\tMedian\tQ3\tMax");
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
            _resultSb.AppendLine(String.Format("Total Benchmark Run Time: {0}", swTimer.Elapsed));
            //Write out summary
            File.WriteAllText(_optimizer.Name + "ResultSummary_" + Dim +".txt",_resultSb.ToString());

            // Output time-usage.
            Console.WriteLine();
            Console.WriteLine("Time usage: {0}", swTimer.Elapsed);
            Console.WriteLine("Press Enter to Exit");
            Console.ReadLine();
        }
    }
}
