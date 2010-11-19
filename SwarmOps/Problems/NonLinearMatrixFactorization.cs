using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace SwarmOps.Problems
{
    public class NonLinearMatrixFactorization : Problem
    {
        NonLinearMatrixFactorization():base()
        {
            Iterations = 20000;
            Quantizations = Enumerable.Repeat(1.0d, 4).ToArray();
        }
        public override string Name
        {
            get { return "Non Linear Matrix Factorization"; }
        }
        public override double Fitness(double[] x)
        {
            Debug.Assert(x != null && x.Length == Dimensionality);
            //This problem uses integer values only. Round and enforce
            Quantize(x, Quantizations);
            double x1 = x[0];
            double x2 = x[1];
            double x3 = x[2];
            double x4 = x[3];
            double f = 0.0D;
            return f * f;
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
            get { return 4; }
        }

        private readonly double[] _lowerBound = Enumerable.Repeat(0.0d, 4).ToArray();
        public override double[] LowerBound
        {
            get { return _lowerBound; }
        }

        private readonly double[] _upperBound = Enumerable.Repeat(100.0d, 4).ToArray();
        public override double[] UpperBound
        {
            get { return _upperBound; }
        }

    }
}
