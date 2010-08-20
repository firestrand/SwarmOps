/// ------------------------------------------------------
/// SwarmOps - Numeric and heuristic optimization for C#
/// Copyright (C) 2003-2009 Magnus Erik Hvass Pedersen.
/// Published under the GNU Lesser General Public License.
/// Please see the file license.txt for license details.
/// SwarmOps on the internet: http://www.Hvass-Labs.org/
/// ------------------------------------------------------

using System;
using System.Diagnostics;

namespace SwarmOps
{
    public static partial class Tools
    {
        /// <summary>
        /// Bound a value between lower and upper boundaries.
        /// </summary>
        /// <param name="x">Parameter to be bounded.</param>
        /// <param name="lower">Lower boundary.</param>
        /// <param name="upper">Upper boundary.</param>
        /// <returns>Bounded value.</returns>
        public static double Bound(double x, double lower, double upper)
        {
            Debug.Assert(upper >= lower);

            double y;

            if (x < lower)
            {
                y = lower;
            }
            else if (x > upper)
            {
                y = upper;
            }
            else
            {
                y = x;
            }

            return y;
        }

        /// <summary>
        /// Bound array of values between lower and upper boundaries.
        /// </summary>
        /// <param name="x">Array of values to be bounded.</param>
        /// <param name="lower">Lower boundaries.</param>
        /// <param name="upper">Upper boundaries.</param>
        public static void Bound(ref double[] x, double[] lower, double[] upper)
        {
            Debug.Assert(x.Length == lower.Length && x.Length == upper.Length);

            for (int i = 0; i < x.Length; i++)
            {
                x[i] = Bound(x[i], lower[i], upper[i]);
            }
        }
    }
}
