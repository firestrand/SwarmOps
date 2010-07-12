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
        /// Print parameters using names associated with an optimization problem.
        /// </summary>
        /// <param name="problem">Optimization problem with associated parameter-names.</param>
        /// <param name="parameters">Parameters to be printed.</param>
        public static void PrintParameters(Problem problem, double[] parameters)
        {
            int NumParameters = problem.Dimensionality;

            if (NumParameters > 0)
            {
                Debug.Assert(parameters.Length == NumParameters);

                for (int i = 0; i < NumParameters; i++)
                {
                    string parameterName = problem.ParameterName[i];
                    double p = parameters[i];
                    string pStr = p.ToString("0.####", _cultureInfo);

                    Console.WriteLine("\t{0} = {1}", parameterName, pStr);
                }
            }
            else
            {
                Console.WriteLine("\tN/A");
            }
        }
    }
}
