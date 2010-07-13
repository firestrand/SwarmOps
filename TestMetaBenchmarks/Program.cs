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
    /// You may want to use TestMetaBenchmarks2 instead which also supports
    /// weights in meta-optimization.
    /// </summary>
    class Program
    {
        // Settings for the optimization layer.
        static readonly int NumRuns = 50;
        static readonly int Dim = 5;
        static readonly int DimFactor = 200;
        static readonly int NumIterations = DimFactor * Dim;
        static readonly bool DisplaceOptimum = false;

        // The optimizer whose control paramters are to be tuned.
        static Optimizer Optimizer = new DE();

        // Wrap the optimizer in a repeater to conduct several optimization runs.
        // This could be replaced with RepeatMin or another Repeat-object, so as
        // to change the type of meta-fitness being computed.
        static Repeat Repeat = new RepeatSum(Optimizer, NumRuns);

        // Problems to optimize. That is, the optimizer is having its control
        // parameters tuned to work well on these problems.
        static Problem[] Problems =
            new Problem[]
            {
                new Step(Dim, DisplaceOptimum, new RunConditionIterations(NumIterations)),
                new Sphere(Dim, DisplaceOptimum, new RunConditionIterations(NumIterations)),
                new Rosenbrock(Dim, DisplaceOptimum, new RunConditionIterations(NumIterations)),
                new Griewank(Dim, DisplaceOptimum, new RunConditionIterations(NumIterations)),
                new Rastrigin(Dim, DisplaceOptimum, new RunConditionIterations(NumIterations))
            };

        // The meta-fitness consists of computing optimization performance
        // for the problems listed above and sum the results, so we wrap
        // the Repeat-object in a Multi-object which takes of this summing.
        static Multi MetaFitness = new Multi(Problems, Repeat);

        // Settings for the meta-optimization layer.
        static readonly int MetaNumRuns = 5;
        static readonly int MetaDim = Optimizer.Dimensionality;
        static readonly int MetaDimFactor = 20;
        static readonly int MetaNumIterations = MetaDimFactor * MetaDim;

        // Continue meta-optimization for a certain number of iterations.
        static IRunCondition RunCondition = new RunConditionIterations(MetaNumIterations);

        // Wrap the MetaFitness in a fitness-printer so we can follow the progress.
        static readonly bool FormatAsArray = false;
        static FitnessPrint Printer = new FitnessPrint(MetaFitness, FormatAsArray);

        // Log all candidate solutions.
        static int LogCapacity = 20;
        static LogSolutions LogSolutions = new LogSolutions(Printer, LogCapacity);

        // The meta-optimizer.
        static Optimizer MetaOptimizer = new LUS(LogSolutions);

        // Wrap the meta-optimizer in a Statistics object for logging results.
        static Statistics Statistics = new Statistics(MetaOptimizer);
        
        // Repeat a number of meta-optimization runs.
        static Repeat MetaRepeat = new RepeatMin(Statistics, MetaNumRuns);

        static void Main(string[] args)
        {
            // Initialize the PRNG.
            Globals.Random = new RandomOps.MersenneTwister();

            // Create a fitness trace for tracing the progress of optimization.
            int NumMeanIntervals = 3000;
            FitnessTrace fitnessTrace = new FitnessTraceMean(MetaNumIterations, NumMeanIntervals);

            // Assign the fitness trace to the meta-optimizer.
            MetaOptimizer.FitnessTrace = fitnessTrace;

            // Assign the RunCondition to the optimizer.
            Optimizer.RunCondition = RunCondition;

            // Output settings.
            Console.WriteLine("Meta-Optimization of benchmark problems.");
            Console.WriteLine();
            Console.WriteLine("Meta-method: {0}", MetaOptimizer.Name);
            Console.WriteLine("Using following parameters:");
            Tools.PrintParameters(MetaOptimizer, MetaOptimizer.DefaultParameters);
            Console.WriteLine("Number of meta-runs: {0}", MetaNumRuns);
            Console.WriteLine("Number of meta-iterations: {0}", MetaNumIterations);
            Console.WriteLine();
            Console.WriteLine("Method to be meta-optimized: {0}", Optimizer.Name);
            Console.WriteLine("Number of benchmark problems: {0}", Problems.Length);

            for (int i = 0; i < Problems.Length; i++)
            {
                Console.WriteLine("\t{0}", Problems[i].Name);
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
            double fitness = MetaRepeat.Fitness();

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
            string traceFilename = "MetaFitnessTrace-" + MetaOptimizer.Name + "-" + Optimizer.Name + ".txt";
            fitnessTrace.WriteToFile(traceFilename);
            Console.WriteLine("Press Enter to Exit");
            Console.ReadLine();
        }
    }
}
