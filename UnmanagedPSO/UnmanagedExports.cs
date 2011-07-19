using System;
using System.Collections.Generic;
using System.Text;
using RGiesecke.DllExport;
using SwarmOps;
using SwarmOps.Optimizers;

namespace UnmanagedPSO
{
   internal static class UnmanagedExports
   {
      // Create optimizer object.
      static Optimizer Optimizer = new SPSO();

      // Control parameters for optimizer.
      private static readonly double[] Parameters = Optimizer.DefaultParameters;
      //static readonly double[] Parameters = MOL.Parameters.HandTuned;

      // Optimization settings.
      static readonly int NumRuns = 10;
      static int _dimensions = 30;
      static int _generations = 2000;
      static readonly int NumIterations = _generations * _dimensions; //Really the number of function evaluations
      static readonly bool DisplaceOptimum = true;
      static IRunCondition RunCondition = new RunConditionIterations(NumIterations);
      static StringBuilder _resultSb = new StringBuilder();

      [DllExport("adddays", CallingConvention = System.Runtime.InteropServices.CallingConvention.StdCall)]
      static double AddDays(double dateValue, int days)
      {
         return DateTime.FromOADate(dateValue).AddDays(days).ToOADate();
      }
      [DllExport("initializePSO", CallingConvention = System.Runtime.InteropServices.CallingConvention.StdCall)]
      static bool InitializePso(int dimensions, int generations)
      {
          _dimensions = dimensions;
          _generations = generations;
          return false;
      }
      
   }
}
