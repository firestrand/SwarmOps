using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using SwarmOps;
using SwarmOps.Optimizers;
using SwarmOps.Problems;

namespace MiniBenchmark
{
    /// <summary>
    /// Test an optimizer on mini benchmark problems.
    /// </summary>
    class Program
    {
        // Create optimizer object.
        //static SPSO Optimizer = new SPSO();
        static Optimizer Optimizer = new DE();
        // Control parameters for optimizer.
        private static readonly double[] Parameters = Optimizer.DefaultParameters;
        

        // Optimization settings.
        const int NumRuns = 100;
        //static IRunCondition RunCondition = new RunConditionFitness(NumIterations);
        static readonly StringBuilder _resultSb = new StringBuilder();

        /// <summary>
        /// Optimize the given problem and output result-statistics.
        /// </summary>
        static void Optimize(Problem problem)
        {

            // Assign the problem etc. to the optimizer.
            Optimizer.Problem = problem;
            Optimizer.RunCondition = problem.RunCondition;

            // Wrap it again in a repeater.
            Repeat Repeat = new RepeatCount(Optimizer, NumRuns);

            // Perform the optimization runs.
            double successPercent = Repeat.Fitness(Parameters);
            //double successPercent = Repeat.Fitness(Optimizer.CalculateParameters(Optimizer.Problem.Dimensionality, 3));

            // Output result-statistics.
            Console.WriteLine("{0} & {1} \\\\",
                problem.Name,
                Tools.FormatNumber(successPercent));


            //Add to result summary
            _resultSb.AppendLine(String.Format("{0} & {1} \\\\",
                                               problem.Name,
                                               Tools.FormatNumber(successPercent)));
        }
        static void Main(string[] args)
        {
            //Create list of problems
            var problems = new List<Problem>();
            problems.Add(new Tripod());
            problems.Add(new RosenbrockF6());
            problems.Add(new GearTrain());

            // Initialize PRNG.
            Globals.Random = new RandomOps.RanSystem();

            // Output optimization settings.
            Console.WriteLine("Mini-Benchmark-tests.");
            Console.WriteLine("Optimizer: {0}", Optimizer.Name);
            Console.WriteLine("Number of runs per problem: {0}", NumRuns);
            Console.WriteLine();
            Console.WriteLine("Problem & Success % \\\\");
            _resultSb.AppendLine("Problem & Success % \\\\");
            Console.WriteLine("\\hline");

            // Starting-time.
            var swTimer = new Stopwatch();
            swTimer.Start();
            foreach (var problem in problems)
            {
                problem.RunCondition = new RunConditionFitness(problem.Iterations, problem.AcceptableFitness);
                Optimize(problem);

            }

            // End-time.
            swTimer.Stop();
            _resultSb.AppendLine(String.Format("Total Benchmark Run Time: {0}", swTimer.Elapsed));
            //Write out summary
            File.WriteAllText(Optimizer.Name + "ResultSummary.txt", _resultSb.ToString());

            // Output time-usage.
            Console.WriteLine();
            Console.WriteLine("Time usage: {0}", swTimer.Elapsed);
            Console.WriteLine("Press Enter to Exit");
            Console.ReadLine();
        }
    }
}
