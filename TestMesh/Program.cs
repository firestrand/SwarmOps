/// ------------------------------------------------------
/// SwarmOps - Numeric and heuristic optimization for C#
/// Copyright (C) 2003-2009 Magnus Erik Hvass Pedersen.
/// Published under the GNU Lesser General Public License.
/// Please see the file license.txt for license details.
/// SwarmOps on the internet: http://www.Hvass-Labs.org/
/// ------------------------------------------------------

using System;
using System.Collections.Generic;

using SwarmOps;
using SwarmOps.Problems;
using SwarmOps.Optimizers;

namespace TestMesh
{
    /// <summary>
    /// Similar to TestMetaBenchmark, only we here use the MetaFitness
    /// class for simplicity, instead of using a combination of the
    /// Repeat- and Multi-classes. This is much simpler to setup and is
    /// also closer to how meta-optimization is described in the research
    /// papers.
    /// </summary>
    class Program
    {
        // Mesh settings.
        static readonly int MeshNumIterationsPerDim = 40;

        // Settings for the optimization layer.
        static readonly int NumRuns = 50;
        static readonly int Dim = 30;
        static readonly int DimFactor = 2000;
        static readonly int NumIterations = DimFactor * Dim;
        static readonly bool DisplaceOptimum = true;

        // The optimizer whose control paramters are to be tuned.
        static Optimizer Optimizer = new MOL();
        //static Optimizer Optimizer = new DE();

        // Problems to optimize. That is, the optimizer is having its control
        // parameters tuned to work well on these problems.
        static Problem[] Problems =
            new Problem[]
            {
                //new Ackley(Dim, DisplaceOptimum, new RunConditionIterations(NumIterations)),
                //new Griewank(Dim, DisplaceOptimum, new RunConditionIterations(NumIterations)),
                //new Penalized1(Dim, DisplaceOptimum, new RunConditionIterations(NumIterations)),
                //new Penalized2(Dim, DisplaceOptimum, new RunConditionIterations(NumIterations)),
                //new QuarticNoise(Dim, DisplaceOptimum, new RunConditionIterations(NumIterations)),
                new Rastrigin(Dim, DisplaceOptimum, new RunConditionIterations(NumIterations)),
                //new Rosenbrock(Dim, DisplaceOptimum, new RunConditionIterations(NumIterations)),
                new Schwefel12(Dim, DisplaceOptimum, new RunConditionIterations(NumIterations)),
                //new Schwefel221(Dim, DisplaceOptimum, new RunConditionIterations(NumIterations)),
                //new Schwefel222(Dim, DisplaceOptimum, new RunConditionIterations(NumIterations)),
                //new Sphere(Dim, DisplaceOptimum, new RunConditionIterations(NumIterations)),
                //new Step(Dim, DisplaceOptimum, new RunConditionIterations(NumIterations)),
            };

        // The meta-fitness consists of computing optimization performance
        // for the problems listed above over several optimization runs and
        // sum the results, so we wrap the Optimizer-object in a
        // MetaFitness-object which takes of this.
        static MetaFitness MetaFitness = new MetaFitness(Optimizer, Problems, NumRuns);

        // Log all candidate solutions.
        static int LogCapacity = 20;
        static LogSolutions LogSolutions = new LogSolutions(MetaFitness, LogCapacity);

        // The meta-optimizer.
        static Optimizer MetaOptimizer = new MESH(LogSolutions);

        // Control parameters to use for the meta-optimizer.
        static double[] MetaParameters = { MeshNumIterationsPerDim };

        // Wrap the meta-optimizer in a Statistics object for logging results.
        static Statistics Statistics = new Statistics(MetaOptimizer);

        static void Main(string[] args)
        {
            // Initialize the PRNG.
            Globals.Random = new RandomOps.MersenneTwister();

            // Output settings.
            Console.WriteLine("Mesh of meta-fitness values using benchmark problems.");
            Console.WriteLine();
            Console.WriteLine("Optimizer to compute mesh for: {0}", Optimizer.Name);
            Console.WriteLine("Mesh, number of iterations per dimension: {0}", MeshNumIterationsPerDim);
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
            Console.WriteLine("Mesh of meta-fitness values:");
            Console.WriteLine();

            // Start-time.
            DateTime t1 = DateTime.Now;

            // Perform the meta-optimization runs.
            double fitness = Statistics.Fitness(MetaParameters);

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
        }
    }
}
