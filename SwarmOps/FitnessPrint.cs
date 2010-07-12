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
    /// Prints parameters and fitness to Console. Useful in
    /// viewing the progress of meta-optimization. Works as
    /// a 'transparent' wrapper for the problem to be optimized.
    /// </summary>
    public class FitnessPrint : ProblemWrapper
    {
        #region Constructors.
        /// <summary>
        /// Constructs a new object.
        /// </summary>
        /// <param name="problem">The problem being wrapped.</param>
        public FitnessPrint(Problem problem)
            : base(problem)
        {
        }

        /// <summary>
        /// Constructs a new object.
        /// </summary>
        /// <param name="problem">The problem being wrapped.</param>
        /// <param name="formatAsArray">Format output string as C# array.</param>
        public FitnessPrint(Problem problem, bool formatAsArray)
            : base(problem)
        {
            FormatAsArray = formatAsArray;
        }
        #endregion

        #region Public fields.
        /// <summary>
        /// Format output string as C# array.
        /// </summary>
        public bool FormatAsArray
        {
            get;
            set;
        }
        #endregion

        #region Base-class overrides.
        /// <summary>
        /// Return name of the optimization problem.
        /// </summary>
        public override string Name
        {
            get { return "FitnessPrint (" + Problem.Name + ")"; }
        }

        /// <summary>
        /// Compute fitness of wrapped problem and print the result.
        /// </summary>
        public override double Fitness(double[] parameters, double fitnessLimit)
        {
            double fitness = Problem.Fitness(parameters, fitnessLimit);

            DoPrint(parameters, fitness, fitnessLimit, FormatAsArray);

            return fitness;
        }
        #endregion

        #region Printing methods.
        /// <summary>
        /// Print parameters and fitness to Console, and print a marking if
        /// fitness was an improvement to fitnessLimit.
        /// </summary>
        public static void DoPrint(double[] parameters, double fitness, double fitnessLimit, bool formatAsArray)
        {
            // Convert parameters to a string.
            string parametersStr = (formatAsArray) ? (Tools.ArrayToString(parameters)) : (Tools.ArrayToStringRaw(parameters));

            Console.WriteLine("{0} \t{1} \t{2}",
                parametersStr,
                Tools.FormatNumber(fitness),
                (fitness < fitnessLimit) ? ("***") : (""));

            // Flush stdout, this is useful if piping the output and you wish
            // to study the the output before the entire optimization run is complete.
            Console.Out.Flush();
        }
        #endregion
    }
}
