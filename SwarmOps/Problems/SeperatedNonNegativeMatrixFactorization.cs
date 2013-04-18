using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using DotNetMatrix;

namespace SwarmOps.Problems
{
    public class SeperatedNonNegativeMatrixFactorization : Problem
    {
        private readonly int _dimensionality;
        private readonly bool _quantize;

        public SeperatedNonNegativeMatrixFactorization(int rowCountV, int rowCountH, double[] columnPackedV, bool quantize = true)
        {
            if(rowCountV <=0 || rowCountH <= 0 || columnPackedV == null || columnPackedV.Length <= 0)
                throw new ArgumentException("Arguments invalid.");
            if (columnPackedV.Length % rowCountV != 0)
                throw new ArgumentOutOfRangeException("rowCountV");
            //Quantizations = Enumerable.Repeat(1.0d, columnPackedV.Length).ToArray();

            RowCountV = rowCountV;
            ColumnCountV = columnPackedV.Length/rowCountV;
            RowCountW = rowCountV;
            ColumnCountW = rowCountH;
            RowCountH = rowCountH;
            ColumnCountH = ColumnCountV;
            
            ColumnPackedV = columnPackedV;

            _dimensionality = RowCountW * ColumnCountW * ColumnCountH;
            Quantizations = Enumerable.Repeat(1.0d, _dimensionality).ToArray();
            _lowerBound = Enumerable.Repeat(1.0d, _dimensionality).ToArray();
            _upperBound = Enumerable.Repeat(100.0d, _dimensionality).ToArray();
            _quantize = quantize;
        }
        public override string Name
        {
            get { return "Non-Negative Matrix Factorization"; }
        }


        public int RowCountV { get; private set; }
        public int RowCountW { get; private set; }
        public int RowCountH { get; private set; }
        public int ColumnCountV { get; private set; }
        public int ColumnCountW { get; private set; }
        public int ColumnCountH { get; private set; }

        public double[] ColumnPackedV { get; private set; }

        public override double Fitness(double[] x)
        {

            Debug.Assert(x != null && x.Length == Dimensionality);
            if (_quantize)
            {
                //This problem uses integer values only. Round and enforce
                Quantize(x, Quantizations);
            }
            double[] packedW = new double[RowCountW * ColumnCountW];
            Array.Copy(x, 0, packedW, 0, packedW.Length);
            var packedH = new double[RowCountH * ColumnCountH];
            Array.Copy(x, packedW.Length, packedH, 0, packedH.Length);

            var v = new GeneralMatrix(ColumnPackedV, RowCountV);
            var wh = new GeneralMatrix[RowCountH];
            var whTemp = new GeneralMatrix(RowCountV, ColumnCountV, 0.0);
            for (int i = 0; i < RowCountH; i++)
            {
                wh[i] = new GeneralMatrix(x, RowCountV, ColumnCountV, i * RowCountV * ColumnCountV);
                whTemp = whTemp + wh[i];
            }
            


            var result = v - (whTemp);
            //RMSE
            double average = 0.0;
            for (int i = 0; i < RowCountV; i++)
            {
                average += result.Array[i].Average();
            }
            average = average / RowCountV;

            double rmse = 0.0;
            for (int i = 0; i < RowCountV; i++)
            {
                for (int j = 0; j < ColumnCountV; j++)
                {
                    rmse += Math.Sqrt(Math.Pow((average - result.Array[i][j]), 2.0));
                }
            }

            return rmse;
        }
        public override double[] Optimal
        {
            get
            {
                return base.Optimal;
            }
        }
        /// <summary>
        /// Threshold for an acceptable fitness value.
        /// </summary>
        public override double AcceptableFitness
        {
            get { return 1e-5d; }
        }

        public override double MinFitness
        {
            get { return 0.0d; }
        }

        public override int Dimensionality
        {
            get { return _dimensionality; }
        }

        private readonly double[] _lowerBound;// = Enumerable.Repeat(0.0d, 4).ToArray();
        public override double[] LowerBound
        {
            get { return _lowerBound; }
        }

        private readonly double[] _upperBound;// = Enumerable.Repeat(100.0d, 4).ToArray();
        public override double[] UpperBound
        {
            get { return _upperBound; }
        }

    }
}
