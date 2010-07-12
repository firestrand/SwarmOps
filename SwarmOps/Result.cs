/// ------------------------------------------------------
/// SwarmOps - Numeric and heuristic optimization for C#
/// Copyright (C) 2003-2009 Magnus Erik Hvass Pedersen.
/// Published under the GNU Lesser General Public License.
/// Please see the file license.txt for license details.
/// SwarmOps on the internet: http://www.Hvass-Labs.org/
/// ------------------------------------------------------

namespace SwarmOps
{
    /// <summary>
    /// Best solution found during optimization.
    /// </summary>
    public class Result
    {
        #region Constructors.
        /// <summary>
        /// Construct the object.
        /// </summary>
        /// <param name="parameters">Best found solution parameters.</param>
        /// <param name="fitness">Fitness for best found solution.</param>
        /// <param name="iterations">Number of iterations used.</param>
        public Result(double[] parameters, double fitness, int iterations)
        {
            Parameters = parameters.Clone() as double[];
            Fitness = fitness;
            Iterations = iterations;
        }
        #endregion

        #region Public fields.
        /// <summary>
        /// Best found solution parameters.
        /// </summary>
        public double[] Parameters
        {
            get;
            protected set;
        }

        /// <summary>
        /// Fitness associated with best found solution.
        /// </summary>
        public double Fitness
        {
            get;
            protected set;
        }

        /// <summary>
        /// Number of iterations (i.e. fitness evaluations) it took
        /// to achieve these results.
        /// </summary>
        public double Iterations
        {
            get;
            protected set;
        }
        #endregion
    }
}
