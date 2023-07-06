/// ------------------------------------------------------
/// RandomOps - (Pseudo) Random Number Generator For C#
/// Copyright (C) 2003-2010 Magnus Erik Hvass Pedersen.
/// Published under the GNU Lesser General Public License.
/// Please see the file license.txt for license details.
/// RandomOps on the internet: http://www.Hvass-Labs.org/
/// ------------------------------------------------------

using System;

namespace RandomOps
{
    /// <summary>
    /// Pseudo-Random Number Generator (PRNG) base-class for a generator of UInt32 integers
    /// that uses an array.
    /// </summary>
    public abstract class RanUInt32Array : RanUInt32
    {
        #region Constructors.
        /// <summary>
        /// Constructs the PRNG-object.
        /// </summary>
        public RanUInt32Array()
        {
        }

        /// <summary>
        /// Constructs the PRNG-object using the designated seed.
        /// This is useful if you want to repeat experiments with the
        /// same sequence of pseudo-random numbers.
        /// </summary>
        public RanUInt32Array(uint[] seed)
        {
            Seed(seed);
        }

        /// <summary>
        /// Constructs the PRNG-object using another PRNG-object
        /// for seeding.
        /// </summary>
        public RanUInt32Array(Random rand)
        {
            Seed(rand);
        }
        #endregion

        #region Seed.
        /// <summary>
        /// Length of seed-array.
        /// </summary>
        public abstract int SeedLength
        {
            get;
        }

        /// <summary>
        /// Seed with an array.
        /// </summary>
        public abstract void Seed(uint[] seed);

        /// <summary>
        /// Seed with random bytes from another RNG.
        /// </summary>
        public void Seed(Random rand)
        {
            uint[] seed = new uint[SeedLength];

            for (int i = 0; i < seed.Length; i++)
            {
                byte[] b = rand.Bytes(4);
                seed[i] = BitConverter.ToUInt32(b, 0);
            }

            Seed(seed);
        }
        #endregion

        #region Base-class overrides.
        /// <summary>
        /// Seed with an integer.
        /// </summary>
        protected sealed override void Seed()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Seed with an integer.
        /// </summary>
        protected sealed override void Seed(uint seed)
        {
            throw new NotImplementedException();
        }
        #endregion
    }
}
