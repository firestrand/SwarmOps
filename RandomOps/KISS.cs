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
    /// Pseudo-Random Number Generator (PRNG) based on KISS as
    /// described in the paper: G. Marsaglia, Random Number Generators,
    /// Journal of Modern Applied Statistical Methods, 2003, vol. 2, no. 1,
    /// p. 2-13. Period of this PRNG is greater than 2^124.
    /// </summary>
    /// <remarks>
    /// This is a translation of the C source-code published in the
    /// paper cited above with Marsaglia's authorization, also under
    /// the GNU LPGL license.
    /// </remarks>
    public class Kiss : RanUInt32Array
    {
        #region Constructors.
        /// <summary>
        /// Constructs the PRNG-object without a seed. Remember
        /// to seed it before drawing random numbers.
        /// </summary>
        public Kiss()
        {
        }

        /// <summary>
        /// Constructs the PRNG-object using the designated seed.
        /// This is useful if you want to repeat experiments with the
        /// same sequence of pseudo-random numbers.
        /// </summary>
        public Kiss(uint[] seed)
            : base(seed)
        {
        }

        /// <summary>
        /// Constructs the PRNG-object and uses another RNG for seeding.
        /// </summary>
        public Kiss(Random rand)
            : base(rand)
        {
        }
        #endregion

        #region Public properties.
        /// <summary>
        /// Default seed.
        /// </summary>
        public static readonly uint[] SeedDefault = { 123456789, 362436000, 521288629, 7654321 };
        #endregion

        #region Internal definitions and variables
        /// <summary>
        /// Iterator variables.
        /// </summary>
        uint _x, _y, _z, _c;

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

            _x = 69069 * _x + 12345;

            _y ^= _y << 13;
            _y ^= _y >> 17;
            _y ^= _y << 5;

            ulong t = 698769069 * _z + _c;

            _c = (uint)(t >> 32);

            _z = (uint)t;

            uint retVal = _x + _y + _z;

            return retVal;
        }

        /// <summary>
        /// The maximum possible value returned by Rand().
        /// </summary>
        public sealed override uint RandMax => uint.MaxValue;

        /// <summary>
        /// Length of seed-array.
        /// </summary>
        public sealed override int SeedLength => 4;

        /// <summary>
        /// Seed with an array.
        /// </summary>
        public sealed override void Seed(uint[] seed)
        {
            Debug.Assert(seed.Length == SeedLength);

            _x = seed[0];
            _y = seed[1];
            _z = seed[2];
            _c = seed[3];

            _isReady = true;
        }
        #endregion

        #region Base-class overrides.
        /// <summary>
        /// Name of the RNG.
        /// </summary>
        public override string Name => "KISS";

        #endregion
    }
}
