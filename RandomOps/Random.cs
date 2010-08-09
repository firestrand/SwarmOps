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
    /// Base-class for an RNG.
    /// </summary>
    /// <remarks>
    /// You must at least implement the Uniform() method, which is
    /// used by default by all the other methods that can be overrided
    /// to provide more efficient implementations for your particular RNG.
    /// </remarks>
    public abstract partial class Random
    {
        /// <summary>
        /// Name of the RNG.
        /// </summary>
        public abstract string Name
        {
            get;
        }

        /// <summary>
        /// Draw a uniform random number in the exclusive range (0,1).
        /// </summary>
        /// <remarks>
        /// This must be implemented in every inherited class. It is important
        /// that it does not return the end-point values of zero or one, or
        /// else some of the methods which use Uniform() to generate their
        /// random numbers will not work.
        /// </remarks>
        public abstract double Uniform();

        /// <summary>
        /// Draw a uniform random number in the exclusive range (low,high).
        /// Thread-safe if Uniform() is thread-safe.
        /// </summary>
        /// <param name="low">The lowest value in the range.</param>
        /// <param name="high">The highest value in the range.</param>
        public virtual double Uniform(double low, double high)
        {
            // Re-use the paremeter-less Uniform() method.

            return Uniform() * (high - low) + low;
        }

        /// <summary>
        /// Draw a random boolean with equal probability of true or false.
        /// Thread-safe if Uniform() is thread-safe.
        /// </summary>
        public virtual bool Bool()
        {
            // Re-use the Uniform() method.
            return Uniform() < 0.5;
        }

        /// <summary>
        /// Draw a random boolean with probability p of true and (1-p) of false.
        /// Thread-safe if Uniform() is thread-safe.
        /// </summary>
        public virtual bool Bool(double p)
        {
            // Re-use the Uniform() method.
            return Uniform() < p;
        }

        /// <summary>
        /// Draw a random and uniform byte.
        /// Thread-safe if Index() is thread-safe.
        /// </summary>
        /// <remarks>
        /// The default implementation uses Index() which in turn uses
        /// Uniform(). This is quite expensive for generating just
        /// one byte, but is the most generally viable way of doing it
        /// for many PRNGs, because their low-order bits are not very
        /// random. Some PRNGs may offer much better ways of generating
        /// just a single random byte, in which case this method should
        /// of course be overrided.
        /// </remarks>
        public virtual byte Byte()
        {
            // Re-use the Index() method.
            return (byte)Index((int)System.Byte.MaxValue + 1);
        }

        /// <summary>
        /// Draw an array of random and uniform bytes.
        /// Thread-safe if Byte() is thread-safe.
        /// </summary>
        /// <param name="length">The array length requested.</param>
        public virtual byte[] Bytes(int length)
        {
             // Re-use the Byte() method.

            byte[] arr = new byte[length];

            for (int i = 0; i < length; i++)
            {
                arr[i] = Byte();
            }

            return arr;
        }
        public static Random GetNewInstance(RandomAlgorithm algorithm)
        {
            switch (algorithm)
            {
                case RandomAlgorithm.CMWC4096:
                    return new CMWC4096();
                case RandomAlgorithm.CMWC4096ThreadSafe:
                    return new ThreadSafe.CMWC4096();
                case RandomAlgorithm.KISS:
                    return new KISS();
                case RandomAlgorithm.MersenneTwister:
                    return new MersenneTwister();
                case RandomAlgorithm.MWC256:
                    return new MWC256();
                case RandomAlgorithm.MWC256ThreadSafe:
                    return new ThreadSafe.MWC256();
                case RandomAlgorithm.Ran2:
                    return new Ran2();
                case RandomAlgorithm.RanQD:
                    return new RanQD();
                case RandomAlgorithm.RanSystem:
                    return new RanSystem();
                case RandomAlgorithm.XorShift:
                    return new XorShift();
                default:
                    return new RanSystem();
            }
        }
        public static Random GetNewInstance(RandomAlgorithm algorithm, Random random)
        {
            switch (algorithm)
            {
                case RandomAlgorithm.CMWC4096:
                    return new CMWC4096(random);
                case RandomAlgorithm.CMWC4096ThreadSafe:
                    return new ThreadSafe.CMWC4096();
                case RandomAlgorithm.KISS:
                    return new KISS(random);
                case RandomAlgorithm.MersenneTwister:
                    return new MersenneTwister(Convert.ToUInt32(random.Bytes(4)));
                case RandomAlgorithm.MWC256:
                    return new MWC256(random);
                case RandomAlgorithm.MWC256ThreadSafe:
                    return new ThreadSafe.MWC256();
                case RandomAlgorithm.Ran2:
                    return new Ran2(Convert.ToInt32(random.Bytes(4)));
                case RandomAlgorithm.RanQD:
                    return new RanQD(Convert.ToUInt32(random.Bytes(4)));
                case RandomAlgorithm.RanSystem:
                    return new RanSystem(Convert.ToInt32(random.Bytes(4)));
                case RandomAlgorithm.XorShift:
                    return new XorShift(random);
                default:
                    return new RanSystem(Convert.ToInt32(random.Bytes(4)));
            }
        }
        public static Int32 GetInt32FromBytes(byte[] bytes)
        {
            if(bytes.Length != 4)
                throw new ArgumentOutOfRangeException("bytes","Method requires 4 bytes to convert.");
            Int32 retVal = 0;
            for (int i = 0; i < bytes.Length; i++)
            {
                retVal |= bytes[i] << i*8;
            }
            return retVal;
        }
        public static UInt32 GetUInt32FromBytes(byte[] bytes)
        {
            if (bytes.Length != 4)
                throw new ArgumentOutOfRangeException("bytes", "Method requires 4 bytes to convert.");
            UInt32 retVal = 0;
            for (int i = 0; i < bytes.Length; i++)
            {
                retVal |= (UInt32) bytes[i] << i * 8;
            }
            return retVal;
        }
    }
}
