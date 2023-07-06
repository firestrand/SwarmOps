/// ------------------------------------------------------
/// SwarmOps - Numeric and heuristic optimization for C#
/// Copyright (C) 2003-2011 Magnus Erik Hvass Pedersen.
/// Please see the file license.txt for license details.
/// SwarmOps on the internet: http://www.Hvass-Labs.org/
/// ------------------------------------------------------

using System;
using SwarmOps;
using SwarmOps.Problems;
using SwarmOps.Optimizers;

namespace TestParallelMetaBenchmarks
{
    /// <summary>
    /// Similar to TestMetaBenchmark only using the parallel version
    /// of MetaFitness and a thread-safe PRNG. Search-space mangler
    /// is not used because it is incompatible with parallel MetaFitness.
    /// </summary>
    class Program
    {
        // Settings for the optimization layer.
        static readonly int NumRuns = 64;       // Set this close to 50 and a multiple of the number of processors, e.g. 8.
        static readonly int Dim = 5;
        static readonly int DimFactor = 2000;
        static readonly int NumIterations = DimFactor * Dim;

        // The optimizer whose control paramters are to be tuned.
        static Optimizer _optimizer = new MOL();

        // Problems to optimize. That is, the optimizer is having its control
        // parameters tuned to work well on these problems. The numbers are weights
        // that significy mutual importance of the problems in tuning. Higher weight
        // means more importance.
        static WeightedProblem[] _weightedProblems =
            new WeightedProblem[]
            {
                //new WeightedProblem(1.0, new Ackley(Dim, NumIterations)),
                //new WeightedProblem(1.0, new Griewank(Dim, NumIterations)),
                new WeightedProblem(1.0, new Penalized1(Dim, NumIterations)),
                //new WeightedProblem(1.0, new Penalized2(Dim, NumIterations)),
                //new WeightedProblem(1.0, new QuarticNoise(Dim, NumIterations)),
                //new WeightedProblem(1.0, new Rastrigin(Dim, NumIterations)),
                new WeightedProblem(1.0, new Rosenbrock(Dim, NumIterations)),
                new WeightedProblem(1.0, new Schwefel12(Dim, NumIterations)),
                //new WeightedProblem(1.0, new Schwefel221(Dim, NumIterations)),
                //new WeightedProblem(1.0, new Schwefel222(Dim, NumIterations)),
                new WeightedProblem(1.0, new Sphere(Dim, NumIterations)),
                //new WeightedProblem(1.0, new Step(Dim, NumIterations)),
            };

        // Settings for the meta-optimization layer.
        static readonly int MetaNumRuns = 5;
        static readonly int MetaDim = _optimizer.Dimensionality;
        static readonly int MetaDimFactor = 20;
        static readonly int MetaNumIterations = MetaDimFactor * MetaDim;

        // The meta-fitness consists of computing optimization performance
        // for the problems listed above over several optimization runs and
        // sum the results, so we wrap the Optimizer-object in a
        // MetaFitness-object which takes of this.
        static SwarmOps.Optimizers.Parallel.MetaFitness _metaFitness = new SwarmOps.Optimizers.Parallel.MetaFitness(_optimizer, _weightedProblems, NumRuns, MetaNumIterations);

        // Print meta-optimization progress.
        static FitnessPrint _metaFitnessPrint = new FitnessPrint(_metaFitness);

        // Log all candidate solutions.
        static int _logCapacity = 20;
        static bool _logOnlyFeasible = true;
        static LogSolutions _logSolutions = new LogSolutions(_metaFitnessPrint, _logCapacity, _logOnlyFeasible);

        // The meta-optimizer.
        static Optimizer _metaOptimizer = new LUS(_logSolutions);

        // Control parameters to use for the meta-optimizer.
        static double[] _metaParameters = _metaOptimizer.DefaultParameters;

        // If using DE as meta-optimizer, use these control parameters.
        //static double[] MetaParameters = DE.Parameters.ForMetaOptimization;

        // Wrap the meta-optimizer in a Statistics object for logging results.
        static readonly bool StatisticsOnlyFeasible = true;
        static Statistics _statistics = new Statistics(_metaOptimizer, StatisticsOnlyFeasible);

        // Repeat a number of meta-optimization runs.
        static Repeat _metaRepeat = new RepeatMin(_statistics, MetaNumRuns);

        static void Main(string[] args)
        {
            // Initialize the PRNG.
            // Parallel version uses a ThreadSafe PRNG.
            Globals.Random = new RandomOps.Cmwc4096();

            // Set max number of threads allowed.
            Globals.ParallelOptions.MaxDegreeOfParallelism = 8;

            // Create a fitness trace for tracing the progress of meta-optimization.
            int maxMeanIntervals = 3000;
            FitnessTrace fitnessTrace = new FitnessTraceMean(MetaNumIterations, maxMeanIntervals);
            FeasibleTrace feasibleTrace = new FeasibleTrace(MetaNumIterations, maxMeanIntervals, fitnessTrace);

            // Assign the fitness trace to the meta-optimizer.
            _metaOptimizer.FitnessTrace = feasibleTrace;

            // Output settings.
            Console.WriteLine("Meta-Optimization of benchmark problems. (Parallel)");
            Console.WriteLine();
            Console.WriteLine("Meta-method: {0}", _metaOptimizer.Name);
            Console.WriteLine("Using following parameters:");
            Tools.PrintParameters(_metaOptimizer, _metaParameters);
            Console.WriteLine("Number of meta-runs: {0}", MetaNumRuns);
            Console.WriteLine("Number of meta-iterations: {0}", MetaNumIterations);
            Console.WriteLine();
            Console.WriteLine("Method to be meta-optimized: {0}", _optimizer.Name);
            Console.WriteLine("Number of benchmark problems: {0}", _weightedProblems.Length);

            for (int i = 0; i < _weightedProblems.Length; i++)
            {
                Problem problem = _weightedProblems[i].Problem;
                double weight = _weightedProblems[i].Weight;

                Console.WriteLine("\t({0})\t{1}", weight, problem.Name);
            }

            Console.WriteLine("Dimensionality for each benchmark problem: {0}", Dim);
            Console.WriteLine("Number of runs per benchmark problem: {0}", NumRuns);
            Console.WriteLine("Number of iterations per run: {0}", NumIterations);
            Console.WriteLine("(Search-space mangling not supported for parallel meta-optimization.)");
            Console.WriteLine();

            Console.WriteLine("0/1 Boolean whether optimizer's control parameters are feasible.");
            Console.WriteLine("*** Indicates meta-fitness/feasibility is an improvement.");

            // Start-time.
            DateTime t1 = DateTime.Now;

            // Perform the meta-optimization runs.
            _metaRepeat.Fitness(_metaParameters);

            // End-time.
            DateTime t2 = DateTime.Now;

            // Compute result-statistics.
            _statistics.Compute();

            // Retrieve best-found control parameters for the optimizer.
            double[] bestParameters = _statistics.BestResult.Parameters;

            // Output results and statistics.
            Console.WriteLine();
            Console.WriteLine("Best found parameters for {0} optimizer:", _optimizer.Name);
            Tools.PrintParameters(_optimizer, bestParameters);
            Console.WriteLine("Parameters written in array notation:");
            Console.WriteLine("\t{0}", Tools.ArrayToString(bestParameters, 4));
            Console.WriteLine("Best parameters have meta-fitness: {0}", Tools.FormatNumber(_statistics.FitnessMin));
            Console.WriteLine("Worst meta-fitness: {0}", Tools.FormatNumber(_statistics.FitnessMax));
            Console.WriteLine("Mean meta-fitness: {0}", Tools.FormatNumber(_statistics.FitnessMean));
            Console.WriteLine("StdDev for meta-fitness: {0}", Tools.FormatNumber(_statistics.FitnessStdDev));

            // Output best found parameters.
            Console.WriteLine();
            Console.WriteLine("Best {0} found parameters:", _logSolutions.Capacity);
            foreach (Solution candidateSolution in _logSolutions.Log)
            {
                Console.WriteLine("\t{0}\t{1}\t{2}",
                    Tools.ArrayToStringRaw(candidateSolution.Parameters, 4),
                    Tools.FormatNumber(candidateSolution.Fitness),
                    (candidateSolution.Feasible) ? (1) : (0));
            }

            // Output time-usage.
            Console.WriteLine();
            Console.WriteLine("Time usage: {0}", t2 - t1);

            // Output fitness and feasible trace.
            string traceFilename
                = _metaOptimizer.Name + "-" + _optimizer.Name
                + "-" + _weightedProblems.Length + "Bnch" + "-" + DimFactor + "xDim.txt";
            fitnessTrace.WriteToFile("MetaFitnessTrace-" + traceFilename);
            feasibleTrace.WriteToFile("MetaFeasibleTrace-" + traceFilename);

            //Console.WriteLine("Press any key to exit ...");
            //Console.ReadKey();
        }
    }
}
