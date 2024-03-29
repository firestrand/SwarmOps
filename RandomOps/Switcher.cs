﻿/// ------------------------------------------------------
/// RandomOps - (Pseudo) Random Number Generator For C#
/// Copyright (C) 2003-2010 Magnus Erik Hvass Pedersen.
/// Published under the GNU Lesser General Public License.
/// Please see the file license.txt for license details.
/// RandomOps on the internet: http://www.Hvass-Labs.org/
/// ------------------------------------------------------

namespace RandomOps
{
    /// <summary>
    /// Randomly switch between different RNGs.
    /// </summary>
    /// <remarks>
    /// This basically just implements the SelectRand() method
    /// from the RandMulti-class.
    /// </remarks>
    public class Switcher : Multi
    {
        #region Constructor.
        /// <summary>
        /// Constructs the RNG-object from other RNGs.
        /// </summary>
        /// <param name="randSwitch">The RNG that will be used to determine switching.</param>
        /// <param name="rands">The RNGs to switch between.</param>
        public Switcher(Random randSwitch, Random[] rands)
            : base(rands)
        {
            _randSwitch = randSwitch;
        }
        #endregion

        #region Internal variables.
        /// <summary>
        /// The RNG used to determine when to switch.
        /// </summary>
        readonly Random _randSwitch;
        #endregion

        #region Base-class overrides.
        /// <summary>
        /// Name of the RNG.
        /// </summary>
        public override string Name => "Switcher-" + base.Name;

        /// <summary>
        /// Determine which RNG in RandMulti to use.
        /// </summary>
        /// <returns>Index for the RNG.</returns>
        protected override int SelectRand()
        {
            return _randSwitch.Index(Rands.Length);
        }
        #endregion
    }
}
