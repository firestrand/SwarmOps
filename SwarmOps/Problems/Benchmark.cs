/// ------------------------------------------------------
/// SwarmOps - Numeric and heuristic optimization for C#
/// Copyright (C) 2003-2009 Magnus Erik Hvass Pedersen.
/// Published under the GNU Lesser General Public License.
/// Please see the file license.txt for license details.
/// SwarmOps on the internet: http://www.Hvass-Labs.org/
/// ------------------------------------------------------

namespace SwarmOps.Problems
{
    /// <summary>
    /// Base-class for a benchmark optimization problem.
    /// </summary>
    public abstract class Benchmark : Problem
    {
        #region Constructors.
        /// <summary>
        /// Construct the object.
        /// </summary>
        /// <param name="dimensionality">Dimensionality of the problem.</param>
        /// <param name="lowerBound">Lower boundary for entire search-space.</param>
        /// <param name="upperBound">Upper boundary for entire search-space.</param>
        /// <param name="lowerInit">Lower boundary for initialization.</param>
        /// <param name="upperInit">Upper boundary for initialization.</param>
        /// <param name="displaceValue">Displace optimum by this amount.</param>
        /// <param name="displaceOptimum">Use optimum displacement?</param>
        /// <param name="runCondition">
        /// Determines for how long to continue optimization.
        /// </param>
        public Benchmark(
            int dimensionality,
            double lowerBound,
            double upperBound,
            double lowerInit,
            double upperInit,
            double displaceValue,
            bool displaceOptimum,
            IRunCondition runCondition)
            : base(runCondition)
        {
            _dimensionality = dimensionality;
            DisplaceOptimum = displaceOptimum;
            DisplaceValue = displaceValue;

            _lowerBound = new double[Dimensionality];
            _upperBound = new double[Dimensionality];

            _lowerInit = new double[Dimensionality];
            _upperInit = new double[Dimensionality];

            for (int i = 0; i < Dimensionality; i++)
            {
                _lowerBound[i] = lowerBound;
                _upperBound[i] = upperBound;

                _lowerInit[i] = lowerInit;
                _upperInit[i] = upperInit;
            }
        }
        #endregion

        #region Base-class overrides.
        int _dimensionality;

        /// <summary>
        /// Dimensionality of the benchmark problem.
        /// </summary>
        public override int Dimensionality
        {
            get { return _dimensionality; }
        }

        double[] _lowerBound;

        /// <summary>
        /// Lower search-space boundary for the benchmark problem.
        /// </summary>
        public override double[] LowerBound
        {
            get { return _lowerBound; }
        }

        double[] _upperBound;

        /// <summary>
        /// Upper search-space boundary for the benchmark problem.
        /// </summary>
        public override double[] UpperBound
        {
            get { return _upperBound; }
        }

        double[] _lowerInit;

        /// <summary>
        /// Lower initialization boundary for the benchmark problem.
        /// </summary>
        public override double[] LowerInit
        {
            get { return _lowerInit; }
        }

        double[] _upperInit;

        /// <summary>
        /// Upper initialization boundary for the benchmark problem.
        /// </summary>
        public override double[] UpperInit
        {
            get { return _upperInit; }
        }
        #endregion.

        #region Private variables.
        /// <summary>
        /// Displace the optimum?
        /// </summary>
        bool DisplaceOptimum = false;

        /// <summary>
        /// Displace optimum by this amount.
        /// </summary>
        double DisplaceValue = 0;
        #endregion

        #region Protected methods.
        /// <summary>
        /// Displace the optimum by subtracting for each x the DisplaceValue.
        /// </summary>
        /// <param name="x">Parameter in the search-space.</param>
        /// <returns>Displaced parameter, if displacement is being used.</returns>
        protected double Displace(double x)
        {
            return (DisplaceOptimum) ? (x - DisplaceValue) : (x);
        }
        #endregion
    }
}
