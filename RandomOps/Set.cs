﻿/// ------------------------------------------------------
/// RandomOps - (Pseudo) Random Number Generator For C#
/// Copyright (C) 2003-2010 Magnus Erik Hvass Pedersen.
/// Published under the GNU Lesser General Public License.
/// Please see the file license.txt for license details.
/// RandomOps on the internet: http://www.Hvass-Labs.org/
/// ------------------------------------------------------

using System.Diagnostics;

namespace RandomOps
{
    /// <summary>
    /// A set of integers enumerated from zero and upwards that
    /// can be drawn at random. Creation or resetting takes time
    /// O(n) where n is the size of the set, and drawing a random
    /// element from the set takes constant time O(1).
    /// </summary>
    public class Set
    {
        #region Public properties.
        /// <summary>
        /// Number of elements available for drawing.
        /// </summary>
        public int Size
        {
            get;
            private set;
        }

        /// <summary>
        /// Maximum number of elements in the set.
        /// </summary>
        public int Capacity
        {
            get;
        }
        #endregion

        #region Constructor.
        /// <summary>
        /// Creates but does not initialize the random set.
        /// </summary>
        /// <param name="rand">RNG object to use for random drawing.</param>
        /// <param name="capacity">Number of elements in the set.</param>
        public Set(Random rand, int capacity)
        {
            _rand = rand;

            _elms = new int[capacity];
            Capacity = capacity;
            Size = 0;
        }
        #endregion

        #region Internal variables.
        /// <summary>
        /// The RNG used for drawing elements from the set.
        /// </summary>
        readonly Random _rand;

        /// <summary>
        /// Array used for holding the elements of the set.
        /// </summary>
        readonly int[] _elms;
        #endregion

        #region Internal methods.
        /// <summary>
        /// Swap the i'th and j'th elements of the internal array.
        /// </summary>
        void Swap(int i, int j)
        {
            Debug.Assert(i >= 0 && i < Capacity);
            Debug.Assert(j >= 0 && j < Capacity);

            int temp = _elms[i];
            _elms[i] = _elms[j];
            _elms[j] = temp;
        }

        /// <summary>
        /// Remove an element from the set.
        /// </summary>
        /// <param name="index">Position of element to be removed.</param>
        void Remove(int index)
        {
            // Various assumptions.
            Debug.Assert(Size >= 1);
            Debug.Assert(index >= 0 && index < Size);

            // Decrease the number of elements in the set.
            Size--;

            // Swap element to be removed with the back of the array.
            Swap(index, Size);
        }
        #endregion

        #region Public methods.
        /// <summary>
        /// Draw and remove an element from the set.
        /// </summary>
        /// <returns>Randomly picked element.</returns>
        public int Draw()
        {
            // Local variables to be used.
            int index;
            int retVal;

            // Various assumptions.
            Debug.Assert(Size > 0);

            // Get random index from remainder of set.
            index = _rand.Index(Size);

            // Retrieve the element at that position.
            retVal = _elms[index];

            // Remove that element from the set.
            Remove(index);

            return retVal;
        }

        /// <summary>
        /// Reset by filling the set with integer elements enumerated
        /// from zero and upwards.
        /// </summary>
        public void Reset()
        {
            // Reset index-variable.
            Size = Capacity;

            // Initialize array.
            for (int i = 0; i < Size; i++)
            {
                _elms[i] = i;
            }
        }

        /// <summary>
        /// Same as Reset() only that it is being followed by a removal
        /// of the designated element.
        /// </summary>
        /// <param name="index">Element to be removed after resetting.</param>
        public void ResetExclude(int index)
        {
            Debug.Assert(index >= 0 && index < Capacity);

            Reset();
            Remove(index);
        }
        #endregion
    }
}
