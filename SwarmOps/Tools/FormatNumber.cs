/// ------------------------------------------------------
/// SwarmOps - Numeric and heuristic optimization for C#
/// Copyright (C) 2003-2009 Magnus Erik Hvass Pedersen.
/// Published under the GNU Lesser General Public License.
/// Please see the file license.txt for license details.
/// SwarmOps on the internet: http://www.Hvass-Labs.org/
/// ------------------------------------------------------

using System;
using System.Globalization;

namespace SwarmOps
{
    public static partial class Tools
    {
        private static CultureInfo _cultureInfo = new CultureInfo("en-US");

        /// <summary>
        /// Convert numeric value d to a string with convenient formatting.
        /// </summary>
        public static string FormatNumber(double d)
        {
            string s;
            double dAbs = Math.Abs(d);

            if (dAbs < 1e-2)
            {
                s = String.Format(_cultureInfo, "{0:0.##e0}", d);
            }
            else if (dAbs > 1e+6)
            {
                s = String.Format(_cultureInfo, "{0:0.##e+0}", d);
            }
            else if (dAbs > 1e+3)
            {
                s = String.Format(_cultureInfo, "{0:0.}", d);
            }
            else
            {
                s = String.Format(_cultureInfo, "{0:0.##}", d);
            }

            return s;
        }

    }
}
