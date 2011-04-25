/// ------------------------------------------------------
/// SwarmOps - Numeric and heuristic optimization for C#
/// Copyright (C) 2003-2011 Magnus Erik Hvass Pedersen.
/// Please see the file license.txt for license details.
/// SwarmOps on the internet: http://www.Hvass-Labs.org/
/// ------------------------------------------------------

using System;

namespace SwarmOps
{
    public static partial class Tools
    {
        /// <summary>
        /// Allocate and return a new matrix double[dim1][dim2].
        /// </summary>
        public static double[][] NewMatrix(int dim1, int dim2)
        {
            double[][] matrix = new double[dim1][];

            for (int i = 0; i < dim1; i++)
            {
                matrix[i] = new double[dim2];
            }

            return matrix;
        }
        public static T[,] ArrayToMatrix<T>(T[] array, int m, int n)
        {
            var matrix = new T[m,n];
            for (int i = 0; i < m; i++)
            {
                for (int j = 0; j < n; j++)
                {
                    matrix[i, j] = array[i*n+j];
                }  
            }
            return matrix;
        }

        public static  bool ValuesEqual<T>(this T[,] matrix, T[,] compare)
        {
            if (matrix.Rank != compare.Rank || matrix.Length != compare.Length)
                return false;
            for (int i = 0; i <= matrix.GetUpperBound(0); i++)
            {
                for(int j = 0; j <= matrix.GetUpperBound(1); j++)
                {
                    if (!matrix[i, j].Equals(compare[i, j]))
                        return false;
                }
            }
            return true;
        }
    }
}
