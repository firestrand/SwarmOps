/// ------------------------------------------------------
/// RandomOps - (Pseudo) Random Number Generator For C#
/// Copyright (C) 2003-2010 Magnus Erik Hvass Pedersen.
/// Please see the file license.txt for license details.
/// RandomOps on the internet: http://www.Hvass-Labs.org/
/// ------------------------------------------------------

using System;
using System.Diagnostics;
using System.Threading;

namespace Test
{
    /// <summary>
    /// Test program for RandomOps.
    /// </summary>
    class Program
    {
        static readonly bool TestByte = false;
        static readonly bool TestBool = false;

        static readonly int NumIterations = 10;

        static readonly int NumBytes = 9;

        static readonly double GaussMean = 0;
        static readonly double GaussDeviation = 1;

        static readonly int ByteIterations = (Byte.MaxValue + 1) * 50000;

        static readonly int MeanIterations = 10000;

        static readonly int SphereDim = 10;
        static readonly double SphereRadius = 1;

        static readonly int BoolIterations = 10000;

        static readonly int IndexIterations = 10000;
        static readonly int MaxIndex = 8;
        static int[] IndexCounts = new int[MaxIndex];

        static readonly int RandSetSize = 8;
        static readonly int SetIterations = 8;
        static readonly int RandSetExclude = RandSetSize / 2;
        static readonly int SetIterations2 = 10000;
        static int[] SetCounts = new int[RandSetSize];

        static readonly int SetDistributedSize = 5;
        static readonly int IndexDistributionIterations = 100000;
        static double[] IndexProbabilities = new double[SetDistributedSize];
        static int[] IndexDistributionCounts = new int[SetDistributedSize];

        static void PrintPoint(double[] x)
        {
            double norm = 0;
            for (int i = 0; i < x.Length; i++)
            {
                Console.WriteLine(x[i]);
                norm += x[i] * x[i];
            }
            Console.WriteLine("Norm: {0}", Math.Sqrt(norm));
        }

        static void ZeroCounts(int[] counts)
        {
            for (int i = 0; i < counts.Length; i++)
            {
                counts[i] = 0;
            }
        }

        static void PrintCounts(int[] counts)
        {
            for (int i = 0; i < counts.Length; i++)
            {
                Console.WriteLine("Index: {0} Count: {1}", i, counts[i]);
            }
        }

        static void PrintDistributionCounts()
        {
            for (int i = 0; i < IndexDistributionCounts.Length; i++)
            {
                Console.WriteLine("Index: {0} Probability: {1:0.0000} Frequency: {2:0.0000}", i, IndexProbabilities[i], (double)IndexDistributionCounts[i] / IndexDistributionIterations);
            }
        }

        static void InitSetProbabilities(RandomOps.Random Rand)
        {
            int i;
            double sum = 0;

            // Initialize probabilities to random values.
            for (i = 0; i < IndexProbabilities.Length; i++)
            {
                double r = Rand.Uniform();
                IndexProbabilities[i] = r;
                sum += r;
            }

            // Normalize so sum of probabilities is one.
            for (i = 0; i < IndexProbabilities.Length; i++)
            {
                IndexProbabilities[i] /= sum;
            }
        }

        static void DoTest(RandomOps.Random Rand)
        {
            int i;

            Console.WriteLine("RNG name: {0}", Rand.Name);
            Console.WriteLine();

            if (TestByte)
            {
                Console.WriteLine("Byte()");
                int[] ByteCounts = new int[Byte.MaxValue + 1];
                ZeroCounts(ByteCounts);
                for (i = 0; i < ByteIterations; i++)
                {
                    ByteCounts[Rand.Byte()] += 1;
                }
                PrintCounts(ByteCounts);
                Console.WriteLine();
            }

            Console.WriteLine("Bytes({0})", NumBytes);
            byte[] byteArr = Rand.Bytes(NumBytes);
            for (i = 0; i < NumBytes; i++)
            {
                Console.WriteLine(byteArr[i]);
            }
            Console.WriteLine();

            Console.WriteLine("Uniform()");
            for (i = 0; i < NumIterations; i++)
            {
                Console.WriteLine(Rand.Uniform());
            }
            Console.WriteLine();

            Console.WriteLine("Uniform(-3, -1)");
            for (i = 0; i < NumIterations; i++)
            {
                Console.WriteLine(Rand.Uniform(-3, -1));
            }
            Console.WriteLine();

            Console.WriteLine("Uniform(-2, 2)");
            for (i = 0; i < NumIterations; i++)
            {
                Console.WriteLine(Rand.Uniform(-2, 2));
            }
            Console.WriteLine();

            Console.WriteLine("Uniform(3, 5)");
            for (i = 0; i < NumIterations; i++)
            {
                Console.WriteLine(Rand.Uniform(3, 5));
            }
            Console.WriteLine();

            Console.WriteLine("Gauss({0}, {1})", GaussMean, GaussDeviation);
            for (i = 0; i < NumIterations; i++)
            {
                Console.WriteLine(Rand.Gauss(GaussMean, GaussDeviation));
            }
            Console.WriteLine();

            double sum = 0;
            for (i = 0; i < MeanIterations; i++)
            {
                sum += Rand.Uniform();
            }
            Console.WriteLine("Mean of {0} x Uniform(): {1:0.0000}", MeanIterations, sum / MeanIterations);
            Console.WriteLine();

            sum = 0;
            for (i = 0; i < MeanIterations; i++)
            {
                sum += Rand.Gauss(0, 1);
            }
            Console.WriteLine("Mean of {0} x Gauss(0, 1): {1:0.0000}", MeanIterations, sum / MeanIterations);
            Console.WriteLine();

            Console.WriteLine("Disk()");
            PrintPoint(Rand.Disk());
            Console.WriteLine();

            Console.WriteLine("Circle()");
            PrintPoint(Rand.Circle());
            Console.WriteLine();

            Console.WriteLine("Sphere3()");
            PrintPoint(Rand.Sphere3());
            Console.WriteLine();

            Console.WriteLine("Sphere4()");
            PrintPoint(Rand.Sphere4());
            Console.WriteLine();

            Console.WriteLine("Sphere({0}, {1})", SphereDim, SphereRadius);
            PrintPoint(Rand.Sphere(SphereDim, SphereRadius));
            Console.WriteLine();

            if (TestBool)
            {
                Console.WriteLine("Bool()");
                int countTrue = 0;
                int countFalse = 0;
                for (i = 0; i < BoolIterations; i++)
                {
                    if (Rand.Bool())
                    {
                        countTrue++;
                    }
                    else
                    {
                        countFalse++;
                    }
                }
                Console.WriteLine("True: {0}", countTrue);
                Console.WriteLine("False: {0}", countFalse);
                Console.WriteLine();
            }

            Console.WriteLine("Index({0})", MaxIndex);
            ZeroCounts(IndexCounts);
            for (i = 0; i < IndexIterations; i++)
            {
                int idx = Rand.Index(MaxIndex);

                IndexCounts[idx] += 1;
            }
            PrintCounts(IndexCounts);
            Console.WriteLine();

            Console.WriteLine("Index2({0}, ...)", MaxIndex);
            ZeroCounts(IndexCounts);
            for (i = 0; i < IndexIterations; i++)
            {
                int idx1, idx2;

                Rand.Index2(MaxIndex, out idx1, out idx2);

                Debug.Assert(idx1 != idx2);

                IndexCounts[idx1] += 1;
                IndexCounts[idx2] += 1;
            }
            PrintCounts(IndexCounts);
            Console.WriteLine();

            RandomOps.Set RandSet = new RandomOps.Set(Rand, RandSetSize);

            Console.WriteLine("RandSet.Reset()");
            RandSet.Reset();
            Console.WriteLine();

            Console.WriteLine("RandSet.Draw() with set of size {0} and {1} iterations", RandSetSize, SetIterations / 2);
            for (i = 0; i < SetIterations / 2; i++)
            {
                Console.WriteLine(RandSet.Draw());
            }
            Console.WriteLine();

            Console.WriteLine("RandSet.Reset()");
            Console.WriteLine("RandSet.Draw() with set of size {0}", RandSetSize);
            RandSet.Reset();
            for (i = 0; i < SetIterations; i++)
            {
                Console.WriteLine(RandSet.Draw());
            }
            Console.WriteLine();

            //RandSet.Draw(); // Assertion fails.

            Console.WriteLine("RandSet.ResetExclude({0})", RandSetExclude);
            Console.WriteLine("RandSet.Draw() with set of size {0}", RandSetSize);
            RandSet.ResetExclude(RandSetExclude);
            for (i = 0; i < SetIterations - 1; i++)
            {
                Console.WriteLine(RandSet.Draw());
            }
            Console.WriteLine();

            //RandSet.Draw(); // Assertion fails.

            ZeroCounts(SetCounts);
            Console.WriteLine("RandSet.Draw() with set of size {0}", RandSetSize);
            for (i = 0; i < SetIterations2; i++)
            {
                RandSet.Reset();

                while (RandSet.Size > 0)
                {
                    int idx = RandSet.Draw();

                    SetCounts[idx] += 1;
                }
            }
            PrintCounts(SetCounts);
            Console.WriteLine();

            InitSetProbabilities(Rand);

            ZeroCounts(IndexDistributionCounts);
            Console.WriteLine("Rand.Index() with probability distribution");
            for (i = 0; i < IndexDistributionIterations; i++)
            {
                int idx = Rand.Index(IndexProbabilities);

                IndexDistributionCounts[idx] += 1;
            }
            PrintDistributionCounts();
            Console.WriteLine();

            RandomOps.IndexDistribution IndexDistribution = new RandomOps.IndexDistribution(Rand, IndexProbabilities);

            ZeroCounts(IndexDistributionCounts);
            Console.WriteLine("IndexDistribution.DrawLinearSearch()");
            for (i = 0; i < IndexDistributionIterations; i++)
            {
                int idx = IndexDistribution.DrawLinearSearch();

                IndexDistributionCounts[idx] += 1;
            }
            PrintDistributionCounts();
            Console.WriteLine();

            ZeroCounts(IndexDistributionCounts);
            Console.WriteLine("IndexDistribution.DrawBinarySearch()");
            for (i = 0; i < IndexDistributionIterations; i++)
            {
                int idx = IndexDistribution.DrawBinarySearch();

                IndexDistributionCounts[idx] += 1;
            }
            PrintDistributionCounts();
            Console.WriteLine();
        }

        static void Test(RandomOps.Random rand)
        {
            DateTime t1 = DateTime.Now;
            DoTest(rand);
            DateTime t2 = DateTime.Now;

            Console.WriteLine("Time usage: {0}", t2.Subtract(t1));
        }

        static void TestRanQD()
        {
            RandomOps.RanQD rand = new RandomOps.RanQD();

            Test(rand);
        }

        static void TestRan2()
        {
            RandomOps.Ran2 rand = new RandomOps.Ran2();

            Test(rand);
        }

        static void TestMersenneTwister()
        {
            RandomOps.MersenneTwister rand = new RandomOps.MersenneTwister();

            Test(rand);
        }

        static void TestArraySeed(RandomOps.RanUInt32Array rand)
        {
            RandomOps.Ran2 ran2 = new RandomOps.Ran2();
            RandomOps.RandomDotOrg randInternet = new RandomOps.RandomDotOrg(rand.SeedLength, ran2, rand.SeedLength);

            rand.Seed(randInternet);

            Test(rand);
        }

        static void TestXorShift()
        {
            RandomOps.XorShift rand = new RandomOps.XorShift();

            TestArraySeed(rand);
        }

        static void TestMWC256()
        {
            RandomOps.MWC256 rand = new RandomOps.MWC256();

            TestArraySeed(rand);
        }

        static void TestCMWC4096()
        {
            RandomOps.CMWC4096 rand = new RandomOps.CMWC4096();

            TestArraySeed(rand);
        }

        static void TestKISS()
        {
            RandomOps.KISS rand = new RandomOps.KISS();

            TestArraySeed(rand);
        }

        static void TestSum()
        {
            RandomOps.Ran2 randSeeder = new RandomOps.Ran2();
            RandomOps.KISS rand1 = new RandomOps.KISS(randSeeder);
            RandomOps.CMWC4096 rand2 = new RandomOps.CMWC4096(randSeeder);
            RandomOps.RanUInt32[] rands = { rand1, rand2 };
            RandomOps.SumUInt32 rand = new RandomOps.SumUInt32(rands);

            Test(rand);
        }

        static void TestRanSystem()
        {
            RandomOps.RanSystem rand = new RandomOps.RanSystem();

            Test(rand);
        }

        static void TestSwitcher()
        {
            RandomOps.RanQD ranQD = new RandomOps.RanQD();
            RandomOps.Ran2 ran2 = new RandomOps.Ran2();
            RandomOps.RanSystem ranSystem = new RandomOps.RanSystem();
            RandomOps.Switcher rand = new RandomOps.Switcher(ranQD, new RandomOps.Random[2] { ran2, ranSystem });

            Test(rand);
        }

        static readonly int BufferSize = 16384;
        static readonly int NumFallback = 16384;
        static readonly int RetrieveTrigger = 4096;

        static void TestInternet()
        {
            RandomOps.Ran2 ran2 = new RandomOps.Ran2();
            RandomOps.RandomDotOrg randInternet = new RandomOps.RandomDotOrg(BufferSize, ran2, NumFallback);

            Test(randInternet);
        }

        static void TestInternetAsync()
        {
            RandomOps.Ran2 ran2 = new RandomOps.Ran2();
            RandomOps.RandomDotOrgAsync randInternetAsync = new RandomOps.RandomDotOrgAsync(BufferSize, RetrieveTrigger, ran2, NumFallback);

            Test(randInternetAsync);

            randInternetAsync.Dispose();
        }

        static RandomOps.Ran2 ran2TS = new RandomOps.Ran2();
        static RandomOps.ThreadSafe.Wrapper randTS = new RandomOps.ThreadSafe.Wrapper(ran2TS);
        static readonly int NumIterationsTS = 100;

        static void Worker1A()
        {
            for (int i = 0; i < NumIterationsTS; i++)
            {
                Console.WriteLine("Thread 1: {0}", randTS.Uniform());
            }
        }

        static void Worker2A()
        {
            for (int i = 0; i < NumIterationsTS; i++)
            {
                Console.WriteLine("Thread 2: {0}", randTS.Gauss());
            }
        }

        static void Worker3A()
        {
            for (int i = 0; i < NumIterationsTS; i++)
            {
                Console.WriteLine("Thread 3: {0}", randTS.Byte());
            }
        }

        static void TestThreadSafeA()
        {
            Thread t1 = new Thread(Worker1A);
            Thread t2 = new Thread(Worker2A);
            Thread t3 = new Thread(Worker3A);

            t1.Start();
            t2.Start();
            t3.Start();

            t1.Join();
            t2.Join();
            t3.Join();
        }

        static RandomOps.ThreadSafe.CMWC4096 randTSB = new RandomOps.ThreadSafe.CMWC4096();

        static void Worker1B()
        {
            for (int i = 0; i < NumIterationsTS; i++)
            {
                Console.WriteLine("Thread 1: {0}", randTSB.Uniform());
            }
        }

        static void Worker2B()
        {
            for (int i = 0; i < NumIterationsTS; i++)
            {
                Console.WriteLine("Thread 2: {0}", randTSB.Gauss());
            }
        }

        static void Worker3B()
        {
            for (int i = 0; i < NumIterationsTS; i++)
            {
                Console.WriteLine("Thread 3: {0}", randTSB.Byte());
            }
        }

        static void TestThreadSafeB()
        {
            Thread t1 = new Thread(Worker1B);
            Thread t2 = new Thread(Worker2B);
            Thread t3 = new Thread(Worker3B);

            t1.Start();
            t2.Start();
            t3.Start();

            t1.Join();
            t2.Join();
            t3.Join();
        }

        static void Main(string[] args)
        {
            //TestRanQD();
            //TestXorShift();
            //TestMWC256();
            //TestCMWC4096();
            TestKISS();
            //TestSum();
            //TestRan2();
            //TestMersenneTwister();
            //TestRanSystem();
            //TestSwitcher();

            //TestInternet();
            //TestInternetAsync();

            //TestThreadSafeA();
            //TestThreadSafeB();
        }
    }
}