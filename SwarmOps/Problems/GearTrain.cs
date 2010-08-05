﻿/// ------------------------------------------------------
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
    /// Gear Train benchmark problem. This variant is from "A mini-benchmark" by Clerc
    /// http://clerc.maurice.free.fr/pso/mini%20benchmark.pdf
    /// </summary>
    public class GearTrain : Problem
    {
        #region Constructors.
        /// <summary>
        /// Construct the object.
        /// </summary>
        public GearTrain()
            : base()
        {
        }
        #endregion

        #region Base-class overrides.
        /// <summary>
        /// Name of the optimization problem.
        /// </summary>
        public override string Name
        {
            get { return "GearTrain"; }
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
            get { return 2.7e-12d; }
        }

        /// <summary>
        /// Compute and return fitness for the given parameters.
        /// </summary>
        /// <param name="x">Candidate solution.</param>
        public override double Fitness(double[] x)
        {
            Debug.Assert(x != null && x.Length == Dimensionality);
            //This problem uses integer values only. Round and enforce
            for (int i = 0; i < x.Length; i++)
            {
                x[i] = Math.Round(x[i], MidpointRounding.AwayFromZero);
            }
            return Math.Pow((1.0 / 6.931) - ((x[0] * x[1]) / (x[2] * x[3])), 2);
        }
        #endregion
        private readonly double[] _lowerBound = Enumerable.Repeat(12.0d,4).ToArray();
        public override double[] LowerBound
        {
            get { return _lowerBound; }
        }
        private readonly double[] _upperBound = Enumerable.Repeat(60.0d, 4).ToArray();
        public override double[] UpperBound
        {
            get { return _upperBound; }
        }

        public override int Dimensionality
        {
            get { return 4; }
        }
    }
}