/// ------------------------------------------------------
/// SwarmOps - Numeric and heuristic optimization for C#
/// Published under the GNU Lesser General Public License.
/// Please see the file license.txt for license details.
/// SwarmOps on the internet: http://www.Hvass-Labs.org/
/// ------------------------------------------------------

using System;
using System.Diagnostics;
using System.Linq;

namespace SwarmOps.Problems
{
    /// <summary>
    /// Rosenbrock F6 benchmark problem. This variant is from "A mini-benchmark" by Clerc
    /// http://clerc.maurice.free.fr/pso/mini%20benchmark.pdf
    /// </summary>
    public class RosenbrockF6 : Problem
    {
        private readonly double[] _offset = new[]{81.0232,-48.395,19.2316,-2.5231,70.4338,47.1774,-7.8358,-86.6693,57.8532,-9.9533};
        public double[] Offset
        { get { return _offset; } }
        #region Constructors.
        /// <summary>
        /// Construct the object.
        /// </summary>
        /// <param name="dimensionality">Dimensionality of the problem (e.g. 20)</param>
        /// <param name="displaceOptimum">Displace optimum?</param>
        /// <param name="runCondition">
        /// Determines for how long to continue optimization.
        /// </param>

        public RosenbrockF6()
            : base()
        {
            Iterations = 100000;
        }
        
        #endregion

        #region Base-class overrides.
        /// <summary>
        /// Name of the optimization problem.
        /// </summary>
        public override string Name
        {
            get { return "RosenbrockF6"; }
        }

        /// <summary>
        /// Minimum possible fitness.
        /// </summary>
        public override double MinFitness
        {
            get { return 0.0d; }
        }
        public override double AcceptableFitness
        {
            get
            {
                return 0.01d;
            }
        }
        /// <summary>
        /// Compute and return fitness for the given parameters.
        /// </summary>
        /// <param name="x">Candidate solution.</param>
        public override double Fitness(double[] x)
        {
            Debug.Assert(x != null && x.Length == Dimensionality);

            double value = 390.0d;
            double z0, z1;
            for (int i = 1; i < Dimensionality; i++)
            {
                z0 = x[i - 1] - _offset[i - 1];
                z1 = x[i] - _offset[i];
                value += 100.0* Math.Pow(Math.Pow(z0,2) - z1,2) + Math.Pow(z0-1.0,2);
            }
            return Math.Abs(390.0-value);
        }

        /// <summary>
        /// Has the gradient has been implemented?
        /// </summary>
        public override bool HasGradient
        {
            get { return false; }
        }

        /// <summary>
        /// Compute the gradient of the fitness-function.
        /// </summary>
        /// <param name="x">Candidate solution.</param>
        /// <param name="v">Array for holding the gradient.</param>
        public override int Gradient(double[] x, ref double[] v)
        {
            throw new NotImplementedException();
        }
        #endregion

        private readonly double[] _lowerBound = Enumerable.Repeat(-100.0, 10).ToArray();
        public override double[] LowerBound
        {
            get { return _lowerBound; }
        }

        private readonly double[] _upperBound = Enumerable.Repeat(100.0, 10).ToArray();
        public override double[] UpperBound
        {
            get { return _upperBound; }
        }

        public override int Dimensionality
        {
            get { return 10; }
        }
    }
}
