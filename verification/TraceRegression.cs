extern alias baseline;
using System;
using System.IO;
using Old = baseline::SwarmOps;
using Current = SwarmOps;

static class TraceRegression
{
    static int checks;

    static void Ensure(bool condition, string message)
    {
        if (!condition) throw new Exception(message);
        checks++;
    }

    public static int Run()
    {
        checks = 0;
        foreach (int iterations in new[] { 1, 2, 10, 23, 100 })
        foreach (int intervals in new[] { 1, 2, 3, 30 })
        foreach (int kind in new[] { 0, 1, 2 })
        {
            Old.FitnessTrace old = kind == 0 ? new Old.FitnessTraceMean(iterations, intervals)
                : kind == 1 ? new Old.FeasibleTrace(iterations, intervals)
                : new Old.FitnessTraceQuartiles(5, iterations, intervals);
            Current.FitnessTrace current = kind == 0 ? new Current.FitnessTraceMean(iterations, intervals)
                : kind == 1 ? new Current.FeasibleTrace(iterations, intervals)
                : new Current.FitnessTraceQuartiles(5, iterations, intervals);
            Ensure(old.Stride == current.Stride && old.Offset == current.Offset && old.MaxIntervals == current.MaxIntervals, "Trace layout changed");
            for (int run = 0; run < 5; run++)
            for (int iteration = 0; iteration < iterations; iteration++)
            {
                double fitness = (run + 1) * 0.125 + iteration * 1.5;
                bool feasible = (run + iteration) % 3 != 0;
                old.Add(iteration, fitness, feasible);
                current.Add(iteration, fitness, feasible);
            }
            CompareOutput(old, current);
            Clear(old); Clear(current);
            CompareOutput(old, current);
            old.Add(iterations - 1, 4.5, true); current.Add(iterations - 1, 4.5, true);
            CompareOutput(old, current);

            using (var writer = new TrackingWriter())
            {
                current.Write(writer);
                Ensure(!writer.Disposed, "Write disposed its caller's writer");
                writer.Write("Caller can continue writing");
            }
            using (var writer = new ThrowingWriter())
            {
                Expect<IOException>(() => current.Write(writer));
                Ensure(!writer.Disposed, "Failed Write disposed its caller's writer");
            }
        }

        foreach (int iterations in new[] { 0, -1 })
            ExpectArgument(() => new Current.FitnessTraceMean(iterations, 1), "numIterations");
        foreach (int intervals in new[] { 0, -1 })
            ExpectArgument(() => new Current.FeasibleTrace(10, intervals), "numIntervals");

        var child = new Probe(null, 10, 10, 0);
        var parent = new Probe(child, 10, 2, 1);
        parent.Add(0, 1, true);
        Ensure(parent.LogCount == 0 && child.LogCount == 1, "Pre-offset sample was not delivered only to child");
        parent.Add(5, 2, true);
        Ensure(parent.LogCount == 1 && child.LogCount == 2, "Valid sample was not delivered to both traces");
        parent.Add(-5, 3, false);
        parent.Add(10, 3, false);
        Ensure(parent.LogCount == 1 && child.LogCount == 2, "Out-of-range sample was logged");

        string directory = Path.Combine(Path.GetTempPath(), "swarmops-trace-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        try
        {
            string path = Path.Combine(directory, "trace.txt");
            var trace = new Current.FitnessTraceMean(10, 3);
            trace.Add(0, 1.25, true);
            using var expected = new StringWriter();
            trace.Write(expected);
            trace.WriteToFile(path);
            Ensure(File.ReadAllText(path) == expected.ToString(), "WriteToFile did not flush the complete trace");
            using (File.Open(path, FileMode.Open, FileAccess.ReadWrite, FileShare.None)) { checks++; }

            var throwing = new Probe(null, 10, 2, 0) { ThrowOnWrite = true };
            Expect<IOException>(() => throwing.WriteToFile(path));
            Expect<ObjectDisposedException>(() => throwing.CapturedWriter.Write("after failure"));
            using (File.Open(path, FileMode.Open, FileAccess.ReadWrite, FileShare.None)) { checks++; }
        }
        finally
        {
            Directory.Delete(directory, true);
        }

        var random = new Random(1983);
        for (int length = 0; length <= 128; length++)
        {
            var input = new double[length];
            for (int i = 0; i < length; i++) input[i] = random.NextDouble() * 200 - 100;
            CheckQuartiles(input);
        }
        CheckQuartiles(new[] { double.NaN, double.PositiveInfinity, double.NegativeInfinity, -0.0, 0.0 });
        Console.WriteLine($"PASS: {checks} trace, writer-lifetime, failure-path and quartile checks");
        return checks;
    }

    static void CheckQuartiles(double[] input)
    {
        var original = (double[])input.Clone();
        var old = new Old.Quartiles(); var current = new Current.Quartiles();
        old.ComputeUnsorted(input); current.ComputeUnsorted(input);
        CompareQuartiles(old, current);
        for (int i = 0; i < input.Length; i++) { Study.Equal(original[i], input[i]); checks++; }
        var oldSorted = (double[])input.Clone(); var newSorted = (double[])input.Clone();
        old.ComputeUnsortedInplace(oldSorted); current.ComputeUnsortedInplace(newSorted);
        CompareQuartiles(old, current);
        for (int i = 0; i < input.Length; i++) { Study.Equal(oldSorted[i], newSorted[i]); checks++; }
    }

    static void CompareQuartiles(Old.Quartiles old, Current.Quartiles current)
    {
        double?[] expected = { old.Min, old.Max, old.Q1, old.Q2, old.Q3, old.Median, old.IQR };
        double?[] actual = { current.Min, current.Max, current.Q1, current.Q2, current.Q3, current.Median, current.IQR };
        for (int i = 0; i < expected.Length; i++)
        {
            Ensure(expected[i].HasValue == actual[i].HasValue, "Quartile presence changed");
            if (expected[i].HasValue) { Study.Equal(expected[i].Value, actual[i].Value); checks++; }
        }
    }

    static void CompareOutput(Old.FitnessTrace old, Current.FitnessTrace current)
    {
        using var expected = new StringWriter(); using var actual = new StringWriter();
        old.Write(expected); current.Write(actual);
        Ensure(expected.ToString() == actual.ToString(), "Trace output changed");
    }

    static void Clear(Old.FitnessTrace trace)
    {
        if (trace is Old.FitnessTraceMean mean) mean.Clear();
        else if (trace is Old.FeasibleTrace feasible) feasible.Clear();
        else ((Old.FitnessTraceQuartiles)trace).Clear();
    }

    static void Clear(Current.FitnessTrace trace)
    {
        if (trace is Current.FitnessTraceMean mean) mean.Clear();
        else if (trace is Current.FeasibleTrace feasible) feasible.Clear();
        else ((Current.FitnessTraceQuartiles)trace).Clear();
    }

    static void ExpectArgument(Action action, string parameter)
    {
        try { action(); }
        catch (ArgumentOutOfRangeException error) { Ensure(error.ParamName == parameter, "Wrong invalid trace parameter"); return; }
        throw new Exception("Invalid trace dimensions were accepted");
    }

    static void Expect<T>(Action action) where T : Exception
    {
        try { action(); }
        catch (T) { checks++; return; }
        throw new Exception($"Expected {typeof(T).Name}");
    }

    class TrackingWriter : StringWriter
    {
        public bool Disposed;
        protected override void Dispose(bool disposing) { Disposed = true; base.Dispose(disposing); }
    }

    class ThrowingWriter : TrackingWriter
    {
        public override void WriteLine(string value) { throw new IOException("Injected write failure"); }
    }

    class Probe : Current.FitnessTrace
    {
        public int LogCount;
        public bool ThrowOnWrite;
        public TextWriter CapturedWriter;
        public Probe(Current.FitnessTrace child, int iterations, int intervals, double offset) : base(child, iterations, intervals, offset) { }
        protected override void Log(int index, double fitness, bool feasible) { if (index < 0) throw new Exception("Negative trace index"); LogCount++; }
        public override void Write(TextWriter writer)
        {
            CapturedWriter = writer;
            writer.Write("partial trace");
            if (ThrowOnWrite) throw new IOException("Injected trace failure");
        }
    }
}
