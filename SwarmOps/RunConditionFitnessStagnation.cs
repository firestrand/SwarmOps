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
    /// Continue optimization until fitness is below the given
    /// threshold or a maximum number of iterations has been
    /// performed.
    /// </summary>
    public class RunConditionFitnessStagnation : RunConditionIterations
    {
        private int _fitnessIteration;
        private double _fitness;
        #region Constructors.
        /// <summary>
        /// Construct the object.
        /// </summary>
        /// <param name="maxIterations">
        /// Stop optimization once this number of iterations is used.
        /// </param>
        /// <param name="fitnessBelow">
        /// Stop optimization once this fitness-threshold is reached.
        /// </param>
        public RunConditionFitnessStagnation(int maxIterations, int maxStagnantCount, double fitnessBelow)
            : base(maxIterations)
        {
            _fitnessIteration = -1;
            _fitness = double.MaxValue;

            MaxStagnantCount = maxStagnantCount;
            FitnessBelow = fitnessBelow;
        }
        #endregion

        #region Public fields.

        public int MaxStagnantCount { get; protected set; }
        /// <summary>
        /// Fitness-threshold below which optimization can be stopped.
        /// </summary>
        public double FitnessBelow
        {
            get;
            protected set;
        }
        #endregion

        #region Base-class overrides.
        /// <summary>
        /// Return whether optimization is allowed to continue.
        /// </summary>
        /// <param name="iterations">Number of iterations performed in optimization run.</param>
        /// <param name="fitness">Best fitness found in optimization run.</param>
        public override bool Continue(int iterations, double fitness)
        {
            if(_fitnessIteration < 0 || _fitness > fitness)
            {
                _fitnessIteration = iterations;
                _fitness = fitness;
            }
            return (iterations-_fitnessIteration) < MaxStagnantCount &&(fitness > FitnessBelow) && (base.Continue(iterations, fitness));
        }
        #endregion
    }
}
