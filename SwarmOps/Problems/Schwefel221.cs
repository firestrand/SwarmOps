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
    /// Schwefel 2-21 benchmark problem.
    /// </summary>
    public class Schwefel221 : Benchmark
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
        public Schwefel221(int dimensionality, bool displaceOptimum, IRunCondition runCondition)
            : base(dimensionality, -100, 100, 50, 100, -25, displaceOptimum, runCondition)
        {
        }
        #endregion

        #region Base-class overrides.
        /// <summary>
        /// Name of the optimization problem.
        /// </summary>
        public override string Name
        {
            get { return "Schwefel2-21"; }
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

            double maxValue = maxValue = GetElm(x[0]);

            for (int i = 1; i < Dimensionality; i++)
            {
                double elm = GetElm(x[i]);

                if (elm > maxValue)
                {
                    maxValue = elm;
                }
            }

            return maxValue;
        }
        #endregion

        #region Protected methods.
        /// <summary>
        /// Return the absolute of the parameter.
        /// </summary>
        protected double GetElm(double x)
        {
            double elm = Displace(x);
            double absElm = System.Math.Abs(elm);

            return absElm;
        }
        #endregion
    }
}
