﻿/// ------------------------------------------------------
/// SwarmOps - Numeric and heuristic optimization for C#
/// Copyright (C) 2003-2011 Magnus Erik Hvass Pedersen.
/// Please see the file license.txt for license details.
/// SwarmOps on the internet: http://www.Hvass-Labs.org/
/// ------------------------------------------------------

using System.Diagnostics;

namespace SwarmOps.Problems
{
    /// <summary>
    /// Rosenbrock benchmark problem.
    /// </summary>
    public class Rosenbrock : Benchmark
    {
        #region Constructors.
        /// <summary>
        /// Construct the object.
        /// </summary>
        /// <param name="dimensionality">Dimensionality of the problem (e.g. 20)</param>
        /// <param name="maxIterations">Max optimization iterations to perform.</param>
        public Rosenbrock(int dimensionality, int maxIterations)
            : base(dimensionality, -100, 100, 15, 30, maxIterations)
        {
        }
        #endregion

        #region Base-class overrides.
        /// <summary>
        /// Name of the optimization problem.
        /// </summary>
        public override string Name
        {
            get { return "Rosenbrock"; }
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

            for (int i = 0; i < Dimensionality - 1; i++)
            {
                double elm = x[i];
                double nextElm = x[i + 1];

                double minusOne = elm - 1;
                double nextMinusSqr = nextElm - elm * elm;

                value += 100 * nextMinusSqr * nextMinusSqr + minusOne * minusOne;
            }

            return value;
        }

        /// <summary>
        /// Has the gradient has been implemented?
        /// </summary>
        public override bool HasGradient
        {
            get { return true; }
        }

        /// <summary>
        /// Compute the gradient of the fitness-function.
        /// </summary>
        /// <param name="x">Candidate solution.</param>
        /// <param name="v">Array for holding the gradient.</param>
        public override int Gradient(double[] x, ref double[] v)
        {
            Debug.Assert(x != null && x.Length == Dimensionality);
            Debug.Assert(v != null && v.Length == Dimensionality);

            for (int i = 0; i < Dimensionality - 1; i++)
            {
                double elm = x[i];
                double nextElm = x[i + 1];

                v[i] = -400 * (nextElm - elm * elm) * elm + 2 * (elm - 1);
            }

            // Gradient for the last dimension.
            {
                double elm = x[Dimensionality - 1];
                double prevElm = x[Dimensionality - 2];

                v[Dimensionality - 1] = 200 * (elm - prevElm * prevElm);
            }

            return 0;
        }
        #endregion
    }
}
