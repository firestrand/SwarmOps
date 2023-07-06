/// ------------------------------------------------------
/// RandomOps - (Pseudo) Random Number Generator For C#
/// Copyright (C) 2003-2010 Magnus Erik Hvass Pedersen.
/// Published under the GNU Lesser General Public License.
/// Please see the file license.txt for license details.
/// RandomOps on the internet: http://www.Hvass-Labs.org/
/// ------------------------------------------------------

using System;
using System.Diagnostics;

namespace RandomOps
{
    /// <summary>
    /// Pseudo-Random Number Generator (PRNG) base-class for a generator of UInt32 integers.
    /// </summary>
    /// <remarks>
    /// It is somewhat tricky to implement Index() using integer-operations and get the
    /// rounding right for all cases, so we reuse the base-class implementation which
    /// indirectly uses Uniform().
    /// </remarks>
    public abstract class RanInt32 : Random
    {
        #region Constructors.
        /// <summary>
        /// Constructs the PRNG-object.
        /// </summary>
        public RanInt32()
        {
            _randMaxHalf = RandMax / 2;
            _randInv = 1.0/((double)RandMax + 2);
        }
        #endregion

        #region Internal variables.
        /// <summary>
        /// Used in Bool(), for convenience and speed.
        /// </summary>
        readonly int _randMaxHalf;

        /// <summary>
        /// Used in Uniform(), for convenience and speed.
        /// </summary>
        readonly double _randInv;
        #endregion

        #region PRNG Implementation.
        /// <summary>
        /// Draw a random number in inclusive range {0, .., RandMax}
        /// </summary>
        public abstract int Rand();

        /// <summary>
        /// The maximum possible value returned by Rand().
        /// </summary>
        public abstract int RandMax
        {
            get;
        }

        /// <summary>
        /// Seed with the time of day.
        /// </summary>
        protected void Seed()
        {
            int seed = (int)(DateTime.Now.Ticks % RandMax);

            Seed(seed);
        }

        /// <summary>
        /// Seed with an integer.
        /// </summary>
        protected abstract void Seed(int seed);
        #endregion

        #region Base-class overrides.
        /// <summary>
        /// Draw a uniform random number in the exclusive range (0,1)
        /// </summary>
        /// <remarks>
        /// Assumes that Rand() is in {0, .., RandMax}.
        /// </remarks>
        public override double Uniform()
        {
            double rand = (double)Rand() + 1;
            double value = rand * _randInv;

            Debug.Assert(value is > 0 and < 1);

            return value;
        }

        /// <summary>
        /// Draw a random boolean with equal probability of true or false.
        /// </summary>
        public override bool Bool()
        {
            return Rand() < _randMaxHalf;
        }

        /// <summary>
        /// Draw a random and uniform byte.
        /// </summary>
        /// <remarks>
        /// The least significant bits are not that statistically random,
        /// hence we must use the most significant bits by a bit-shift.
        /// </remarks>
        public override byte Byte()
        {
            int r = Rand();
            int value = r >> 23;

            Debug.Assert(value is >= 0 and <= byte.MaxValue);

            byte b = (byte)value;

            return b;
        }
        #endregion
    }
}
