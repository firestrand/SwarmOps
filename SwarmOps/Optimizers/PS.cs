/// ------------------------------------------------------
/// SwarmOps - Numeric and heuristic optimization for C#
/// Copyright (C) 2003-2009 Magnus Erik Hvass Pedersen.
/// Published under the GNU Lesser General Public License.
/// Please see the file license.txt for license details.
/// SwarmOps on the internet: http://www.Hvass-Labs.org/
/// ------------------------------------------------------

namespace SwarmOps.Optimizers
{
    /// <summary>
    /// Pattern Search (PS), an early variant was originally
    /// due to Fermi and Metropolis at the Los Alamos nuclear
    /// laboratory, as described by Davidon (1). It is also
    /// sometimes called compass search. This is a slightly
    /// different variant by Pedersen (2). It works for a
    /// wide variety of optimization problems, especially
    /// when only few iterations are allowed. It does, however,
    /// stagnate rather quickly.
    /// </summary>
    /// <remarks>
    /// References:
    /// (1) W.C. Davidon. Variable metric method for minimization.
    ///     SIAM Journal on Optimization, 1(1):1{17, 1991
    /// (2) M.E.H. Pedersen. Simplifying Swarm Optimization.
    ///     PhD Thesis, University of Southampton, 2009.
    /// </remarks>
    public class PS : Optimizer
    {
        #region Constructors.
        /// <summary>
        /// Construct the object.
        /// </summary>
        public PS()
            : base()
        {
        }

        /// <summary>
        /// Construct the object.
        /// </summary>
        /// <param name="problem">Problem to optimize.</param>
        public PS(Problem problem)
            : base(problem)
        {
        }
        #endregion

        #region Base-class overrides, Problem.
        /// <summary>
        /// Name of the optimizer.
        /// </summary>
        public override string Name
        {
            get { return "PS"; }
        }

        /// <summary>
        /// Number of control parameters for optimizer.
        /// </summary>
        public override int Dimensionality
        {
            get { return 0; }
        }

        /// <summary>
        /// Control parameter names.
        /// </summary>
        public override string[] ParameterName
        {
            get { return null; }
        }

        /// <summary>
        /// Default control parameters.
        /// </summary>
        public override double[] DefaultParameters
        {
            get { return null; }
        }

        /// <summary>
        /// Lower search-space boundary for control parameters.
        /// </summary>
        public override double[] LowerBound
        {
            get { return null; }
        }

        /// <summary>
        /// Upper search-space boundary for control parameters.
        /// </summary>
        public override double[] UpperBound
        {
            get { return null; }
        }
        #endregion

        #region Base-class override, Optimizer.
        /// <summary>
        /// Perform one optimization run and return the best found solution.
        /// </summary>
        /// <param name="parameters">Control parameters for the optimizer.</param>
        public override Result Optimize(double[] parameters)
        {
            // Get problem-context.
            double[] lowerBound = Problem.LowerBound;
            double[] upperBound = Problem.UpperBound;
            double[] lowerInit = Problem.LowerInit;
            double[] upperInit = Problem.UpperInit;
            int n = Problem.Dimensionality;

            // Allocate agent position and search-range vectors.
            double[] x = new double[n];      // Current position.
            double[] d = new double[n];      // Search-range.

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
                // Pick random dimension.
                int R = Globals.Random.Index(n);

                // Store old value for that dimension.
                double t = x[R];

                // Compute new value for that dimension.
                x[R] += d[R];

                // Enforce bounds before computing new fitness.
                x[R] = Tools.Bound(x[R], lowerBound[R], upperBound[R]);

                // Compute new fitness.
                newFitness = Problem.Fitness(x, fitness);

                // If improvement to fitness, keep new position.
                if (newFitness < fitness)
                {
                    // Store fitness.
                    fitness = newFitness;
                }
                else
                {
                    // Restore position.
                    x[R] = t;

                    // Reduce and invert search-range.
                    d[R] *= -0.5;
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
