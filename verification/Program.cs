extern alias baseline;
using System;
using System.Collections.Generic;
using System.IO;
using Old = baseline::SwarmOps;

class Runner
{
    static void Main(string[] args)
    {
        Study.PublicApi(typeof(Old.Problem).Assembly, typeof(SwarmOps.Problem).Assembly);
        int checks = 0;
        var measurements = new List<object>();
        var random = new Random(1937);
        foreach (int dimension in new[] { 1, 2, 10, 30, 100, 1000 })
        {
            var old = new Old.Problems.Schwefel12(dimension, 5000);
            var current = new SwarmOps.Problems.Schwefel12(dimension, 5000);
            var x = new double[dimension];
            for (int sample = 0; sample < 1000; sample++)
            {
                for (int d = 0; d < dimension; d++) x[d] = sample == 0 ? 0 : sample == 1 ? 100 : sample == 2 ? (d % 2 == 0 ? 100 : -100) : random.NextDouble() * 200 - 100;
                Study.Equal(old.Fitness(x), current.Fitness(x)); checks++;
            }
            measurements.Add(Study.Measure($"Schwefel12 dimension {dimension}: prefix reuse only",
                () => old.Fitness(x), () => current.Fitness(x), dimension >= 100 ? 1000 : 20000));
        }
        Old.Globals.ParallelOptions.MaxDegreeOfParallelism = 1;
        SwarmOps.Globals.ParallelOptions.MaxDegreeOfParallelism = 1;
        foreach (string method in new[] { "PSO", "MOL", "ParallelPSO", "ParallelMOL" })
        for (int seed = 1; seed <= 30; seed++)
        {
            var oldProblem = new OldTrace(10, 5000);
            Old.Globals.Random = new RandomOps.Ran2(seed);
            Old.Optimizer oldOptimizer = method switch {
                "PSO" => new Old.Optimizers.PSO(oldProblem),
                "MOL" => new Old.Optimizers.MOL(oldProblem),
                "ParallelPSO" => new Old.Optimizers.Parallel.PSO(oldProblem),
                _ => new Old.Optimizers.Parallel.MOL(oldProblem) };
            var expected = oldOptimizer.Optimize();
            var problem = new CurrentTrace(10, 5000);
            SwarmOps.Globals.Random = new RandomOps.Ran2(seed);
            SwarmOps.Optimizer optimizer = method switch {
                "PSO" => new SwarmOps.Optimizers.PSO(problem),
                "MOL" => new SwarmOps.Optimizers.MOL(problem),
                "ParallelPSO" => new SwarmOps.Optimizers.Parallel.PSO(problem),
                _ => new SwarmOps.Optimizers.Parallel.MOL(problem) };
            var actual = optimizer.Optimize();
            Study.Equal(expected.Fitness, actual.Fitness); checks++;
            if (expected.Iterations != actual.Iterations || expected.Feasible != actual.Feasible) throw new Exception($"{method} result metadata changed");
            for (int d = 0; d < 10; d++) { Study.Equal(expected.Parameters[d], actual.Parameters[d]); checks++; }
            if (oldProblem.Values.Count != problem.Values.Count) throw new Exception("Evaluation count changed");
            for (int i = 0; i < problem.Values.Count; i++) { Study.Equal(oldProblem.Values[i], problem.Values[i]); checks++; }
        }
        // Real multi-worker runs: compare final solutions and unordered fitness traces,
        // because scheduling can change observation order without changing the algorithm.
        Old.Globals.ParallelOptions.MaxDegreeOfParallelism = 4;
        SwarmOps.Globals.ParallelOptions.MaxDegreeOfParallelism = 4;
        foreach (bool mol in new[] { false, true })
        for (int seed = 1; seed <= 5; seed++)
        {
            var oldProblem = new OldTrace(10, 5000);
            Old.Globals.Random = new RandomOps.Ran2(seed);
            Old.Optimizer oldOptimizer = mol ? new Old.Optimizers.Parallel.MOL(oldProblem) : new Old.Optimizers.Parallel.PSO(oldProblem);
            var expected = oldOptimizer.Optimize();
            var problem = new CurrentTrace(10, 5000);
            SwarmOps.Globals.Random = new RandomOps.Ran2(seed);
            SwarmOps.Optimizer optimizer = mol ? new SwarmOps.Optimizers.Parallel.MOL(problem) : new SwarmOps.Optimizers.Parallel.PSO(problem);
            var actual = optimizer.Optimize();
            Study.Equal(expected.Fitness, actual.Fitness); checks++;
            for (int d = 0; d < 10; d++) { Study.Equal(expected.Parameters[d], actual.Parameters[d]); checks++; }
            if (expected.Iterations != actual.Iterations || expected.Feasible != actual.Feasible || oldProblem.Values.Count != problem.Values.Count) throw new Exception("Parallel result metadata changed");
            oldProblem.Values.Sort(); problem.Values.Sort();
            for (int i = 0; i < problem.Values.Count; i++) { Study.Equal(oldProblem.Values[i], problem.Values[i]); checks++; }
        }
        var velocityLower = new[] { 0.0, 0.0, 123.0 };
        var velocityUpper = new[] { 0.0, 0.0, 456.0 };
        typeof(SwarmOps.Tools).GetMethod("InitializeVelocityBounds", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static)
            .Invoke(null, new object[] { 2, new[] { 1.0, 7.0, 0.0 }, new[] { 3.0, 3.0, 0.0 }, velocityLower, velocityUpper });
        Study.Equal(-2.0, velocityLower[0]); Study.Equal(-4.0, velocityLower[1]); Study.Equal(123.0, velocityLower[2]);
        Study.Equal(2.0, velocityUpper[0]); Study.Equal(4.0, velocityUpper[1]); Study.Equal(456.0, velocityUpper[2]); checks += 6;
        foreach (double value in new[] { -2.0, -1.0, -0.0, 0.0, 1.0, 2.0, double.NaN, double.NegativeInfinity, double.PositiveInfinity })
        {
            var x = new[] { value };
            foreach (double lower in new[] { -1.0, double.NaN, double.NegativeInfinity })
            foreach (double upper in new[] { 1.0, double.NaN, double.PositiveInfinity })
            {
                bool expected = Old.Tools.BetweenBounds(x, new[] { lower }, new[] { upper });
                bool actual = SwarmOps.Tools.BetweenBounds(x, new[] { lower }, new[] { upper });
                if (expected != actual) throw new Exception("Bounds check behavior changed"); checks++;
            }
        }
        if (!SwarmOps.Tools.BetweenBounds(Array.Empty<double>(), Array.Empty<double>(), Array.Empty<double>())) throw new Exception("Empty bounds changed"); checks++;
        int traceChecks = TraceRegression.Run();
        Study.Save(args[0], checks, measurements, "Prefix sums, shared velocity-bound setup, and bounds simplification. Fitnesses and full PSO/MOL/sequential/parallel evaluation traces match exactly for 30 seeds, 5000 evaluations per run; parallel comparisons use one worker for deterministic evaluation order. Nine alternating timing trials after warmup.", traceChecks);
    }
    class OldTrace : Old.Problems.Schwefel12
    {
        public List<double> Values = new List<double>();
        public OldTrace(int dim, int iterations) : base(dim, iterations) { }
        public override double Fitness(double[] x) { double value = base.Fitness(x); lock (Values) Values.Add(value); return value; }
    }
    class CurrentTrace : SwarmOps.Problems.Schwefel12
    {
        public List<double> Values = new List<double>();
        public CurrentTrace(int dim, int iterations) : base(dim, iterations) { }
        public override double Fitness(double[] x) { double value = base.Fitness(x); lock (Values) Values.Add(value); return value; }
    }
}
static class Study
{
    public static void PublicApi(System.Reflection.Assembly baseline, System.Reflection.Assembly current)
    {
        string[] Describe(System.Reflection.Assembly assembly)
        {
            var members = new List<string>();
            foreach (var type in assembly.GetExportedTypes())
            {
                members.Add(type.FullName);
                foreach (var member in type.GetMembers(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.DeclaredOnly))
                    members.Add(type.FullName + ":" + member.MemberType + ":" + member);
            }
            members.Sort(StringComparer.Ordinal);
            return members.ToArray();
        }
        var expected = Describe(baseline); var actual = Describe(current);
        if (expected.Length != actual.Length) throw new Exception("Public API member count changed");
        for (int i = 0; i < expected.Length; i++)
            if (expected[i] != actual[i]) throw new Exception($"Public API changed: {expected[i]} versus {actual[i]}");
        Console.WriteLine($"PASS: {expected.Length} public type/member signatures unchanged");
    }

    public static void Equal(double expected, double actual)
    {
        if (BitConverter.DoubleToInt64Bits(expected) != BitConverter.DoubleToInt64Bits(actual))
            throw new Exception($"Numerical mismatch: {expected:R} versus {actual:R}");
    }
    public static object Measure(string name, Func<double> baseline, Func<double> candidate, int iterations = 20000)
    {
        for (int i = 0; i < 10000; i++) { baseline(); candidate(); }
        var oldTimes = new double[9]; var newTimes = new double[9];
        var oldBytes = new long[9]; var newBytes = new long[9];
        double sink = 0;
        void Batch(Func<double> call, double[] times, long[] bytes, int trial)
        {
            long before = GC.GetAllocatedBytesForCurrentThread();
            long start = System.Diagnostics.Stopwatch.GetTimestamp();
            for (int i = 0; i < iterations; i++) sink += call();
            times[trial] = System.Diagnostics.Stopwatch.GetElapsedTime(start).TotalMilliseconds;
            bytes[trial] = GC.GetAllocatedBytesForCurrentThread() - before;
        }
        for (int trial = 0; trial < 9; trial++)
        {
            if (trial % 2 == 0) { Batch(baseline, oldTimes, oldBytes, trial); Batch(candidate, newTimes, newBytes, trial); }
            else { Batch(candidate, newTimes, newBytes, trial); Batch(baseline, oldTimes, oldBytes, trial); }
        }
        GC.KeepAlive(sink);
        double[] sortedOld = (double[])oldTimes.Clone(), sortedNew = (double[])newTimes.Clone();
        Array.Sort(sortedOld); Array.Sort(sortedNew);
        return new { name, iterations, baselineMilliseconds = oldTimes, candidateMilliseconds = newTimes,
            baselineMedianMilliseconds = sortedOld[4], candidateMedianMilliseconds = sortedNew[4],
            speedup = sortedOld[4] / sortedNew[4], baselineBytesPerCall = oldBytes[4] / (double)iterations,
            candidateBytesPerCall = newBytes[4] / (double)iterations };
    }
    public static void Save(string path, int checks, List<object> measurements, string notes, int traceBehaviorChecks = 0)
    {
        File.WriteAllText(path, System.Text.Json.JsonSerializer.Serialize(new {
            runtime = System.Runtime.InteropServices.RuntimeInformation.FrameworkDescription,
            architecture = System.Runtime.InteropServices.RuntimeInformation.ProcessArchitecture.ToString(),
            numericalChecks = checks, traceBehaviorChecks, maximumAbsoluteError = 0, maximumRelativeError = 0,
            measurements, notes
        }, new System.Text.Json.JsonSerializerOptions { WriteIndented = true }) + "\n");
        Console.WriteLine($"PASS: {checks} exact numerical checks; results: {path}");
    }
}
