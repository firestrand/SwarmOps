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
    /// Helper-methods for Penalized benchmark problems.
    /// </summary>
    public static class Penalized
    {
        /// <summary>
        /// Helper-method for Penalized benchmark problems.
        /// </summary>
        public static double U(double x, double a, double k, double m)
        {
            double value;

            if (x < -a)
            {
                value = k * System.Math.Pow(-x - a, m);
            }
            else if (x > a)
            {
                value = k * System.Math.Pow(x - a, m);
            }
            else
            {
                value = 0;
            }

            return value;
        }
    }
}
