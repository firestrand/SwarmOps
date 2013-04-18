using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using DotNetMatrix;

namespace SwarmOps.Problems
{
    public class NonNegativeMatrixFactorization : Problem
    {
        private readonly int _dimensionality;
        private readonly bool _quantization;

        public NonNegativeMatrixFactorization(int rowCountV, int rowCountH, double[] columnPackedV, bool quantization = false)
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

            _dimensionality = RowCountH * ColumnCountH + RowCountW * ColumnCountW;
            Quantizations = Enumerable.Repeat(1.0d, _dimensionality).ToArray();
            _lowerBound = Enumerable.Repeat(1.0d, _dimensionality).ToArray();
            _upperBound = Enumerable.Repeat(10.0d, _dimensionality).ToArray();

            _quantization = quantization;
        }
        public override string Name
        {
            get { return "Non Negative Matrix Factorization"; }
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
            if (_quantization)
            {
                //This problem uses integer values only. Round and enforce
                Quantize(x, Quantizations);
            }
            var packedW = new double[RowCountW * ColumnCountW];
            Array.Copy(x, 0, packedW, 0, packedW.Length);
            var packedH = new double[RowCountH * ColumnCountH];
            Array.Copy(x, packedW.Length, packedH, 0, packedH.Length);

            var v = new GeneralMatrix(ColumnPackedV, RowCountV);
            var h = new GeneralMatrix(packedH, RowCountH);
            var w = new GeneralMatrix(packedW, RowCountW);

            var result = v - (w * h);
            return result.NormF();







            
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
