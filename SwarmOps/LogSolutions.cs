/// ------------------------------------------------------
/// SwarmOps - Numeric and heuristic optimization for C#
/// Copyright (C) 2003-2009 Magnus Erik Hvass Pedersen.
/// Published under the GNU Lesser General Public License.
/// Please see the file license.txt for license details.
/// SwarmOps on the internet: http://www.Hvass-Labs.org/
/// ------------------------------------------------------

using System.Collections.Generic;

namespace SwarmOps
{
    /// <summary>
    /// Log best solutions found during optimization, that is, log
    /// parameters and their associated fitness. Transparently wraps
    /// around a problem-object.
    /// </summary>
    public class LogSolutions : ProblemWrapper
    {
        #region Constructors.
        /// <summary>
        /// Create the object.
        /// </summary>
        /// <param name="problem">Problem-object to be wrapped.</param>
        /// <param name="capacity">Log capacity.</param>
        public LogSolutions(Problem problem, int capacity)
            : base(problem)
        {
            Capacity = capacity;

            _log = new SortedList<double, Solution>(capacity);
        }
        #endregion

        #region Private fields.
        /// <summary>
        /// Log of candidate solutions.
        /// </summary>
        SortedList<double, Solution> _log;
        #endregion

        #region Public fields.
        /// <summary>
        /// Maximum capacity of log.
        /// </summary>
        public int Capacity
        {
            get;
            private set;
        }

        /// <summary>
        /// Log of candidate solutions.
        /// </summary>
        public IList<Solution> Log
        {
            get { return _log.Values; }
        }
        #endregion

        #region Public methods.
        /// <summary>
        /// Clear the log.
        /// </summary>
        public void Clear()
        {
            _log.Clear();
        }
        #endregion

        #region Problem base-class overrides.
        /// <summary>
        /// Return the name of the problem.
        /// </summary>
        public override string Name
        {
            get { return "LogSolutions (" + Problem.Name + ")"; }
        }

        /// <summary>
        /// Compute the fitness measure by passing the
        /// given parameters to the wrapped problem, and if
        /// candidate solution is an improvement then log
        /// the results.
        /// </summary>
        /// <param name="parameters">Candidate solution.</param>
        /// <param name="fitnessLimit">Preemptive Fitness Limit</param>
        /// <returns>Fitness value.</returns>
        public override double Fitness(double[] parameters, double fitnessLimit)
        {
            double fitness = Problem.Fitness(parameters, fitnessLimit);

            if (fitness < fitnessLimit)
            {
                Solution candidateSolution = new Solution(parameters, fitness);

                // Add new solution to the log.
                _log.Add(fitness, candidateSolution);

                if (Log.Count > Capacity)
                {
                    // Remove worst from the log.
                    _log.RemoveAt(Log.Count - 1);
                }
            }

            return fitness;
        }
        #endregion
    }
}
