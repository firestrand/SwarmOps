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
    /// Candidate solution found during optimization, consisting of parameters
    /// and fitness value.
    /// </summary>
    public class Solution
    {
        #region Constructors.
        /// <summary>
        /// Construct the object.
        /// </summary>
        /// <param name="parameters">Candidate solution parameters.</param>
        /// <param name="fitness">Fitness for candidate solution.</param>
        public Solution(double[] parameters, double fitness)
        {
            Parameters = parameters.Clone() as double[];
            Fitness = fitness;
        }
        #endregion

        #region Public fields.
        /// <summary>
        /// Candidate solution parameters.
        /// </summary>
        public double[] Parameters
        {
            get;
            protected set;
        }

        /// <summary>
        /// Fitness associated with candidate solution.
        /// </summary>
        public double Fitness
        {
            get;
            protected set;
        }
        #endregion
    }
}
