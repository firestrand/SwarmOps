/// ------------------------------------------------------
/// SwarmOps - Numeric and heuristic optimization for C#
/// Copyright (C) 2003-2009 Magnus Erik Hvass Pedersen.
/// Published under the GNU Lesser General Public License.
/// Please see the file license.txt for license details.
/// SwarmOps on the internet: http://www.Hvass-Labs.org/
/// ------------------------------------------------------

using System.Diagnostics;

namespace TestCustomProblem
{
    /// <summary>
    /// Custom optimization problem, example.
    /// </summary>
    class CustomProblem : SwarmOps.Problem
    {
        #region Constructors.
        /// <summary>
        /// Construct the object.
        /// </summary>
        public CustomProblem()
            : base()
        {
        }
        #endregion

        #region Get parameters.
        /// <summary>
        /// Get parameter, A.
        /// </summary>
        /// <param name="parameters">Optimizer parameters.</param>
        public double GetA(double[] parameters)
        {
            return parameters[0];
        }

        /// <summary>
        /// Get parameter, B.
        /// </summary>
        /// <param name="parameters">Optimizer parameters.</param>
        public double GetB(double[] parameters)
        {
            return parameters[1];
        }

        /// <summary>
        /// Get parameter, C.
        /// </summary>
        /// <param name="parameters">Optimizer parameters.</param>
        public double GetC(double[] parameters)
        {
            return parameters[2];
        }
        #endregion

        #region Base-class overrides.
        /// <summary>
        /// Name of the optimizer.
        /// </summary>
        public override string Name
        {
            get { return "CustomProblem"; }
        }

        /// <summary>
        /// Dimensionality of the problem.
        /// </summary>
        public override int Dimensionality
        {
            get { return 3; }
        }

        double[] _lowerBound = { -10, -20, -30 };

        /// <summary>
        /// Lower search-space boundary.
        /// </summary>
        public override double[] LowerBound
        {
            get { return _lowerBound; }
        }

        double[] _upperBound = { 30, 40, 70 };

        /// <summary>
        /// Upper search-space boundary.
        /// </summary>
        public override double[] UpperBound
        {
            get { return _upperBound; }
        }

        /// <summary>
        /// Lower initialization boundary.
        /// </summary>
        public override double[] LowerInit
        {
            get { return LowerBound; }
        }

        /// <summary>
        /// Upper initialization boundary.
        /// </summary>
        public override double[] UpperInit
        {
            get { return UpperBound; }
        }

        /// <summary>
        /// Minimum possible fitness for this problem.
        /// </summary>
        public override double MinFitness
        {
            get { return 0; }
        }

        /// <summary>
        /// Acceptable fitness threshold.
        /// </summary>
        public override double AcceptableFitness
        {
            get { return 0.01; }
        }

        string[] _parameterName = { "a", "b", "c" };
 
        /// <summary>
        /// Names of parameters for problem.
        /// </summary>
        public override string[] ParameterName
        {
            get { return _parameterName; }
        }

        /// <summary>
        /// Compute and return fitness for the given parameters.
        /// </summary>
        /// <param name="x">Candidate solution.</param>
        public override double Fitness(double[] x)
        {
            Debug.Assert(x != null && x.Length == Dimensionality);

            double a = GetA(x);
            double b = GetB(x);
            double c = GetC(x);

            double value
                = 2 * a * a
                + 3 * a * b 
                + 4 * b * b 
                + 5 * b * c 
                + 6 * c * c;

            return value;
        }
        #endregion
    }
}
