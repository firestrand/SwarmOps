/// ------------------------------------------------------
/// SwarmOps - Numeric and heuristic optimization for C#
/// Copyright (C) 2003-2009 Magnus Erik Hvass Pedersen.
/// Published under the GNU Lesser General Public License.
/// Please see the file license.txt for license details.
/// SwarmOps on the internet: http://www.Hvass-Labs.org/
/// ------------------------------------------------------

using System.Collections.Generic;
using System.Linq;
using SwarmOps.Extensions;

namespace SwarmOps
{
    /// <summary>
    /// Wrapper for an optimizer providing statistics such as
    /// mean fitness achieved over a number of optimization runs,
    /// best results achieved, etc. Transparently supports the
    /// same methods as the the optimizer itself, but stores the
    /// optimization results so as to compute the statistics.
    /// </summary>
    public class Statistics : Optimizer
    {
        #region Constructors.
        /// <summary>
        /// Create a Statistics-object.
        /// </summary>
        /// <param name="optimizer">Optimizer-object being wrapped.</param>
        public Statistics(Optimizer optimizer)
            : base()
        {
            Optimizer = optimizer;
            Results = new List<Result>();
        }
        #endregion

        #region Public fields.
        /// <summary>
        /// The optimizer that is being wrapped.
        /// </summary>
        public Optimizer Optimizer
        {
            get;
            private set;
        }

        /// <summary>
        /// Optimization results stored for later computation of statistics.
        /// </summary>
        public List<Result> Results
        {
            get;
            private set;
        }

        /// <summary>
        /// Best optimization results. There may be several, equally good results.
        /// To get the first call BestResult instead.
        /// </summary>
        public IEnumerable<Result> BestResults
        {
            get;
            private set;
        }

        /// <summary>
        /// Best optimization result achieved.
        /// </summary>
        public Result BestResult
        {
            get
            {
                IEnumerator<Result> results = BestResults.GetEnumerator();
                results.MoveNext();
                return results.Current;
            }
        }

        /// <summary>
        /// Parameters for best optimization result achieved.
        /// </summary>
        public double[] BestParameters
        {
            get
            {
                return BestResult.Parameters;
            }
        }

        /// <summary>
        /// Quartiles for fitness results.
        /// </summary>
        public Quartiles FitnessQuartiles
        {
            get;
            private set;
        }

        /// <summary>
        /// Fitness for best solution found.
        /// </summary>
        public double FitnessMin
        {
            get { return FitnessQuartiles.Min; }
        }

        /// <summary>
        /// Fitness for worst solution found.
        /// </summary>
        public double FitnessMax
        {
            get { return FitnessQuartiles.Max; }
        }

        /// <summary>
        /// Fitness mean or average for all optimization results.
        /// </summary>
        public double FitnessMean
        {
            get;
            private set;
        }

        /// <summary>
        /// Standard deviation of fitness for all optimization results.
        /// </summary>
        public double FitnessStdDev
        {
            get;
            private set;
        }

        /// <summary>
        /// Quartiles for iterations results.
        /// </summary>
        public Quartiles IterationsQuartiles
        {
            get;
            private set;
        }

        /// <summary>
        /// Lowest number of iterations used in a single optimization run.
        /// </summary>
        public double IterationsMin
        {
            get { return IterationsQuartiles.Min; }
        }

        /// <summary>
        /// Highest number of iterations used in a single optimization run.
        /// </summary>
        public double IterationsMax
        {
            get { return IterationsQuartiles.Max; }
        }

        /// <summary>
        /// Mean number of iterations used in optimization runs.
        /// </summary>
        public double IterationsMean
        {
            get;
            private set;
        }

        /// <summary>
        /// Standard deviation for the number of iterations used in optimization runs.
        /// </summary>
        public double IterationsStdDev
        {
            get;
            private set;
        }
        #endregion

        #region Public methods.
        /// <summary>
        /// Compute the statistics. Call this after all
        /// optimization runs have executed.
        /// </summary>
        public void Compute()
        {

            // Fitness quartiles.
            double[] fitnessArray = Results.Select(o => o.Fitness).ToArray();
            FitnessQuartiles = new Quartiles();
            FitnessQuartiles.ComputeUnsortedInplace(fitnessArray);

            // Fitness mean and stddev.
            FitnessMean = Results.Average(o => o.Fitness);
            FitnessStdDev = Results.StdDev(o => o.Fitness);

            // Iterations quartiles.
            double[] iterationsArray = Results.Select(o => o.Iterations).ToArray();
            IterationsQuartiles = new Quartiles();
            IterationsQuartiles.ComputeUnsortedInplace(iterationsArray);

            // Iterations mean and stddev.
            IterationsMean = Results.Average(o => o.Iterations);
            IterationsStdDev = Results.StdDev(o => o.Iterations);

            // Best results.
            BestResults = Results.Where(o => o.Fitness == FitnessMin);
        }

        /// <summary>
        /// Clear the stored data used for computing statistics.
        /// </summary>
        public void Clear()
        {
            Results.Clear();
        }
        #endregion

        #region Problem base-class overrides.
        /// <summary>
        /// Used for determining whether or not to continue optimization.
        /// </summary>
        public override IRunCondition RunCondition
        {
            get { return Optimizer.RunCondition; }
            set { Optimizer.RunCondition = value; }
        }

        /// <summary>
        /// Return the name of the problem.
        /// </summary>
        public override string Name
        {
            get { return "Statistics (" + Optimizer.Name + ")"; }
        }

        /// <summary>
        /// Return LowerBound of Optimizer.
        /// </summary>
        public override double[] LowerBound
        {
            get { return Optimizer.LowerBound; }
        }

        /// <summary>
        /// Return UpperBound of Optimizer.
        /// </summary>
        public override double[] UpperBound
        {
            get { return Optimizer.UpperBound; }
        }

        /// <summary>
        /// Return LowerInit of Optimizer.
        /// </summary>
        public override double[] LowerInit
        {
            get { return Optimizer.LowerInit; }
        }

        /// <summary>
        /// Return UpperInit of Optimizer.
        /// </summary>
        public override double[] UpperInit
        {
            get { return Optimizer.UpperInit; }
        }

        /// <summary>
        /// Return Dimensionality of Optimizer.
        /// </summary>
        public override int Dimensionality
        {
            get { return Optimizer.Dimensionality; }
        }

        /// <summary>
        /// Return MinFitness of Optimizer.
        /// </summary>
        public override double MinFitness
        {
            get { return Optimizer.MinFitness; }
        }

        /// <summary>
        /// Return ParameterName of Optimizer.
        /// </summary>
        public override string[] ParameterName
        {
            get { return Optimizer.ParameterName; }
        }
        #endregion

        #region Optimizer base-class overrides.
        /// <summary>
        /// Return DefaultParameters of Optimizer.
        /// </summary>
        public override double[] DefaultParameters
        {
            get { return Optimizer.DefaultParameters; }
        }

        /// <summary>
        /// Perform one optimization run and return the best found solution.
        /// This just wraps around the Optimizer and stores the results for
        /// later computation of statistics.
        /// </summary>
        /// <param name="parameters">Control parameters for the optimizer.</param>
        /// <param name="fitnessLimit">Preemptive Fitness Limit</param>
        public override Result Optimize(double[] parameters, double fitnessLimit)
        {
            // Call through to the Optimizer.
            Result result = Optimizer.Optimize(parameters, fitnessLimit);

            // Store optimization results for later use by the Compute() method.
            Results.Add(result);

            // Return results.
            return result;
        }
        #endregion
    }
}
