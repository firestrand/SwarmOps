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
    /// Interface for a class used for determining
    /// if an optimization run should be allowed to
    /// continue.
    /// </summary>
    public interface IRunCondition
    {
        /// <summary>
        /// Return whether optimization is allowed to continue.
        /// </summary>
        /// <param name="iteration">Number of iterations performed in optimization run.</param>
        /// <param name="fitness">Best fitness found in optimization run.</param>
        bool Continue(int iteration, double fitness);
    }
}
