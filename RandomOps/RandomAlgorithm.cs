using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RandomOps
{
    public enum RandomAlgorithm
    {
        ByteStream,
        ByteStreamAsync,
        CMWC4096,
        CMWC4096ThreadSafe,
        KISS,
        MersenneTwister,
        MWC256,
        MWC256ThreadSafe,
        Ran2,
        RanQD,
        RanSystem,
        SumUInt32,
        Switcher,
        XorShift
    }
}
