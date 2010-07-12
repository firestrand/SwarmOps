/// ------------------------------------------------------
/// SwarmOps - Numeric and heuristic optimization for C#
/// Copyright (C) 2003-2009 Magnus Erik Hvass Pedersen.
/// Published under the GNU Lesser General Public License.
/// Please see the file license.txt for license details.
/// SwarmOps on the internet: http://www.Hvass-Labs.org/
/// ------------------------------------------------------

namespace SwarmOps
{
    /// <summary>
    /// Transparently wrap a problem-object.
    /// </summary>
    public abstract class ProblemWrapper : Problem
    {
        #region Constructors.
        /// <summary>
        /// Create the object.
        /// </summary>
        /// <param name="problem">Problem-object to be wrapped.</param>
        public ProblemWrapper(Problem problem)
            : base()
        {
            Problem = problem;
        }
        #endregion

        #region Public fields.
        /// <summary>
        /// The problem that is being wrapped.
        /// </summary>
        public Problem Problem
        {
            get;
            private set;
        }
        #endregion

        #region Problem base-class overrides.
        /// <summary>
        /// Used for determining whether or not to continue optimization.
        /// </summary>
        public override IRunCondition RunCondition
        {
            get { return Problem.RunCondition; }
            set { Problem.RunCondition = value; }
        }

        /// <summary>
        /// Return LowerBound of wrapped problem.
        /// </summary>
        public override double[] LowerBound
        {
            get { return Problem.LowerBound; }
        }

        /// <summary>
        /// Return UpperBound of wrapped problem.
        /// </summary>
        public override double[] UpperBound
        {
            get { return Problem.UpperBound; }
        }

        /// <summary>
        /// Return LowerInit of wrapped problem.
        /// </summary>
        public override double[] LowerInit
        {
            get { return Problem.LowerInit; }
        }

        /// <summary>
        /// Return UpperInit of wrapped problem.
        /// </summary>
        public override double[] UpperInit
        {
            get { return Problem.UpperInit; }
        }

        /// <summary>
        /// Return Dimensionality of wrapped problem.
        /// </summary>
        public override int Dimensionality
        {
            get { return Problem.Dimensionality; }
        }

        /// <summary>
        /// Return MinFitness of wrapped problem.
        /// </summary>
        public override double MinFitness
        {
            get { return Problem.MinFitness; }
        }

        /// <summary>
        /// Return MaxFitness of wrapped problem.
        /// </summary>
        public override double MaxFitness
        {
            get { return Problem.MaxFitness; }
        }

        /// <summary>
        /// Return AcceptableFitness of wrapped problem.
        /// </summary>
        public override double AcceptableFitness
        {
            get { return Problem.AcceptableFitness; }
        }

        /// <summary>
        /// Return ParameterName of wrapped problem.
        /// </summary>
        public override string[] ParameterName
        {
            get { return Problem.ParameterName; }
        }

        /// <summary>
        /// Return HasGradient of wrapped problem.
        /// </summary>
        public override bool HasGradient
        {
            get { return Problem.HasGradient; }
        }

        /// <summary>
        /// Compute the gradient of the wrapped problem.
        /// </summary>
        /// <param name="x">Candidate solution.</param>
        /// <param name="v">Array for holding the gradient.</param>
        /// <returns>
        /// Computation time-complexity factor. E.g. if fitness takes
        /// time O(n) to compute and gradient takes time O(n*n) to compute,
        /// then return n.
        /// </returns>
        public override int Gradient(double[] x, ref double[] v)
        {
            return Problem.Gradient(x, ref v);
        }
        #endregion
    }
}
