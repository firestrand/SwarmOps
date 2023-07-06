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
    /// Summing the output of multiple RNGs and taking modulo 2^32.
    /// Note that this assumes the RNGs have RandMax roughly equal
    /// to UInt32.MaxValue otherwise there will be a bias.
    /// </summary>
    /// <remarks>
    /// If you are using RNGs that have custom methods for generating
    /// random numbers then you need to extend this class in a fashion
    /// similar to that of the Uniform()-method.
    /// </remarks>
    public partial class SumUInt32 : RanUInt32
    {
        #region Constructor.
        /// <summary>
        /// Constructs the RNG-object from different RNG's.
        /// </summary>
        public SumUInt32(RanUInt32[] rands)
        {
            Rands = rands;
        }
        #endregion

        #region Internal variables.
        /// <summary>
        /// The array of RNGs to sum.
        /// </summary>
        protected RanUInt32[] Rands
        {
            get;
        }
        #endregion

        #region Base-class overrides.
        /// <summary>
        /// Name of the RNG.
        /// </summary>
        public sealed override string Name
        {
            get
            {
                string s = "Sum(";

                foreach (Random rand in Rands)
                {
                    s += rand.Name + ", ";
                }

                s += ")";

                return s;
            }
        }

        /// <summary>
        /// Draw a random number in inclusive range {0, .., RandMax}
        /// </summary>
        public sealed override uint Rand()
        {
            uint sum = 0;

            // Sum and modulo.
            foreach (RanUInt32 rand in Rands)
            {
                sum += rand.Rand();
            }

            return sum;
        }

        /// <summary>
        /// The maximum possible value returned by Rand().
        /// </summary>
        public sealed override uint RandMax => uint.MaxValue;

        #endregion
    }
}
