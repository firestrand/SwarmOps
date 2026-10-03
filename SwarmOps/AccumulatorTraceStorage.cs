namespace SwarmOps
{
    /// <summary>Creates and clears accumulator storage shared by interval traces.</summary>
    internal static class AccumulatorTraceStorage
    {
        internal static StatisticsAccumulator[] Create(int count)
        {
            var trace = new StatisticsAccumulator[count];
            for (int i = 0; i < trace.Length; i++)
            {
                trace[i] = new StatisticsAccumulator();
            }
            return trace;
        }

        internal static void Clear(StatisticsAccumulator[] trace)
        {
            foreach (StatisticsAccumulator accumulator in trace)
            {
                accumulator.Clear();
            }
        }
    }
}
