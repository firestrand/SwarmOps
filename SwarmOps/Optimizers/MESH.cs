/// ------------------------------------------------------
/// SwarmOps - Numeric and heuristic optimization for C#
/// Copyright (C) 2003-2009 Magnus Erik Hvass Pedersen.
/// Published under the GNU Lesser General Public License.
/// Please see the file license.txt for license details.
/// SwarmOps on the internet: http://www.Hvass-Labs.org/
/// ------------------------------------------------------

using System.Diagnostics;

namespace SwarmOps.Optimizers
{
    /// <summary>
    /// Optimizer that iterates over all possible combinations
    /// of parameters fitting a mesh of a certain size. This is
    /// particularly useful for displaying performance
    /// landscapes from meta-optimization, relating choices of
    /// control parameters to the performance of the optimizer.
    /// </summary>
    public class MESH : Optimizer
    {
        #region Constructors.
        /// <summary>
        /// Construct the object.
        /// </summary>
        public MESH()
            : base()
        {
        }

        /// <summary>
        /// Construct the object.
        /// </summary>
        /// <param name="problem">Problem to optimize.</param>
        public MESH(Problem problem)
            : base(problem)
        {
        }
        #endregion

        #region Get control parameters.
        /// <summary>
        /// Get parameter, Number of iterations per dimension.
        /// </summary>
        /// <param name="parameters">Optimizer parameters.</param>
        public int GetNumIterationsPerDim(double[] parameters)
        {
            return (int)System.Math.Round(parameters[0], System.MidpointRounding.AwayFromZero);
        }
        #endregion

        #region Base-class overrides, Problem.
        /// <summary>
        /// Name of the optimizer.
        /// </summary>
        public override string Name
        {
            get { return "MESH"; }
        }

        /// <summary>
        /// Number of control parameters for optimizer.
        /// </summary>
        public override int Dimensionality
        {
            get { return 1; }
        }

        string[] _parameterName = { "NumIterationsPerDim" };

        /// <summary>
        /// Control parameter names.
        /// </summary>
        public override string[] ParameterName
        {
            get { return _parameterName; }
        }

        static readonly double[] _defaultParameters = { 8.0 };

        /// <summary>
        /// Default control parameters.
        /// </summary>
        public override double[] DefaultParameters
        {
            get { return _defaultParameters; }
        }

        static readonly double[] _lowerBound = { 1 };

        /// <summary>
        /// Lower search-space boundary for control parameters.
        /// </summary>
        public override double[] LowerBound
        {
            get { return _lowerBound; }
        }

        static readonly double[] _upperBound = { 1000.0 };

        /// <summary>
        /// Upper search-space boundary for control parameters.
        /// </summary>
        public override double[] UpperBound
        {
            get { return _upperBound; }
        }
        #endregion

        #region Base-class overrides, Optimizer.
        /// <summary>
        /// Perform one optimization run and return the best found solution.
        /// </summary>
        /// <param name="parameters">Control parameters for the optimizer.</param>
        public override Result Optimize(double[] parameters)
        {
            Debug.Assert(parameters != null && parameters.Length == Dimensionality);

            // Retrieve parameter specific to this optimizer.
            int numIterationsPerDim = GetNumIterationsPerDim(parameters);

            Debug.Assert(numIterationsPerDim >= 1);

            // Get problem-context.
            double[] lowerBound = Problem.LowerBound;
            double[] upperBound = Problem.UpperBound;
            int n = Problem.Dimensionality;

            // Allocate mesh position and mesh-incremental values.
            double[] x = new double[n];					// Mesh position.
            double[] delta = new double[n];				// Mesh incremental values.
            double[] g = new double[n];					// Best found position for this run.
            double gFitness = Problem.MaxFitness;			// Fitness for best found position.

            // Initialize mesh position to the lower boundary.
            LowerBound.CopyTo(x, 0);

            // Compute mesh incremental values for all dimensions.
            for (int i = 0; i < n; i++)
            {
                delta[i] = (upperBound[i] - lowerBound[i]) / (numIterationsPerDim - 1);
            }

            // Start recursive traversal of mesh.
            Recursive(0, numIterationsPerDim, delta, ref x, ref g, ref gFitness);

            // Return best-found solution and fitness.
            return new Result(g, gFitness, (int)System.Math.Pow(numIterationsPerDim, n));
        }
        #endregion

        #region Protected methods.
        /// <summary>
        /// Helper function for recursive traversal of the mesh in a depth-first order.
        /// </summary>
        /// <param name="curDim">Current dimension being processed.</param>
        /// <param name="numIterationsPerDim">Number of mesh iterations per dimension.</param>
        /// <param name="delta">Distance between points in mesh.</param>
        /// <param name="x">Current mesh point.</param>
        /// <param name="g">Best found point in mesh.</param>
        /// <param name="gFitness">Fitness of best found point.</param>
        void Recursive(
            int curDim,
            int numIterationsPerDim,
            double[] delta,
            ref double[] x,
            ref double[] g,
            ref double gFitness)
        {
            // Get problem-context.
            double[] lowerBound = Problem.LowerBound;
            double[] upperBound = Problem.UpperBound;
            int n = Problem.Dimensionality;

            Debug.Assert(curDim >= 0 && curDim < n);

            // Iterate over all mesh-entries for current dimension.
            int i;
            for (i = 0; i < numIterationsPerDim; i++)
            {
                // Update mesh position for current dimension.
                x[curDim] = lowerBound[curDim] + delta[curDim] * i;

                // Bound mesh position for current dimension.
                x[curDim] = Tools.Bound(x[curDim], lowerBound[curDim], upperBound[curDim]);

                // Either recurse or compute fitness for mesh position.
                if (curDim < n - 1)
                {
                    // Recurse for next dimension.
                    Recursive(curDim + 1, numIterationsPerDim, delta, ref x, ref g, ref gFitness);
                }
                else
                {
                    // Compute fitness for current mesh position.
                    double fitness = Problem.Fitness(x, Problem.MaxFitness);

                    // Update best position and fitness found in this run.
                    if (fitness < gFitness)
                    {
                        // Update this run's best known position.
                        x.CopyTo(g, 0);

                        // Update this run's best know fitness.
                        gFitness = fitness;
                    }
                }
            }
        }
        #endregion
    }
}