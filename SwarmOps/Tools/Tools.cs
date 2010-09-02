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
    /// Collection of commonly used methods.
    /// </summary>
    public static partial class Tools
    {
        /// <summary>
        /// Clamp the position and velocity vectors setting velocity to 0 if the position is outside of the solution space.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="v"></param>
        /// <param name="lower"></param>
        /// <param name="upper"></param>
        public static void Clamp(double[] x, double[] v, double[] lower, double[] upper)
        {
            if (x.Length != v.Length || x.Length != lower.Length || x.Length != upper.Length)
                throw new ArgumentOutOfRangeException("arrays", "Array lengths are not equal.");
            for (int i = 0; i < x.Length; i++)
            {
                if (x[i] < lower[i])
                {
                    x[i] = lower[i];
                    v[i] = 0.0;
                }
                if (x[i] > upper[i])
                {
                    x[i] = upper[i];
                    v[i] = 0.0;
                }
            }
        }
        public static double NextDouble(this Random rand, double lowerBound, double upperBound)
        {
            return lowerBound + rand.NextDouble() * (upperBound - lowerBound);
        }
        public static double NextDouble(this RandomOps.Random rand, double lowerBound, double upperBound)
        {
            return lowerBound + rand.Uniform() * (upperBound - lowerBound);
        }
    }
}
