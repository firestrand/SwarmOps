/// ------------------------------------------------------
/// SwarmOps - Numeric and heuristic optimization for C#
/// Published under the GNU Lesser General Public License.
/// Please see the file license.txt for license details.
/// SwarmOps on the internet: http://www.Hvass-Labs.org/
/// ------------------------------------------------------

using System;
using System.Diagnostics;

namespace SwarmOps.Problems
{
    /// <summary>
    /// Tripod benchmark problem. This variant is from "A mini-benchmark" by Clerc
    /// http://clerc.maurice.free.fr/pso/mini%20benchmark.pdf
    /// </summary>
    public class Tripod : Problem
    {
        #region Constructors.
        /// <summary>
        /// Construct the object.
        /// </summary>
        public Tripod()
            : base()
        {
            Iterations = 10000;
        }
        #endregion

        #region Base-class overrides.
        /// <summary>
        /// Name of the optimization problem.
        /// </summary>
        public override string Name
        {
            get { return "Tripod"; }
        }

        /// <summary>
        /// Minimum possible fitness.
        /// </summary>
        public override double MinFitness
        {
            get { return 0.0d; }
        }

        /// <summary>
        /// Threshold for an acceptable fitness value.
        /// </summary>
        public override double AcceptableFitness
        {
            get { return 0.0001d; }
        }

        /// <summary>
        /// Compute and return fitness for the given parameters.
        /// </summary>
        /// <param name="x">Candidate solution.</param>
        public override double Fitness(double[] x)
        {
            Debug.Assert(x != null && x.Length == Dimensionality);

            double x1 = x[0];
            double x2 = x[1];
            double value = ((1.0 - Sign(x2)) / 2.0) * (Math.Abs(x1) + Math.Abs(x2 + 50.0)) +
                    ((1.0 + Sign(x2)) / 2.0) * ((1.0 - Sign(x1)) / 2.0) * (1.0 + Math.Abs(x1 + 50.0) + Math.Abs(x2 - 50.0)) +
                    ((1.0 + Sign(x1)) / 2.0) * (2.0 + Math.Abs(x1 - 50.0) + Math.Abs(x2 - 50.0));
            return value;
        }
        #endregion
        private readonly double[] _lowerBound = new[] { -100.0, -100.0 };
        public override double[] LowerBound
        {
            get { return _lowerBound; }
        }
        private readonly double[] _upperBound = new[] {100.0, 100.0};
        public override double[] UpperBound
        {
            get { return _upperBound; }
        }

        public override int Dimensionality
        {
            get { return 2; }
        }
        private double Sign(double val)
        {
            if (val <= 0.0d)
                return -1.0d;
            else
                return 1.0d;
        }
    }
}
