/// ------------------------------------------------------
/// SwarmOps - Numeric and heuristic optimization for C#
/// Published under the GNU Lesser General Public License.
/// Please see the file license.txt for license details.
/// SwarmOps on the internet: http://www.Hvass-Labs.org/
/// ------------------------------------------------------

using System;

namespace SwarmOps
{
    /// <summary>
    /// Performs a number of optimization runs and returns the
    /// percentage which met the success criteria found.
    /// This does NOT allow for Preemptive Fitness Evaluation!
    /// </summary>
    public class RepeatCount : Repeat
    {
        #region Constructors.
        /// <summary>
        /// Construct the object.
        /// </summary>
        /// <param name="optimizer">Optimizer to use.</param>
        /// <param name="numRuns">Number of optimization runs to perform.</param>
        public RepeatCount(Optimizer optimizer, int numRuns)
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
            get { return "RepeatCount(" + Optimizer.Name + ")"; }
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
        /// <returns>Fitness value.</returns>
        public override double Fitness(double[] parameters)
        {
            // Best fitness found so far is initialized to the worst possible fitness.
            double fitnessCount = 0.0;

            // Perform a number of optimization runs.
            for (int i = 0; i < NumRuns; i++)
            {
                // Perform one optimization run.
                Result result = Optimizer.Optimize(parameters);
                Console.WriteLine(String.Format("{0} {1} {2}",Optimizer.Problem.Name,result.Iterations, result.Fitness));
                // Count where Fitness is acceptable
                if (result.Fitness <= Optimizer.Problem.AcceptableFitness)
                {
                    fitnessCount++;
                }
            }

            return fitnessCount / NumRuns;
        }
        #endregion
    }
}
