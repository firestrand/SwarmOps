/// ------------------------------------------------------
/// RandomOps - (Pseudo) Random Number Generator For C#
/// Copyright (C) 2003-2010 Magnus Erik Hvass Pedersen.
/// Published under the GNU Lesser General Public License.
/// Please see the file license.txt for license details.
/// RandomOps on the internet: http://www.Hvass-Labs.org/
/// ------------------------------------------------------

namespace RandomOps
{
    /// <summary>
    /// Abstract class for using multiple RNGs that can be switched
    /// between. Example implementation is the Switcher-class which
    /// does the RNG-switching randomly.
    /// </summary>
    /// <remarks>
    /// If you are using RNGs that have custom methods for generating
    /// random numbers then you need to extend this class in a fashion
    /// similar to that of the Uniform()-method.
    /// </remarks>
    public abstract partial class Multi : Random
    {
        #region Constructor.
        /// <summary>
        /// Constructs the RNG-object from different RNG's.
        /// </summary>
        /// <param name="rands">The RNGs that will be switched between.</param>
        public Multi(Random[] rands)
        {
            Rands = rands;
        }
        #endregion

        #region Override these methods.
        /// <summary>
        /// Select the RNG to use.
        /// </summary>
        /// <returns>Index into the Rands-array.</returns>
        protected abstract int SelectRand();
        #endregion

        #region Internal variables.
        /// <summary>
        /// The array of RNGs to switch between.
        /// </summary>
        protected Random[] Rands
        {
            get;
        }

        /// <summary>
        /// The currently selected RNG.
        /// </summary>
        Random _rand;
        #endregion

        #region RNG Implementation.
        /// <summary>
        /// Switch the RNG currently being used. This is to be
        /// called before every RNG-method call.
        /// </summary>
        void Switch()
        {
            int selected = SelectRand();

            _rand = Rands[selected];
        }
        #endregion

        #region Base-class overrides.
        /// <summary>
        /// Name of the RNG.
        /// </summary>
        public override string Name
        {
            get
            {
                string s = "Multi(";

                foreach (Random rand in Rands)
                {
                    s += rand.Name + ", ";
                }

                s += ")";

                return s;
            }
        }

        /// <summary>
        /// Draw a uniform random number in the exclusive range (0,1)
        /// </summary>
        public sealed override double Uniform()
        {
            Switch();

            return _rand.Uniform();
        }

        /// <summary>
        /// Draw a random boolean with equal probability of drawing true or false.
        /// </summary>
        public sealed override bool Bool()
        {
            Switch();

            return _rand.Bool();
        }

        /// <summary>
        /// Draw a random and uniform byte.
        /// </summary>
        public sealed override byte Byte()
        {
            Switch();

            return _rand.Byte();
        }

        /// <summary>
        /// Draw an array of random and uniform bytes.
        /// </summary>
        /// <param name="length">The array length requested.</param>
        public sealed override byte[] Bytes(int length)
        {
            Switch();

            return _rand.Bytes(length);
        }
        #endregion
    }
}
