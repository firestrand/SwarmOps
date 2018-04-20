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
    /// Continue optimization until a maximum number of
    /// iterations has been performed.
    /// </summary>
    public class RunConditionIterations : IRunCondition
    {
        #region Constructors.
        /// <summary>
        /// Create the object.
        /// </summary>
        /// <param name="maxIterations">
        /// Stop optimization once this number of iterations is used.
        /// </param>
        public RunConditionIterations(long maxIterations)
        {
            MaxIterations = maxIterations;
        }
        #endregion

        #region Public fields.
        /// <summary>
        /// Maximum number of optimization iterations to perform.
        /// </summary>
        public long MaxIterations
        {
            get;
            protected set;
        }
        #endregion

        #region Interface implementation.
        /// <summary>
        /// Return whether optimization is allowed to continue.
        /// </summary>
        /// <param name="iterations">Number of iterations performed in optimization run.</param>
        /// <param name="fitness">Best fitness found in optimization run.</param>
        public virtual bool Continue(int iterations, double fitness)
        {
            return iterations < MaxIterations;
        }
        #endregion
    }
}
