/// ------------------------------------------------------
/// SwarmOps - Numeric and heuristic optimization for C#
/// Copyright (C) 2003-2009 Magnus Erik Hvass Pedersen.
/// Published under the GNU Lesser General Public License.
/// Please see the file license.txt for license details.
/// SwarmOps on the internet: http://www.Hvass-Labs.org/
/// ------------------------------------------------------

using System.Diagnostics;

namespace SwarmOps.Optimizers
{
    /// <summary>
    /// Local Unimodal Sampling (LUS) optimizer originally
    /// due to Pedersen (1). Does local sampling with an
    /// exponential decrease of the sampling-range. Works
    /// well for many optimization problems, especially
    /// when only short runs are allowed. Is particularly
    /// well suited as the overlaying meta-optimizer when
    /// tuning parameters for another optimizer.
    /// </summary>
    /// <remarks>
    /// References:
    /// (1) M.E.H. Pedersen. Simplifying Swarm Optimization.
    ///     PhD Thesis, University of Southampton, 2009.
    /// </remarks>
    public class LUS : Optimizer
    {
        #region Constructors.
        /// <summary>
        /// Construct the object.
        /// </summary>
        public LUS()
            : base()
        {
        }

        /// <summary>
        /// Construct the object.
        /// </summary>
        /// <param name="problem">Problem to optimize.</param>
        public LUS(Problem problem)
            : base(problem)
        {
        }
        #endregion

        #region Get control parameters.
        /// <summary>
        /// Get parameter, Gamma.
        /// </summary>
        /// <param name="parameters">Optimizer parameters.</param>
        public double GetGamma(double[] parameters)
        {
            return parameters[0];
        }
        #endregion

        #region Base-class overrides, Problem.
        /// <summary>
        /// Name of the optimizer.
        /// </summary>
        public override string Name
        {
            get { return "LUS"; }
        }

        /// <summary>
        /// Number of control parameters for optimizer.
        /// </summary>
        public override int Dimensionality
        {
            get { return 1; }
        }

        string[] _parameterName = { "Gamma" };

        /// <summary>
        /// Control parameter names.
        /// </summary>
        public override string[] ParameterName
        {
            get { return _parameterName ; }
        }

        static readonly double[] _defaultParameters = { 3.0 };

        /// <summary>
        /// Default control parameters.
        /// </summary>
        public override double[] DefaultParameters
        {
            get { return _defaultParameters; }
        }

        static readonly double[] _lowerBound = { 0.5 };

        /// <summary>
        /// Lower search-space boundary for control parameters.
        /// </summary>
        public override double[] LowerBound
        {
            get { return _lowerBound; }
        }

        static readonly double[] _upperBound = { 100.0 };

        /// <summary>
        /// Upper search-space boundary for control parameters.
        /// </summary>
        public override double[] UpperBound
        {
            get { return _upperBound; }
        }
        #endregion

        #region Base-class overrides, Optimizer.
        /// <summary>
        /// Perform one optimization run and return the best found solution.
        /// </summary>
        /// <param name="parameters">Control parameters for the optimizer.</param>
        public override Result Optimize(double[] parameters)
        {
            Debug.Assert(parameters != null && parameters.Length == Dimensionality);

            // Retrieve parameter specific to LUS method.
            double gamma = GetGamma(parameters);

            // Get problem-context.
            double[] lowerBound = Problem.LowerBound;
            double[] upperBound = Problem.UpperBound;
            double[] lowerInit = Problem.LowerInit;
            double[] upperInit = Problem.UpperInit;
            int n = Problem.Dimensionality;

            // Allocate agent position and search-range vectors.
            double[] x = new double[n];      // Current position.
            double[] y = new double[n];      // Potentially new position.
            double[] d = new double[n];      // Search-range.

            // Initialize search-range and decrease-factor.
            double r = 1;								            	// Search-range.
            double q = System.Math.Pow(2.0, -1.0 / (n * gamma));		// Decrease-factor (using gamma = 1.0/alpha).

            // Fitness variables.
            double fitness, newFitness;

            // Initialize agent-position in search-space.
            Tools.InitializeUniform(ref x, lowerBound, upperBound);

            // Initialize search-range to full search-space.
            Tools.InitializeRange(ref d, lowerBound, upperBound);

            // Compute fitness of initial position.
            // This counts as an iteration below.
            fitness = Problem.Fitness(x);

            // Trace fitness of best found solution.
            Trace(0, fitness);

            int i;
            for (i = 1; Problem.RunCondition.Continue(i, fitness); i++)
            {
                // Compute potentially new position.
                for (int j = 0; j < n; j++)
                {
                    // Pick a sample from the neighbourhood of the current
                    // position and within the given range.
                    y[j] = Tools.SampleBounded(x[j], r * d[j], lowerBound[j], upperBound[j]);
                }

                // Compute new fitness.
                newFitness = Problem.Fitness(y, fitness);

                // Update position and fitness in case of strict improvement.
                if (newFitness < fitness)
                {
                    // Update fitness.
                    fitness = newFitness;

                    // Update position by swapping array x and y.
                    double[] temp = x;
                    x = y;
                    y = temp;
                }
                else
                {
                    // Decrease the search-range.
                    r *= q;
                }

                // Trace fitness of best found solution.
                Trace(i, fitness);
            }

            // Return best-found solution and fitness.
            return new Result(x, fitness, i);
        }
        #endregion
    }
}