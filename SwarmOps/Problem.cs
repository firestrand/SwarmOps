/// ------------------------------------------------------
/// SwarmOps - Numeric and heuristic optimization for C#
/// Copyright (C) 2003-2009 Magnus Erik Hvass Pedersen.
/// Published under the GNU Lesser General Public License.
/// Please see the file license.txt for license details.
/// SwarmOps on the internet: http://www.Hvass-Labs.org/
/// ------------------------------------------------------

using System;

namespace SwarmOps
{
    /// <summary>
    /// Base-class for an optimization problem.
    /// </summary>
    public abstract class Problem
    {
        #region Constructors.
        /// <summary>
        /// Create the object.
        /// </summary>
        public Problem()
        {
        }

        /// <summary>
        /// Create the object.
        /// </summary>
        /// <param name="runCondition">
        /// Determines for how long to continue optimization.
        /// </param>
        public Problem(IRunCondition runCondition)
        {
            RunCondition = runCondition;
        }
        #endregion

        #region Public fields, override these.
        /// <summary>
        /// Used for determining whether or not to continue optimization.
        /// </summary>
        public virtual IRunCondition RunCondition
        {
            get;
            set;
        }

        /// <summary>
        /// Return name of the optimization problem.
        /// </summary>
        public abstract string Name
        {
            get;
        }

        /// <summary>
        /// Array with names of parameters.
        /// </summary>
        public virtual string[] ParameterName
        {
            get { return null; }
        }

        /// <summary>
        /// Lower search-space boundary.
        /// </summary>
        public abstract double[] LowerBound
        {
            get;
        }

        /// <summary>
        /// Upper search-space boundary.
        /// </summary>
        public abstract double[] UpperBound
        {
            get;
        }

        /// <summary>
        /// Lower initialization boundary,
        /// if different from search-space boundary.
        /// </summary>
        public virtual double[] LowerInit
        {
            get { return LowerBound; }
        }

        /// <summary>
        /// Upper initialization boundary,
        /// if different from search-space boundary.
        /// </summary>
        public virtual double[] UpperInit
        {
            get { return UpperBound; }
        }

        /// <summary>
        /// Maximum (i.e. worst) fitness possible.
        /// </summary>
        public virtual double MaxFitness
        {
            get { return double.MaxValue; }
        }

        /// <summary>
        /// Minimum (i.e. best) fitness possible. This is
        /// especially important if using meta-optimization
        /// where the Fitness is assumed to be non-negative,
        /// and should be roughly equivalent amongst all the
        /// problems meta-optimized for.
        /// </summary>
        public abstract double MinFitness
        {
            get;
        }

        /// <summary>
        /// Threshold for an acceptable fitness value.
        /// </summary>
        public virtual double AcceptableFitness
        {
            get { return MinFitness; }
        }

        /// <summary>
        /// Return dimensionality of the problem, that is, the number
        /// of parameters in a candidate solution.
        /// </summary>
        public abstract int Dimensionality
        {
            get;
        }

        /// <summary>
        /// Has the gradient has been implemented?
        /// </summary>
        public virtual bool HasGradient
        {
            get { return false; }
        }
        #endregion

        #region Public methods, override these.
        /// <summary>
        /// Compute and return fitness for the given parameters.
        /// </summary>
        /// <param name="parameters">Candidate solution.</param>
        public virtual double Fitness(double[] parameters)
        {
            return Fitness(parameters, MaxFitness);
        }

        /// <summary>
        /// Compute and return fitness for the given parameters.
        /// The fitness evaluation is aborted preemptively, if the
        /// fitness becomes higher (i.e. worse) than fitnessLimit, and
        /// if it is not possible for the fitness to improve.
        /// </summary>
        /// <param name="parameters">Candidate solution.</param>
        /// <param name="fitnessLimit">Preemptive Fitness Limit.</param>
        public virtual double Fitness(double[] parameters, double fitnessLimit)
        {
            return Fitness(parameters);
        }

        /// <summary>
        /// Compute the gradient of the fitness-function.
        /// </summary>
        /// <param name="x">Candidate solution.</param>
        /// <param name="v">Array for holding the gradient.</param>
        /// <returns>
        /// Computation time-complexity factor. E.g. if fitness takes
        /// time O(n) to compute and gradient takes time O(n*n) to compute,
        /// then return n.
        /// </returns>
        public virtual int Gradient(double[] x, ref double[] v)
        {
            throw new NotImplementedException();
        }

        public virtual int Iterations { get; set; } //Iterations to run
        #endregion
    }
}
