﻿/// ------------------------------------------------------
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
    /// Pseudo-Random Number Generator (PRNG) using the Mersenne Twister
    /// algorithm by Makoto Matsumoto and Takuji Nishimura. This implementation
    /// is rewritten from their C source-code originally dated 2002/1/26.
    /// This PRNG has a very long period of 2^19937-1 (approximately 4.3 x 10^6001),
    /// and is hence known as MT19937. This implementation is the 32-bit version.
    /// </summary>
    /// <remarks>
    /// The original C source-code contains the following copyright notice which
    /// still holds for this more or less direct translation to the C# language:
    /// 
    /// Copyright (C) 1997 - 2002, Makoto Matsumoto and Takuji Nishimura,
    /// All rights reserved.
    ///
    /// Redistribution and use in source and binary forms, with or without
    /// modification, are permitted provided that the following conditions
    /// are met:
    /// 
    /// 1. Redistributions of source code must retain the above copyright
    ///    notice, this list of conditions and the following disclaimer.
    ///
    /// 2. Redistributions in binary form must reproduce the above copyright
    ///    notice, this list of conditions and the following disclaimer in the
    ///    documentation and/or other materials provided with the distribution.
    ///
    /// 3. The names of its contributors may not be used to endorse or promote 
    ///    products derived from this software without specific prior written 
    ///    permission.
    ///
    /// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS
    /// "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT
    /// LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR
    /// A PARTICULAR PURPOSE ARE DISCLAIMED.  IN NO EVENT SHALL THE COPYRIGHT OWNER OR
    /// CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL,
    /// EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO,
    /// PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR
    /// PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF
    /// LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING
    /// NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS
    /// SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
    /// 
    /// Any feedback is very welcome.
    /// http://www.math.sci.hiroshima-u.ac.jp/~m-mat/MT/emt.html
    /// email: m-mat @ math.sci.hiroshima-u.ac.jp (remove space)
    /// </remarks>
    public class MersenneTwister : RanUInt32
    {
        #region Constructors.
        /// <summary>
        /// Constructs the PRNG-object and seeds the PRNG with the current time of day.
        /// This is what you will mostly want to use.
        /// </summary>
        public MersenneTwister()
        {
            Seed();
        }

        /// <summary>
        /// Constructs the PRNG-object using the designated seed.
        /// This is useful if you want to repeat experiments with the
        /// same sequence of pseudo-random numbers.
        /// </summary>
        public MersenneTwister(uint seed)
        {
            Seed(seed);
        }

        /// <summary>
        /// Constructs the PRNG-object using the designated array
        /// of seeds. Use this if you need to seed with more than 32 bits.
        /// </summary>
        public MersenneTwister(uint[] seeds)
        {
            Seed(seeds);
        }
        #endregion

        #region Internal definitions and variables
        static readonly uint N = 624;                     // Array-length.
        static readonly uint M = 397;
        static readonly uint MatrixA = 0x9908b0df;       // Constant vector a.
        static readonly uint UpperMask = 0x80000000;     // Most significant w-r bits.
        static readonly uint LowerMask = 0x7fffffff;     // Least significant r bits.
        static readonly uint[] Mag01 = { 0x0, MatrixA };

        readonly uint[] _mt = new uint[N];                        // The array for the state vector.
        uint _mti;                                         // Index into mt-array.

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

            uint y;

            if (_mti >= N)
            {
                // Generate N words.

                int kk;

                for (kk = 0; kk < N - M; kk++)
                {
                    y = (_mt[kk] & UpperMask) | (_mt[kk + 1] & LowerMask);
                    _mt[kk] = _mt[kk + M] ^ (y >> 1) ^ Mag01[y & 0x1];
                }

                for (; kk < N - 1; kk++)
                {
                    y = (_mt[kk] & UpperMask) | (_mt[kk + 1] & LowerMask);
                    _mt[kk] = _mt[kk + M - N] ^ (y >> 1) ^ Mag01[y & 0x1];
                }

                y = (_mt[N - 1] & UpperMask) | (_mt[0] & LowerMask);
                _mt[N - 1] = _mt[M - 1] ^ (y >> 1) ^ Mag01[y & 0x1];

                _mti = 0;
            }

            y = _mt[_mti++];

            /* Tempering */
            y ^= (y >> 11);
            y ^= (y << 7) & 0x9d2c5680;
            y ^= (y << 15) & 0xefc60000;
            y ^= (y >> 18);

            Debug.Assert(y <= RandMax);

            return y;
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
            _mt[0] = seed;

            for (_mti = 1; _mti < N; _mti++)
            {
                uint lcg = 1812433253;
                _mt[_mti] = (lcg * (_mt[_mti - 1] ^ (_mt[_mti - 1] >> 30)) + _mti);
            }

            _isReady = true;
        }

        /// <summary>
        /// Seed with an array of integers.
        /// </summary>
        protected void Seed(uint[] seeds)
        {
            Seed(19650218);

            uint i = 1;
            uint j = 0;
            uint k = (N > seeds.Length) ? (N) : ((uint)seeds.Length);

            for (; k > 0; k--)
            {
                // Non-linear.
                _mt[i] = (_mt[i] ^ ((_mt[i - 1] ^ (_mt[i - 1] >> 30)) * 1664525)) + seeds[j] + j;

                i++;
                j++;

                if (i >= N)
                {
                    _mt[0] = _mt[N - 1];
                    i = 1;
                }

                if (j >= seeds.Length)
                {
                    j = 0;
                }
            }

            for (k = N - 1; k > 0; k--)
            {
                // Non-linear.
                _mt[i] = (_mt[i] ^ ((_mt[i - 1] ^ (_mt[i - 1] >> 30)) * 1566083941)) - i;

                i++;

                if (i >= N)
                {
                    _mt[0] = _mt[N - 1];
                    i = 1;
                }
            }

            // MSB is 1; assuring non-zero initial array.
            _mt[0] = 0x80000000;
        }
        #endregion

        #region Base-class overrides.
        /// <summary>
        /// Name of the RNG.
        /// </summary>
        public override string Name => "MersenneTwister19937";

        #endregion
    }
}
