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
    /// Pseudo-Random Number Generator (PRNG) based on XorShift as
    /// described in the paper: G. Marsaglia, Random Number Generators,
    /// Journal of Modern Applied Statistical Methods, 2003, vol. 2, no. 1,
    /// p. 2-13. Period of this PRNG is about 2^160.
    /// </summary>
    /// <remarks>
    /// This is a translation of the C source-code published 2003-05-13
    /// in the newsgroup comp.lang.c by George Marsaglia, published
    /// here with Marsaglia's authorization under the GNU LGPL license.
    /// </remarks>
    public class XorShift : RanUInt32Array
    {
        #region Constructors.
        /// <summary>
        /// Constructs the PRNG-object without a seed. Remember
        /// to seed it before drawing random numbers.
        /// </summary>
        public XorShift()
        {
        }

        /// <summary>
        /// Constructs the PRNG-object using the designated seed.
        /// This is useful if you want to repeat experiments with the
        /// same sequence of pseudo-random numbers.
        /// </summary>
        public XorShift(uint[] seed)
            : base(seed)
        {
        }

        /// <summary>
        /// Constructs the PRNG-object and uses another RNG for seeding.
        /// </summary>
        public XorShift(Random rand)
            : base(rand)
        {
        }
        #endregion

        #region Public properties.
        /// <summary>
        /// Default seed.
        /// </summary>
        public static readonly uint[] SeedDefault = { 123456789, 362436069, 521288629, 88675123, 886756453 };
        #endregion

        #region Internal definitions and variables
        /// <summary>
        /// Iterator variables.
        /// </summary>
        uint _x, _y, _z, _w, _v;

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

            uint t = (_x ^ (_x >> 7));

            _x=_y;
            _y=_z;
            _z=_w;
            _w=_v;
            
            _v = (_v ^ (_v << 6)) ^ (t ^ (t << 13));
            
            uint retVal = (_y+_y+1)*_v;

            return retVal;
        }

        /// <summary>
        /// The maximum possible value returned by Rand().
        /// </summary>
        public sealed override uint RandMax => uint.MaxValue;

        /// <summary>
        /// Length of seed-array.
        /// </summary>
        public sealed override int SeedLength => 5;

        /// <summary>
        /// Seed with an array.
        /// </summary>
        public sealed override void Seed(uint[] seed)
        {
            Debug.Assert(seed.Length == SeedLength);

            _x = seed[0];
            _y = seed[1];
            _z = seed[2];
            _w = seed[3];
            _v = seed[4];

            _isReady = true;
        }
        #endregion

        #region Base-class overrides.
        /// <summary>
        /// Name of the RNG.
        /// </summary>
        public override string Name => "XorShift";

        #endregion
    }
}
