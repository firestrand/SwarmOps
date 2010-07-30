/// ------------------------------------------------------
/// SwarmOps - Numeric and heuristic optimization for C#
/// Copyright (C) 2003-2009 Magnus Erik Hvass Pedersen.
/// Published under the GNU Lesser General Public License.
/// Please see the file license.txt for license details.
/// SwarmOps on the internet: http://www.Hvass-Labs.org/
/// ------------------------------------------------------

using System;

using SwarmOps;
using SwarmOps.Problems;
using SwarmOps.Optimizers;

namespace TestMetaBenchmarks
{
    /// <summary>
    /// Test meta-optimization, that is, tuning of control parameters
    /// for an optimizer by applying an additional layer of optimization.
    /// See TestParallelMetaBenchmarks for the parallel version of this.
    /// </summary>
    class Program
    {
        // Settings for the optimization layer.
        static readonly int NumRuns = 64;
        static readonly int Dim = 30;
        static readonly int DimFactor = 2000;
        static readonly int NumIterations = DimFactor * Dim;
        static readonly bool DisplaceOptimum = true;

        // The optimizer whose control paramters are to be tuned.
        static Optimizer Optimizer = new DE();

        // Problems to optimize. That is, the optimizer is having its control
        // parameters tuned to work well on these problems. The numbers are weights
        // that significy mutual importance of the problems in tuning. Higher weight
        // means more importance.
        static WeightedProblem[] WeightedProblems =
            new WeightedProblem[]
            {
                new WeightedProblem(1.0, new Ackley(Dim, DisplaceOptimum, new RunConditionIterations(NumIterations))),
                new WeightedProblem(1.0, new Griewank(Dim, DisplaceOptimum, new RunConditionIterations(NumIterations))),
                new WeightedProblem(1.0, new Penalized1(Dim, DisplaceOptimum, new RunConditionIterations(NumIterations))),
                new WeightedProblem(1.0, new Penalized2(Dim, DisplaceOptimum, new RunConditionIterations(NumIterations))),
                //new WeightedProblem(1.0, new QuarticNoise(Dim, DisplaceOptimum, new RunConditionIterations(NumIterations))),
                new WeightedProblem(1.0, new Rastrigin(Dim, DisplaceOptimum, new RunConditionIterations(NumIterations))),
                new WeightedProblem(1.0, new Rosenbrock(Dim, DisplaceOptimum, new RunConditionIterations(NumIterations))),
                new WeightedProblem(1.0, new Schwefel12(Dim, DisplaceOptimum, new RunConditionIterations(NumIterations))),
                new WeightedProblem(1.0, new Schwefel221(Dim, DisplaceOptimum, new RunConditionIterations(NumIterations))),
                new WeightedProblem(1.0, new Schwefel222(Dim, DisplaceOptimum, new RunConditionIterations(NumIterations))),
                new WeightedProblem(1.0, new Sphere(Dim, DisplaceOptimum, new RunConditionIterations(NumIterations))),
                new WeightedProblem(1.0, new Step(Dim, DisplaceOptimum, new RunConditionIterations(NumIterations))),
            };

        // Settings for the meta-optimization layer.
        static readonly int MetaNumRuns = 5;
        static readonly int MetaDim = Optimizer.Dimensionality;
        static readonly int MetaDimFactor = 20;
        static readonly int MetaNumIterations = MetaDimFactor * MetaDim;

        // Continue meta-optimization for a certain number of iterations.
        static IRunCondition RunCondition = new RunConditionIterations(MetaNumIterations);

        // The meta-fitness consists of computing optimization performance
        // for the problems listed above over several optimization runs and
        // sum the results, so we wrap the Optimizer-object in a
        // MetaFitness-object which takes of this.
        static MetaFitness MetaFitness = new MetaFitness(Optimizer, WeightedProblems, NumRuns);

        // Log all candidate solutions.
        static int LogCapacity = 20;
        static LogSolutions LogSolutions = new LogSolutions(MetaFitness, LogCapacity);

        // The meta-optimizer.
        static Optimizer MetaOptimizer = new LUS(LogSolutions);

        // Control parameters to use for the meta-optimizer.
        static double[] MetaParameters = MetaOptimizer.DefaultParameters;

        // If using DE as meta-optimizer, use these control parameters.
        //static double[] MetaParameters = DE.Parameters.ForMetaOptimization;

        // Wrap the meta-optimizer in a Statistics object for logging results.
        static Statistics Statistics = new Statistics(MetaOptimizer);

        // Repeat a number of meta-optimization runs.
        static Repeat MetaRepeat = new RepeatMin(Statistics, MetaNumRuns);

        static void Main(string[] args)
        {
            // Initialize the PRNG.
            Globals.Random = new RandomOps.MersenneTwister();

            // Create a fitness trace for tracing the progress of optimization.
            int MaxMeanIntervals = 3000;
            FitnessTrace fitnessTrace = new FitnessTraceMean(MetaNumIterations, MaxMeanIntervals);

            // Assign the RunCondition to the optimizer.
            Optimizer.RunCondition = RunCondition;

            // Assign the fitness trace to the meta-optimizer.
            MetaOptimizer.FitnessTrace = fitnessTrace;

            // Output settings.
            Console.WriteLine("Meta-Optimization of benchmark problems.");
            Console.WriteLine();
            Console.WriteLine("Meta-method: {0}", MetaOptimizer.Name);
            Console.WriteLine("Using following parameters:");
            Tools.PrintParameters(MetaOptimizer, MetaParameters);
            Console.WriteLine("Number of meta-runs: {0}", MetaNumRuns);
            Console.WriteLine("Number of meta-iterations: {0}", MetaNumIterations);
            Console.WriteLine();
            Console.WriteLine("Method to be meta-optimized: {0}", Optimizer.Name);
            Console.WriteLine("Number of benchmark problems: {0}", WeightedProblems.Length);

            for (int i = 0; i < WeightedProblems.Length; i++)
            {
                Problem problem = WeightedProblems[i].Problem;
                double weight = WeightedProblems[i].Weight;

                Console.WriteLine("\t({0})\t{1}", weight, problem.Name);
            }

            Console.WriteLine("Dimensionality for each benchmark problem: {0}", Dim);
            Console.WriteLine("Number of runs per benchmark problem: {0}", NumRuns);
            Console.WriteLine("Number of iterations per run: {0}", NumIterations);
            Console.WriteLine("Displace global optimum: {0}", (DisplaceOptimum) ? ("Yes") : ("No"));
            Console.WriteLine();

            Console.WriteLine("*** Indicates a meta-fitness evaluation is an improvement.");
            Console.WriteLine();

            // Start-time.
            DateTime t1 = DateTime.Now;

            // Perform the meta-optimization runs.
            double fitness = MetaRepeat.Fitness(MetaParameters);

            // End-time.
            DateTime t2 = DateTime.Now;

            // Compute result-statistics.
            Statistics.Compute();

            // Retrieve best-found control parameters for the optimizer.
            double[] bestParameters = Statistics.BestResult.Parameters;

            // Output results and statistics.
            Console.WriteLine();
            Console.WriteLine("Best found parameters for {0} optimizer:", Optimizer.Name);
            Tools.PrintParameters(Optimizer, bestParameters);
            Console.WriteLine("Parameters written in array notation:");
            Console.WriteLine("\t{0}", Tools.ArrayToString(bestParameters, 4));
            Console.WriteLine("Best parameters have meta-fitness: {0}", Tools.FormatNumber(Statistics.FitnessMin));
            Console.WriteLine("Worst meta-fitness: {0}", Tools.FormatNumber(Statistics.FitnessMax));
            Console.WriteLine("Mean meta-fitness: {0}", Tools.FormatNumber(Statistics.FitnessMean));
            Console.WriteLine("StdDev for meta-fitness: {0}", Tools.FormatNumber(Statistics.FitnessStdDev));

            // Output best found parameters.
            Console.WriteLine();
            Console.WriteLine("Best {0} found parameters:", LogSolutions.Capacity);
            foreach (Solution candidateSolution in LogSolutions.Log)
            {
                Console.WriteLine("\t{0}\t{1}",
                    Tools.ArrayToStringRaw(candidateSolution.Parameters, 4),
                    Tools.FormatNumber(candidateSolution.Fitness));
            }

            // Output time-usage.
            Console.WriteLine();
            Console.WriteLine("Time usage: {0}", t2 - t1);

            // Output fitness trace.
            string traceFilename
                = "MetaFitnessTrace-" + MetaOptimizer.Name + "-" + Optimizer.Name
                + "-" + WeightedProblems.Length + "Bnch" + "-" + DimFactor + "xDim.txt";
            fitnessTrace.WriteToFile(traceFilename);

            //Console.WriteLine("Press any key to exit ...");
            //Console.ReadKey();
        }
    }
}
