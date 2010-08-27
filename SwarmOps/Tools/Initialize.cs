/// ------------------------------------------------------
/// SwarmOps - Numeric and heuristic optimization for C#
/// Copyright (C) 2003-2009 Magnus Erik Hvass Pedersen.
/// Published under the GNU Lesser General Public License.
/// Please see the file license.txt for license details.
/// SwarmOps on the internet: http://www.Hvass-Labs.org/
/// ------------------------------------------------------

using System.Diagnostics;
using System.Linq;
namespace SwarmOps
{
    public static partial class Tools
    {
        /// <summary>
        /// Initialize array with value.
        /// </summary>
        /// <param name="x">Array to be initialized.</param>
        /// <param name="value">Value.</param>
        public static void Initialize(ref double[] x, double value)
        {
            for (int i = 0; i < x.Length; i++)
            {
                x[i] = value;
            }
        }

        /// <summary>
        /// Initialize array with uniform random values between given boundaries.
        /// </summary>
        /// <param name="x">Array to be initialized.</param>
        /// <param name="lower">Lower boundary.</param>
        /// <param name="upper">Upper boundary.</param>
        public static void InitializeUniform(ref double[] x, double lower, double upper)
        {
            for (int i = 0; i < x.Length; i++)
            {
                x[i] = Globals.Random.Uniform(lower, upper);
            }
        }

        /// <summary>
        /// Initialize array with uniform random values between given boundaries.
        /// </summary>
        /// <param name="x">Array to be initialized.</param>
        /// <param name="lower">Array of lower boundary.</param>
        /// <param name="upper">Array of upper boundary.</param>
        public static void InitializeUniform(ref double[] x, double[] lower, double[] upper)
        {
            for (int i = 0; i < x.Length; i++)
            {
                x[i] = Globals.Random.Uniform(lower[i], upper[i]);
            }
        }

        /// <summary>
        /// Initialize array with the range between the boundaries.
        /// That is, x[i] = upper[i]-lower[i].
        /// </summary>
        /// <param name="x">Array to be initialized.</param>
        /// <param name="lower">Lower boundary.</param>
        /// <param name="upper">Upper boundary.</param>
        public static void InitializeRange(ref double[] x, double[] lower, double[] upper)
        {
            for (int i = 0; i < x.Length; i++)
            {
                x[i] = upper[i] - lower[i];

                Debug.Assert(x[i] >= 0);
            }
        }
        /// <summary>
        /// Simulated Orthogonal Array
        /// </summary>
        /// <param name="x"></param>
        /// <param name="m"></param>
        /// <param name="lower"></param>
        /// <param name="upper"></param>
        public static void InitializeSOA(ref double[][] x, double[] lower, double[] upper)
        {
            //TODO: Assert that lengths are > 0
            int numAgents = x.Length;
            int dimSize = x[0].Length;

            double[][] SOA = new double[dimSize][];
            double[] M = new double[numAgents];
            for (int i = 0; i < numAgents;i++ )
            {
                M[i] += i;
            }
            for (int i = 0; i < dimSize; i++)
            {
                Tools.Shuffle<double>(ref M);
                SOA[i] = M.Clone() as double[];

            }
            for (int i = 0; i < numAgents; i++)
            {
                for (int j = 0; j < dimSize; j++)
                {
                    // Xi[j]=li+(ui-li)*SOA[i, j]/(M-1)
                    x[i][j] = lower[j] + (upper[j] - lower[j])*(SOA[j][i])/(numAgents - 1);
                }
            }
        }
        /// <summary>
        /// Simulated Orthogonal Array
        /// </summary>
        /// <param name="x"></param>
        /// <param name="m"></param>
        /// <param name="lower"></param>
        /// <param name="upper"></param>
        public static void InitializeSOA(ref double[,] x, double[] lower, double[] upper)
        {
            //TODO: Assert that lengths are > 0
            int numAgents = x.GetUpperBound(0);
            int dimSize = x.GetUpperBound(1);

            var SOA = new double[dimSize][];
            var M = new double[numAgents];
            for (int i = 0; i < numAgents; i++)
            {
                M[i] += i;
            }
            for (int i = 0; i < dimSize; i++)
            {
                Tools.Shuffle<double>(ref M);
                SOA[i] = M.Clone() as double[];

            }
            for (int i = 0; i < numAgents; i++)
            {
                for (int j = 0; j < dimSize; j++)
                {
                    // Xi[j]=li+(ui-li)*SOA[i, j]/(M-1)
                    x[i,j] = lower[j] + (upper[j] - lower[j]) * (SOA[j][i]) / (numAgents - 1);
                }
            }
        }
        public static void Shuffle<T>(ref T[] x)
        {
            //TODO: using system random for simplicity
            System.Random rand = new System.Random();
            int arrLen = x.Length;
            T temp;
            int dest;
            for(int i = 0; i< 7;i++) //Shuffle 7 times
            {
                for(int j = 0; j < arrLen; j++)
                {
                    dest = rand.Next(arrLen);
                    temp = x[dest];
                    x[dest] = x[j];
                    x[j] = temp;
                }
            }
        }
        /// <summary>
        /// Shuffle the first count elements
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="x"></param>
        /// <param name="count"></param>
        public static void Shuffle<T>(ref T[] x, int count)
        {
            //TODO: using system random for simplicity
            System.Random rand = new System.Random();
            int arrLen = count;
            T temp;
            int dest;
            for (int i = 0; i < 7; i++) //Shuffle 7 times
            {
                for (int j = 0; j < arrLen; j++)
                {
                    dest = rand.Next(arrLen);
                    temp = x[dest];
                    x[dest] = x[j];
                    x[j] = temp;
                }
            }
        }
    }
}
