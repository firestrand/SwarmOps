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
    /// Pseudo-Random Number Generator (PRNG) based on the RanQD1 (Quick and Dirty)
    /// algorithm from the book: 'Numerical Recipes in C' chapter 7.1.
    /// </summary>
    public class RanQd : RanUInt32
    {
        #region Constructors.
        /// <summary>
        /// Constructs the PRNG-object and seeds the PRNG with the current time of day.
        /// This is what you will mostly want to use.
        /// </summary>
        public RanQd()
        {
            Seed();
        }

        /// <summary>
        /// Constructs the PRNG-object using the designated seed.
        /// This is useful if you want to repeat experiments with the
        /// same sequence of pseudo-random numbers.
        /// </summary>
        public RanQd(uint seed)
        {
            Seed(seed);
        }
        #endregion

        #region Internal definitions and variables
        static readonly uint L1 = 1664525;
        static readonly uint L2 = 1013904223;

        /// <summary>
        /// Is PRNG ready for use?
        /// </summary>
        bool _isReady;

        /// <summary>
        /// Iterator-variable.
        /// </summary>
        uint _iter;
        #endregion

        #region PRNG Implementation.
        /// <summary>
        /// Draw a random number in inclusive range {0, .., RandMax}
        /// </summary>
        public sealed override uint Rand()
        {
            Debug.Assert(_isReady);

            _iter = L1 * _iter + L2;

            Debug.Assert(_iter >= 0 && _iter <= RandMax);

            return _iter;
        }

        /// <summary>
        /// The maximum possible value returned by Rand().
        /// </summary>
        public sealed override uint RandMax => uint.MaxValue;

        /// <summary>
        /// Seed with an integer.
        /// </summary>
        protected sealed override void Seed(uint seed)
        {
            _iter = seed;
            _isReady = true;
        }
        #endregion

        #region Base-class overrides.
        /// <summary>
        /// Name of the RNG.
        /// </summary>
        public override string Name => "RanQD";

        #endregion
    }
}
