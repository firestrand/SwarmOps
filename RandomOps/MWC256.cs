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
    /// Pseudo-Random Number Generator (PRNG) based on MWC256 by
    /// George Marsaglia. Period of this PRNG is about 2^8222.
    /// </summary>
    /// <remarks>
    /// This is a translation of the C source-code published 2003-05-13
    /// in the newsgroup comp.lang.c by George Marsaglia, published
    /// here with Marsaglia's authorization under the GNU LGPL license.
    /// </remarks>
    public class Mwc256 : RanUInt32Array
    {
        #region Constructors.
        /// <summary>
        /// Constructs the PRNG-object without a seed. Remember
        /// to seed it before drawing random numbers.
        /// </summary>
        public Mwc256()
        {
        }

        /// <summary>
        /// Constructs the PRNG-object using the designated seed.
        /// This is useful if you want to repeat experiments with the
        /// same sequence of pseudo-random numbers.
        /// </summary>
        public Mwc256(uint[] seed)
            : base(seed)
        {
        }

        /// <summary>
        /// Constructs the PRNG-object and uses another RNG for seeding.
        /// </summary>
        public Mwc256(Random rand)
            : base(rand)
        {
        }
        #endregion

        #region Internal definitions and variables
        /// <summary>
        /// Iterator array.
        /// </summary>
        readonly uint[] _q = new uint[256];

        /// <summary>
        /// Carry variable.
        /// </summary>
        uint _c;

        /// <summary>
        /// Iteration counter.
        /// </summary>
        byte _counter = 255;

        /// <summary>
        /// Is PRNG ready for use?
        /// </summary>
        bool _isReady;
        #endregion

        #region PRNG Implementation.
        /// <summary>
        /// Draw a random number in inclusive range {0, .., RandMax}
        /// </summary>
        public sealed override uint Rand()
        {
            Debug.Assert(_isReady);

            ulong t = (ulong)809430660 * _q[++_counter] + _c;

            _c = (uint)(t >> 32);

            uint retVal = (uint)t;
            _q[_counter] = retVal;

            return retVal;
        }

        /// <summary>
        /// The maximum possible value returned by Rand().
        /// </summary>
        public sealed override uint RandMax => uint.MaxValue;

        /// <summary>
        /// Length of seed-array.
        /// </summary>
        public sealed override int SeedLength => 257;

        /// <summary>
        /// Seed with an array.
        /// </summary>
        public sealed override void Seed(uint[] seed)
        {
            Debug.Assert(seed.Length == SeedLength);

            // First seed is used for C.
            _c = seed[0] % 809430660;

            // Remaining seeds are used for Q.
            for (int i = 1; i < SeedLength; i++)
            {
                _q[i-1] = seed[i];
            }

            _isReady = true;
        }
        #endregion

        #region Base-class overrides.
        /// <summary>
        /// Name of the RNG.
        /// </summary>
        public override string Name => "MWC256";

        #endregion
    }
}
