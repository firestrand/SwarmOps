/// ------------------------------------------------------
/// SwarmOps - Numeric and heuristic optimization for C#
/// Copyright (C) 2003-2010 Magnus Erik Hvass Pedersen.
/// Published under the GNU Lesser General Public License.
/// Please see the file license.txt for license details.
/// SwarmOps on the internet: http://www.Hvass-Labs.org/
/// ------------------------------------------------------

using System.Diagnostics;

namespace SwarmOps.Optimizers.Parallel
{
    /// <summary>
    /// Parallel version of DE which computes the fitness of its agents
    /// in parallel. Assumes the fitness function is thread-safe. Should
    /// only be used with very time-consuming optimization problems otherwise
    /// basic DE will execute faster because of less overhead.
    /// </summary>
    public class DE : Optimizer
    {
        #region Constructors.
        /// <summary>
        /// Construct the object.
        /// </summary>
        public DE()
            : this(1)
        {
        }

        /// <summary>
        /// Construct the object.
        /// </summary>
        /// <param name="problem">Problem to optimize.</param>
        public DE(Problem problem)
            : this(1, problem)
        {
        }

        /// <summary>
        /// Construct the object.
        /// </summary>
        /// <param name="numAgentsMultiple">Population size multiple, e.g. 4 ensures populations are sized 4, 8, 12, 16, ...</param>
        public DE(int numAgentsMultiple)
            : base()
        {
            NumAgentsMultiple = numAgentsMultiple;
        }

        /// <summary>
        /// Construct the object.
        /// </summary>
        /// <param name="numAgentsMultiple">Population size multiple, e.g. 4 ensures populations are sized 4, 8, 12, 16, etc.</param>
        /// <param name="problem">Problem to optimize.</param>
        public DE(int numAgentsMultiple, Problem problem)
            : base(problem)
        {
            NumAgentsMultiple = numAgentsMultiple;
        }
        #endregion

        #region Sets of control parameters.
        /// <summary>
        /// Control parameters.
        /// </summary>
        public struct Parameters
        {
            /// <summary>
            /// Control parameters tuned for all benchmark problems in
            /// 5 dimensions and 10000 fitness evaluations in one optimization run.
            /// </summary>
            public static readonly double[] AllBenchmarks5Dim10000Iter = { 32.0, 0.4845, 0.9833 };

            /// <summary>
            /// Control parameters tuned for all benchmark problems in
            /// 30 dimensions and 60000 fitness evaluations in one optimization run.
            /// </summary>
            public static readonly double[] AllBenchmarks30Dim60000Iter = { 32.0, 0.3176, 0.5543 };
        }
        #endregion

        #region Get individual control parameters.
        /// <summary>
        /// Population size multiple, e.g. 4 ensures populations are sized 4, 8, 12, 16, etc.
        /// </summary>
        public int NumAgentsMultiple
        {
            get;
            protected set;
        }

        /// <summary>
        /// Get parameter, Number of agents, aka. population size.
        /// </summary>
        /// <param name="parameters">Optimizer parameters.</param>
        public int GetNumAgents(double[] parameters)
        {
            int numAgents = (int)System.Math.Round(parameters[0], System.MidpointRounding.AwayFromZero);

            // Ensure numAgents falls on desired multiple.
            numAgents--;
            int mod = numAgents % NumAgentsMultiple;
            numAgents += NumAgentsMultiple - mod;

            return numAgents;
        }

        /// <summary>
        /// Get parameter, CR, aka. crossover probability.
        /// </summary>
        /// <param name="parameters">Optimizer parameters.</param>
        public double GetCR(double[] parameters)
        {

            return parameters[1];
        }

        /// <summary>
        /// Get parameter, F, aka. differential weight.
        /// </summary>
        /// <param name="parameters">Optimizer parameters.</param>
        public double GetF(double[] parameters)
        {
            return parameters[2];
        }
        #endregion

        #region Base-class overrides, Problem.
        /// <summary>
        /// Name of the optimizer.
        /// </summary>
        public override string Name
        {
            get { return "DE-Simple-Par" + NumAgentsMultiple; }
        }

        /// <summary>
        /// Number of control parameters for optimizer.
        /// </summary>
        public override int Dimensionality
        {
            get { return 3; }
        }

        string[] _parameterName = { "NP", "CR", "F" };

        /// <summary>
        /// Control parameter names.
        /// </summary>
        public override string[] ParameterName
        {
            get { return _parameterName; }
        }

        /// <summary>
        /// Default control parameters.
        /// </summary>
        public override double[] DefaultParameters
        {
            get { return Parameters.AllBenchmarks30Dim60000Iter; }
        }

        static readonly double[] _lowerBound = { 3, 0, 0 };

        /// <summary>
        /// Lower search-space boundary for control parameters.
        /// </summary>
        public override double[] LowerBound
        {
            get { return _lowerBound; }
        }

        static readonly double[] _upperBound = { 200, 1, 2.0 };

        /// <summary>
        /// Upper search-space boundary for control parameters.
        /// </summary>
        public override double[] UpperBound
        {
            get { return _upperBound; }
        }
        #endregion

        #region Base-class overrides, Optimizer.
        /// <summary>
        /// Perform one optimization run and return the best found solution.
        /// </summary>
        /// <param name="parameters">Control parameters for the optimizer.</param>
        public override Result Optimize(double[] parameters)
        {
            Debug.Assert(parameters != null && parameters.Length == Dimensionality);

            // Retrieve parameters specific to DE method.
            int numAgents = GetNumAgents(parameters);
            double CR = GetCR(parameters);
            double F = GetF(parameters);

            Debug.Assert(numAgents > 0);

            // Get problem-context.
            double[] lowerBound = Problem.LowerBound;
            double[] upperBound = Problem.UpperBound;
            double[] lowerInit = Problem.LowerInit;
            double[] upperInit = Problem.UpperInit;
            int n = Problem.Dimensionality;

            // Allocate agent positions and associated fitnesses.
            double[][] agentsX = Tools.NewMatrix(numAgents, n);
            double[][] agentsY = Tools.NewMatrix(numAgents, n);
            double[] agentFitnessX = new double[numAgents];
            double[] agentFitnessY = new double[numAgents];
            double[] t = new double[n];

            // Iteration variables.
            int i, j;

            // Fitness variables.
            double gFitness = Problem.MaxFitness;
            double[] g = null;

            // Initialize agent-position in search-space. (Non-parallel)
            for (j = 0; j < numAgents; j++)
            {
                Tools.InitializeUniform(ref agentsX[j], lowerInit, upperInit);
            }

            // Compute fitness of initial positions. (Parallel)
            // This counts as iterations below.
            System.Threading.Tasks.Parallel.For(0, numAgents, Globals.ParallelOptions, (jPar) =>
            {
                agentFitnessX[jPar] = Problem.Fitness(agentsX[jPar]);
            });

            // Initialize the best-found position to the first in the population.
            g = agentsX[0];
            gFitness = agentFitnessX[0];
            Trace(0, gFitness);

            // Update best-found position. (Non-parallel)
            for (j = 1; j < numAgents; j++)
            {
                // Update swarm's best known position.
                if (agentFitnessX[j] < gFitness)
                {
                    g = agentsX[j];
                    gFitness = agentFitnessX[j];
                }

                // Trace fitness of best found solution.
                Trace(j, gFitness);
            }

            // Perform optimization.
            for (i = numAgents; Problem.RunCondition.Continue(i, gFitness); )
            {
                // Compute potential new position. (Non-parallel)
                for (j=0; j<numAgents; j++)
                {
                    // Refer to the j'th agent as x.
                    double[] x = agentsX[j];

                    // Refer to its potentially new position as y.
                    double[] y = agentsY[j];

                    // Pick a random dimension.
                    int R = Globals.Random.Index(n);

                    // Pick random and distinct agent-indices.
                    // Not necessarily distinct from x though.
                    int R1, R2;
                    Globals.Random.Index2(numAgents, out R1, out R2);

                    // Refer to the randomly picked agents as a and b.
                    double[] a = agentsX[R1];
                    double[] b = agentsX[R2];

                    // Compute potentially new position.
                    for (int k = 0; k < n; k++)
                    {
                        if (k == R || Globals.Random.Uniform() < CR)
                        {
                            y[k] = g[k] + F * (a[k] - b[k]);
                        }
                        else
                        {
                            y[k] = x[k];
                        }
                    }

                    // Enforce bounds before computing new fitness.
                    Tools.Bound(ref y, lowerBound, upperBound);
                }

                // Compute fitness of y-position. (Parallel)
                System.Threading.Tasks.Parallel.For(0, numAgents, Globals.ParallelOptions, (jPar) =>
                {
                    agentFitnessY[jPar] = Problem.Fitness(agentsY[jPar], agentFitnessX[jPar]);
                });

                // Update agent-positions if improved fitness. (Non-parallel)
                for (j = 0; j < numAgents; j++, i++)
                {
                    // Update agent in case of fitness improvement.
                    if (agentFitnessY[j] < agentFitnessX[j])
                    {
                        // Update agent's position.
                        agentsY[j].CopyTo(agentsX[j], 0);

                        // Update agent's fitness.
                        agentFitnessX[j] = agentFitnessY[j];

                        // Update swarm's best known position.
                        if (agentFitnessY[j] < gFitness)
                        {
                            g = agentsX[j];
                            gFitness = agentFitnessY[j];
                        }
                    }

                    // Trace fitness of best found solution.
                    Trace(i, gFitness);
                }
            }

            // Return best-found solution and fitness.
            return new Result(g, gFitness, i);
        }
        #endregion
    }
}