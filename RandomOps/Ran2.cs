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
    /// Pseudo-Random Number Generator (PRNG) based on the Ran2 algorithm from the book:
    /// 'Numerical Recipes in C' chapter 7.1 and which is originally
    /// due to L'Ecuyer with Bays-Durham shuffle and added safeguards.
    /// Period is greater than 2 * 10^18.
    /// </summary>
    /// <remarks>
    /// We MUST use division when generating random integers in a certain range.
    /// Do NOT use bit-manipulation because the low-order bits of are not that random!
    /// This works by default because Uniform() is used for creating integers through
    /// the implementation of the Index() methods in the Random base-class.
    /// </remarks>
    public class Ran2 : RanInt32
    {
        #region Constructors.
        /// <summary>
        /// Constructs the PRNG-object and seeds the PRNG with the current time of day.
        /// This is what you will mostly want to use.
        /// </summary>
        public Ran2()
        {
            Seed();
        }

        /// <summary>
        /// Constructs the PRNG-object using the designated seed.
        /// This is useful if you want to repeat experiments with the
        /// same sequence of pseudo-random numbers.
        /// </summary>
        public Ran2(int seed)
        {
            Seed(seed);
        }
        #endregion

        #region Iterator-class (internal use only).
        class Iterator
        {
            readonly int _im;
            readonly int _ia;
            readonly int _iq;
            readonly int _ir;

            public int Idum
            {
                get;
                private set;
            }

            public Iterator(int im, int ia, int iq, int ir)
            {
                _im = im;
                _ia = ia;
                _iq = iq;
                _ir = ir;
            }

            public void Seed(int seed)
            {
                Idum = seed;
            }

            public int DoRand()
            {
                int k;

                k = Idum / _iq;

                Idum = _ia * (Idum - k * _iq) - _ir * k;

                if (Idum < 0)
                {
                    Idum += _im;
                }

                return Idum;
            }
        }
        #endregion

        #region Internal definitions and variables.
        static readonly int Im0 = 2147483563;
        static readonly int Im1 = 2147483399;
        static readonly int Ia0 = 40014;
        static readonly int Ia1 = 40692;
        static readonly int Iq0 = 53668;
        static readonly int Iq1 = 52774;
        static readonly int Ir0 = 12211;
        static readonly int Ir1 = 3791;
        static readonly int Ntab = 32;
        static readonly int Imm = Im0 - 1;
        static readonly int Ndiv = 1 + Imm / Ntab;
        static readonly int Warmup = 1024 + 8;
        static readonly int Warmup2 = 200;

        readonly Iterator[] _iterators = { new(Im0, Ia0, Iq0, Ir0), new(Im1, Ia1, Iq1, Ir1) };

        int _iy;
        readonly int[] _iv = new int[Ntab];

        /// <summary>
        /// Is PRNG ready for use?
        /// </summary>
        bool _isReady;
        #endregion

        #region PRNG Implementation.
        /// <summary>
        /// Draw a random number in inclusive range {0, .., RandMax}
        /// </summary>
        public sealed override int Rand()
        {
            Debug.Assert(_isReady);

            _iterators[0].DoRand();
            _iterators[1].DoRand();

            {
                // Will be in the range 0..NTAB-1.
                int j = _iy / Ndiv;

                Debug.Assert(j >= 0 && j < Ntab);

                // Idum is shuffled, idum0 and idum1 are
                // combined to generate output.
                _iy = _iv[j] - _iterators[1].Idum;
                _iv[j] = _iterators[0].Idum;

                if (_iy < 1)
                {
                    _iy += Imm;
                }
            }

            Debug.Assert(_iy >= 0 && _iy <= RandMax);

            return _iy;
        }

        /// <summary>
        /// The maximum possible value returned by Rand().
        /// </summary>
        public sealed override int RandMax => Im0 - 1;

        /// <summary>
        /// Seed with an integer.
        /// </summary>
        protected sealed override void Seed(int seed)
        {
            int j;

            // Ensure seed>0
            if (seed == 0)
            {
                seed = 1;
            }
            else if (seed < 0)
            {
                seed = -seed;
            }

            _iterators[0].Seed(seed);
            _iterators[1].Seed(seed);

            // Perform initial warm-ups.
            for (j = 0; j < Warmup; j++)
            {
                _iterators[0].DoRand();
            }

            for (j = Ntab - 1; j >= 0; j--)
            {
                _iv[j] = _iterators[0].DoRand();
            }

            _iy = _iv[0];

            // PRNG is now ready for use.
            _isReady = true;

            // Perform additional warm-ups.
            for (j = 0; j < Warmup2; j++)
            {
                Rand();
            }
        }
        #endregion

        #region Base-class overrides.
        /// <summary>
        /// Name of the RNG.
        /// </summary>
        public override string Name => "Ran2";

        #endregion
    }
}
