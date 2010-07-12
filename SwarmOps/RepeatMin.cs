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
    /// Performs a number of optimization runs and returns the
    /// minimum fitness found.
    /// This does NOT allow for Preemptive Fitness Evaluation!
    /// </summary>
    public class RepeatMin : Repeat
    {
        #region Constructors.
        /// <summary>
        /// Construct the object.
        /// </summary>
        /// <param name="optimizer">Optimizer to use.</param>
        /// <param name="numRuns">Number of optimization runs to perform.</param>
        public RepeatMin(Optimizer optimizer, int numRuns)
            : base(optimizer, numRuns)
        {
        }
        #endregion

        #region Base-class overrides.
        /// <summary>
        /// Return problem-name.
        /// </summary>
        public override string Name
        {
            get { return "RepeatMin(" + Optimizer.Name + ")"; }
        }

        /// <summary>
        /// Return minimum fitness possible. This is the same as
        /// the minimum fitness of the Optimizer in question.
        /// </summary>
        public override double MinFitness
        {
            get { return Optimizer.MinFitness; }
        }

        /// <summary>
        /// Compute the fitness by repeating a number of optimization runs
        /// and taking the best fitness achieved in any one of these runs.
        /// </summary>
        /// <param name="parameters">Parameters to use for the Optimizer.</param>
        /// <param name="fitnessLimit">Preemptive Fitness Limit</param>
        /// <returns>Fitness value.</returns>
        public override double Fitness(double[] parameters, double fitnessLimit)
        {
            // Best fitness found so far is initialized to the worst possible fitness.
            double fitnessMin = Optimizer.MaxFitness;

            // Perform a number of optimization runs.
            for (int i = 0; i < NumRuns; i++)
            {
                // Perform one optimization run.
                Result result = Optimizer.Optimize(parameters, fitnessLimit);

                // Update the best fitness found so far, if improvement.
                if (result.Fitness < fitnessMin)
                {
                    fitnessMin = result.Fitness;
                }
            }

            return fitnessMin;
        }
        #endregion
    }
}
