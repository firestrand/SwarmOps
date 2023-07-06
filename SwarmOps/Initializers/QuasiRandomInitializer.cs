using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SwarmOps.Initializers
{
    public class QuasiRandomInitializer
    {
        public double Phi { get; private set; }

        public double Seed { get; private set; }

        public double[] Alpha { get; private set; }

        public QuasiRandomInitializer(int dimensions, double seed = 0.5)
        {
            double x = 2.0;
            for (int i = 0; i < dimensions; i++)
            {
                x = Math.Pow(1.0 + x, 1.0 / (dimensions + 1));
            }
            Phi = x;
            Seed = seed;
            Alpha = new double[dimensions];
            for (int i = 0; i < dimensions; i++)
            {
                Alpha[i] = Math.Pow(1.0 / Phi, i + 1) % 1.0;
            }
        }

        public void Initialize(ref double[] x, int n, double[] lower, double[] upper)
        {
            //validate x.Length == Alpha.Length
            if (x.Length != Alpha.Length)
            {
                throw new ArgumentException("x.Length != Alpha.Length");
            }

            for (int i = 0; i < x.Length; i++)
            {
                x[i] = lower[i] + (upper[i] - lower[i]) * (Seed + Alpha[i] * (n+1)) % 1.0;
            }
        }
    }
}