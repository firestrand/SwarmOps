/// ------------------------------------------------------
/// SwarmOps - Numeric and heuristic optimization for C#
/// Copyright (C) 2003-2009 Magnus Erik Hvass Pedersen.
/// Published under the GNU Lesser General Public License.
/// Please see the file license.txt for license details.
/// SwarmOps on the internet: http://www.Hvass-Labs.org/
/// ------------------------------------------------------

using System.Diagnostics;

namespace SwarmOps
{
    /// <summary>
    /// For use in meta-optimization with several
    /// problems. This wraps around a Repeat-object.
    /// It is useful because there are different
    /// Repeat-classes. Otherwise you may wish to
    /// use the MetaFitness-class which is essentially
    /// a combination of the Multi-class and the
    /// RepeatSum-class that also allows for weighted
    /// problems.
    /// </summary>
    public class Multi : Problem
    {
        #region Constructors.
        /// <summary>
        /// Create new object.
        /// </summary>
        /// <param name="problems">Problems to be optimized.</param>
        /// <param name="repeat">Repeat-object.</param>
        public Multi(Problem[] problems, Repeat repeat)
            : base()
        {
            Repeat = repeat;
            ProblemIndex = new ProblemIndex(problems);
        }
        #endregion

        #region Public fields.
        /// <summary>
        /// Repeat-object which is a wrapper around the Optimizer.
        /// </summary>
        public Repeat Repeat
        {
            get;
            private set;
        }
        #endregion

        #region Base-class overrides.
        /// <summary>
        /// Used for determining whether or not to continue optimization.
        /// </summary>
        public override IRunCondition RunCondition
        {
            get { return Repeat.RunCondition; }
            set { Repeat.RunCondition = value; }
        }

        /// <summary>
        /// Minimum fitness possible which is zero because of normalization
        /// being employed.
        /// </summary>
        public override double MinFitness
        {
            get { return 0; }
        }

        /// <summary>
        /// Return LowerBound of Repeat.
        /// </summary>
        public override double[] LowerBound
        {
            get { return Repeat.LowerBound; }
        }

        /// <summary>
        /// Return UpperBound of Repeat.
        /// </summary>
        public override double[] UpperBound
        {
            get { return Repeat.Optimizer.UpperBound; }
        }

        /// <summary>
        /// Return LowerInit of Repeat.
        /// </summary>
        public override double[] LowerInit
        {
            get { return Repeat.Optimizer.LowerInit; }
        }

        /// <summary>
        /// Return UpperInit of Repeat.
        /// </summary>
        public override double[] UpperInit
        {
            get { return Repeat.Optimizer.UpperInit; }
        }

        /// <summary>
        /// Return name of problem.
        /// </summary>
        public override string Name
        {
            get { return "Multi(" + Repeat.Name + ")"; }
        }

        /// <summary>
        /// Return Dimensionality of Repeat.Optimizer.
        /// </summary>
        public override int Dimensionality
        {
            get { return Repeat.Optimizer.Dimensionality; }
        }

        /// <summary>
        /// Compute the fitness by optimizing all the problems in turn and
        /// summing their fitness results.
        /// </summary>
        /// <param name="parameters">Parameters to use for the Optimizer.</param>
        /// <param name="fitnessLimit">Preemptive Fitness Limit</param>
        /// <returns>Fitness value.</returns>
        public override double Fitness(double[] parameters, double fitnessLimit)
        {
            // Initialize the fitness sum.
            double fitnessSum = 0;

            // Iterate over the problems.
            for (int i = 0; i < ProblemIndex.Count && fitnessSum < fitnessLimit; i++)
            {
                // Assign the problem to the optimizer.
                Repeat.Optimizer.Problem = ProblemIndex.GetProblem(i);

                // Perform optimization runs.
                double fitness = Repeat.Fitness(parameters, fitnessLimit - fitnessSum);

                // Adjust the fitness result by subtracting its minimum possible value.
                double fitnessAdjusted = fitness - Repeat.MinFitness;

                // Ensure adjusted fitness is non-negative, otherwise Preemptive
                // Fitness Evaluation does not work.
                Debug.Assert(fitnessAdjusted >= 0);

                // Set the fitness result achieved on the problem.
                ProblemIndex.SetFitness(i, fitnessAdjusted);

                // Accumulate fitness sum.
                fitnessSum += fitnessAdjusted;
            }

            // Sort the optimization problems so that the worst
            // performing will be attempted optimized first, when
            // this method is called again.
            ProblemIndex.Sort();

            return fitnessSum;
        }
        #endregion

        #region Private members.
        /// <summary>
        /// Sorted index of optimization problems.
        /// </summary>
        ProblemIndex ProblemIndex;
        #endregion
    }
}