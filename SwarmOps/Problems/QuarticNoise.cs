/// ------------------------------------------------------
/// SwarmOps - Numeric and heuristic optimization for C#
/// Copyright (C) 2003-2009 Magnus Erik Hvass Pedersen.
/// Published under the GNU Lesser General Public License.
/// Please see the file license.txt for license details.
/// SwarmOps on the internet: http://www.Hvass-Labs.org/
/// ------------------------------------------------------

using System.Diagnostics;

namespace SwarmOps.Problems
{
    /// <summary>
    /// Quartic Noise benchmark problem.
    /// </summary>
    public class QuarticNoise : Benchmark
    {
        #region Constructors.
        /// <summary>
        /// Construct the object.
        /// </summary>
        /// <param name="dimensionality">Dimensionality of the problem (e.g. 20)</param>
        /// <param name="displaceOptimum">Displace optimum?</param>
        /// <param name="runCondition">
        /// Determines for how long to continue optimization.
        /// </param>
        public QuarticNoise(int dimensionality, bool displaceOptimum, IRunCondition runCondition)
            : base(dimensionality, -1.28, 1.28, 0.64, 1.28, -0.32, displaceOptimum, runCondition)
        {
        }
        #endregion

        #region Base-class overrides.
        /// <summary>
        /// Name of the optimization problem.
        /// </summary>
        public override string Name
        {
            get { return "QuarticNoise"; }
        }

        /// <summary>
        /// Minimum possible fitness.
        /// </summary>
        public override double MinFitness
        {
            get { return 0; }
        }

        /// <summary>
        /// Compute and return fitness for the given parameters.
        /// </summary>
        /// <param name="x">Candidate solution.</param>
        public override double Fitness(double[] x)
        {
            Debug.Assert(x != null && x.Length == Dimensionality);

            double value = 0;

            for (int i = 0; i < Dimensionality; i++)
            {
                double elm = Displace(x[i]);
                double elm2 = elm * elm;
                double elm4 = elm2 * elm2;

                value += ((double)(i + 1)) * elm4 + Globals.Random.Uniform();
            }

            return value;
        }
        #endregion
    }
}
