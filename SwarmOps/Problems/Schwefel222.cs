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
    /// Schwefel 2-22 benchmark problem.
    /// </summary>
    public class Schwefel222 : Benchmark
    {
        /// <summary>
        /// Construct the object.
        /// </summary>
        /// <param name="dimensionality">Dimensionality of the problem (e.g. 20)</param>
        /// <param name="displaceOptimum">Displace optimum?</param>
        /// <param name="runCondition">
        /// Determines for how long to continue optimization.
        /// </param>
        public Schwefel222(int dimensionality, bool displaceOptimum, IRunCondition runCondition)
            : base(dimensionality, -10, 10, 5, 10, -2.5, displaceOptimum, runCondition)
        {
        }

        #region Base-class overrides.
        /// <summary>
        /// Name of the optimization problem.
        /// </summary>
        public override string Name
        {
            get { return "Schwefel2-22"; }
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

            double sum = 0;
            double product = 1;

            for (int i = 0; i < Dimensionality; i++)
            {
                double elm = Displace(x[i]);
                double absElm = System.Math.Abs(elm);

                sum += absElm;
                product *= absElm;
            }

            return sum + product;
        }
        #endregion
    }
}
