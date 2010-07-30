SwarmOps - Numeric and heuristic optimization for C#
Copyright (C) 2003-2010 Magnus Erik Hvass Pedersen.
Published under the GNU Lesser General Public License.
Please see the file license.txt for license details.
SwarmOps on the internet: http://www.Hvass-Labs.org/


Installation:

To install SwarmOps follow these simple steps:
1. Unpack the SwarmOps archive to a convenient
   directory.
2. In MS Visual Studio open the Solution in which you
   will use SwarmOps.
3. Add the SwarmOps project to the solution.
4. Add a reference to SwarmOps in all the projects
   which must use it.


Requirements:

SwarmOps requires a Random Number Generator (RNG) and by
default uses the RandomOps library version 2 or later,
which must be installed before SwarmOps can be used. If
you wish to use another RNG, then the easiest thing is
to make a wrapper for that RNG in RandomOps, so you do
not have to change all the source-code of SwarmOps that
uses the RNG. RandomOps can be obtained on the internet:
http://www.Hvass-Labs.org/


Compiler Compatibility:

SwarmOps was developed in MS Visual C# 2010 with .NET
framework v4.


Update History:

Version 2:
- Added parallel version of the optimizers: DE, DESuite, MOL, PSO.
- Added parallel version of MetaFitness.
- Changed FitnessTrace to allow optimization runs that are longer
  than assumed without causing a crash.
- Changed GD so it doesn't add time-complexity-factor because it
  is incompatible with FitnessTrace and causes a crash.

Version 1.1:
- Added weights to MetaFitness to be used in TestMetaBenchmarks2.
  This would be a bit intricate to program for the approach
  used in TestMetaBenchmarks.
- Added DESuite which is DE optimizer with variants for
  crossover and dithering.
- Added JDE which is the Janez 'self-adaptive' DE variant.
- Added new parameter sets to MOL method.
- Added a 'raw' format to FitnessPrint, useful with e.g. GnuPlot.
- Fixed potential bug in Result-class by cloning parameters-array.
- Added Solution and LogSolutions to log best solutions discovered.
- Added ProblemWrapper as a base-class for convenience.
- Added VarianceAccumulator for computing variance and stddev.
- Changed printing of numbers to US-format which is better supported
  by other software, e.g. GnuPlot.
- Added TestMesh project for computing mesh of meta-fitness values.
- Added StatisticsAccumulator.
- Changed FitnessTrace to FitnessTraceMean and to use
  StatisticsAccumulator and print more statistics.
- Added Max limit for length of fitness trace.
- Added Quartiles class to compute quartiles.
- Added FitnessTraceQuartiles to compute quartiles at intervals
  during optimization runs.
- Added chaining capability for FitnessTrace class to link
  the computation of several kinds of fitness-traces.
- Added some sample GnuPlot files.

Version 1.0:
- First release.
