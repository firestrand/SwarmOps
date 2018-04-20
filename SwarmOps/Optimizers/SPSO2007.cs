/// ------------------------------------------------------
/// SwarmOps - Numeric and heuristic optimization for C#
/// Published under the GNU Lesser General Public License.
/// Please see the file license.txt for license details.
/// SwarmOps on the internet: http://www.Hvass-Labs.org/
/// ------------------------------------------------------

using System;
using System.Collections;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;
using System.Linq.Expressions;
using RandomOps;
using Random = System.Random;

namespace SwarmOps.Optimizers
{
    /// <summary>
    /// Standard Particle Swarm Optimization (SPSO) originally due to
    /// Eberhart et al. This the 'Standard PSO 2007' variant from Clerc.
    /// 
    /// </summary>
    /// <remarks>
    /// References:
    /// (1) Eberhart, R. C. and Kennedy, J. A new optimizer using particle
    ///     swarm theory. Proceedings of the Sixth International Symposium
    ///     on Micromachine and Human Science, Nagoya, Japan pp. 39-43.
    /// (2) J. Kennedy and R. Eberhart. Particle swarm optimization.
    ///     In Proceedings of IEEE International Conference on Neural
    ///     Networks, volume IV, pages 1942-1948, Perth, Australia, 1995
    /// (3) Clerc, M. C Source Code downloaded from http://clerc.maurice.free.fr/pso/
    /// </remarks>
    public class SPSO2007 : Optimizer
    {
        #region Constructors.
        /// <summary>
        /// Construct the object.
        /// </summary>
        public SPSO2007()
            : base()
        {
            RandomChoice = RandomAlgorithm.MersenneTwister;
        }

        /// <summary>
        /// Construct the object.
        /// </summary>
        /// <param name="problem">Problem to optimize.</param>
        public SPSO2007(Problem problem)
            : base(problem)
        {
            RandomChoice = RandomAlgorithm.MersenneTwister; //Default Random Choice
        }
        #endregion

        #region Sets of control parameters.
        /// <summary>
        /// Control parameters.
        /// </summary>
        public struct Parameters
        {
            /// <summary>
            /// Hand-tuned control parameters.
            /// </summary>
            public static readonly double[] HandTuned = { 50.0, 3.0,1.0, 0.72984, 1.193 };
        }
        #endregion

        #region Base-class overrides, Problem.
        /// <summary>
        /// Name of the optimizer.
        /// </summary>
        public override string Name
        {
            get { return "SPSO"; }
        }

        /// <summary>
        /// Number of control parameters for optimizer.
        /// </summary>
        public override int Dimensionality
        {
            get { return 5; }
        }
            
        /* S := swarm size
        * K := maximum number of particles _informed_ by a given one
        * p := probability threshold of random topology, typically calculated from K
        * w := first cognitive/confidence coefficient
        * c := second cognitive/confidence coefficient
         */
        string[] _parameterName = { "S", "K", "p", "w", "c" };

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
            get{return this.CalculateParameters(30,3);}
        }

        static readonly double[] _lowerBound = { 1.0, 0.0,1.0, -2.0, -4.0 };

        /// <summary>
        /// Lower search-space boundary for control parameters.
        /// </summary>
        public override double[] LowerBound
        {
            get { return _lowerBound; }
        }

        static readonly double[] _upperBound = { 200.0, 200.0,0.0, 2.0, 4.0 };

        /// <summary>
        /// Upper search-space boundary for control parameters.
        /// </summary>
        public override double[] UpperBound
        {
            get { return _upperBound; }
        }

        public RandomAlgorithm RandomChoice { get; set; }
        #endregion

        #region Base-class overrides, Optimizer.
        public new Result Optimize()
        {
            var parameters = this.CalculateParameters(Problem.Dimensionality, 3);
            return Optimize(parameters);
        }
        /// <summary>
        /// Perform one optimization run and return the best found solution.
        /// </summary>
        /// <param name="parameters">Control parameters for the optimizer.</param>
        public override Result Optimize(double[] parameters)
        {
            Debug.Assert(parameters != null && parameters.Length == Dimensionality);

            // Retrieve parameter specific to SPSO method.
            int S = (int)System.Math.Round(parameters[0], System.MidpointRounding.AwayFromZero);
            Debug.Assert(S > 0);

            double p = parameters[2]; //This is what matters for informed particles
            double w = parameters[3];
            double c = parameters[4];

            //Initialize Random for each particle
            Random rand = new Random();

            // Get problem-context.
            double[] lowerBound = Problem.LowerBound;
            double[] upperBound = Problem.UpperBound;
            double[] lowerInit = Problem.LowerInit;
            double[] upperInit = Problem.UpperInit;
            int n = Problem.Dimensionality;

            // Allocate agent positions and associated fitnesses.
            double[][] agents = Tools.NewMatrix(S, n);
            double[][] velocities = Tools.NewMatrix(S, n);
            double[][] bestAgentPosition = Tools.NewMatrix(S, n);

            int[,] links = new int[S, S];
            int[] index = new int[S];
            int g;
            double[] px = new double[n];
            double[] gx = new double[n];
            int nEval = 0;

            double[] agentFitness = new double[S];
            double[] bestAgentFitness = new double[S];

            // Initialize
            // Initialize all agents.
            // This counts as iterations below.
            // Position and velocity
            for (int s = 0; s < S; s++)
            {
                for (int d = 0; d < n; d++)
                {
                    agents[s][d] = rand.NextDouble(lowerInit[d], upperInit[d]);
                    velocities[s][d] = (rand.NextDouble(lowerBound[d], upperBound[d]) - agents[s][d]) / 2;
                }
            }

            // First evaluations
            for (int s = 0; s < S; s++)
            {
                agentFitness[s] = Problem.Fitness(agents[s]);
                nEval++;
                agents[s].CopyTo(bestAgentPosition[s], 0);	// Best position = current one
                bestAgentFitness[s] = agentFitness[s];
            }

            // Find the best
            int best = 0;
            double errorPrev = bestAgentFitness[best];

            for (int s = 1; s < S; s++)
            {
                if (bestAgentFitness[s] < errorPrev)
                {
                    best = s;
                    errorPrev = bestAgentFitness[s];
                }
            }

            int initLinks = 1;		// So that information links will beinitialized
            int noStop = 0;
            // ---------------------------------------------- ITERATIONS
            while (noStop == 0)
            {
                if (initLinks == 1)	// Random topology
                {
                    // Who informs who, at random
                    for (int s = 0; s < S; s++)
                    {
                        for (int m = 0; m < S; m++)
                        {
                            if (rand.NextDouble() < p) links[m, s] = 1;	// Probabilistic method
                            else links[m, s] = 0;
                        }
                        links[s, s] = 1;
                    }
                }

                // The swarm MOVES
                for (int i = 0; i < S; i++)
                    index[i] = i;

                for (int s0 = 0; s0 < S; s0++)	// For each particle ...
                {
                    int s = index[s0];
                    // ... find the first informant
                    int s1 = 0;
                    while (links[s1, s] == 0) s1++;
                    if (s1 >= S) s1 = s;

                    // Find the best informant			
                    g = s1;
                    for (int m = s1; m < S; m++)
                    {
                        if (links[m, s] == 1 && bestAgentFitness[m] < bestAgentFitness[g])
                            g = m;
                    }

                    //.. compute the new velocity, and move
                    // Exploration tendency
                    if(g!=s)
                    {
                        for (int d = 0; d < n; d++)
                        {
                            velocities[s][d] = w*velocities[s][d];
                            px[d] = bestAgentPosition[s][d] - agents[s][d];
                            gx[d] = bestAgentPosition[g][d] - agents[s][d];
                            velocities[s][d] += rand.NextDouble(0.0, c) * px[d];
                            velocities[s][d] += rand.NextDouble(0.0, c) * gx[d];
                            agents[s][d] = agents[s][d] + velocities[s][d];
                        }
                    }
                    else
                    {
                        for (int d = 0; d < n; d++)
                        {
                            velocities[s][d] = w * velocities[s][d];
                            px[d] = bestAgentPosition[s][d] - agents[s][d];
                            velocities[s][d] += rand.NextDouble(0.0, c) * px[d];
                            agents[s][d] = agents[s][d] + velocities[s][d];
                        }
                    }

                    if (!Problem.RunCondition.Continue(nEval, bestAgentFitness[best]))
                    {
                        //error= fabs(error - pb.objective);
                        goto end;
                    }

                    for (int d = 0; d < n; d++)
                    {
                        if (agents[s][d] < lowerBound[d])
                        {
                            agents[s][d] = lowerBound[d];
                            velocities[s][d] = 0;
                        }

                        if (agents[s][d] > upperBound[d])
                        {
                            agents[s][d] = upperBound[d];
                            velocities[s][d] = 0;
                        }
                    }

                    agentFitness[s] = Problem.Fitness(agents[s]);
                    nEval++;
                    // ... update the best previous position
                    if (agentFitness[s] < bestAgentFitness[s])	// Improvement
                    {
                        agents[s].CopyTo(bestAgentPosition[s], 0);
                        bestAgentFitness[s] = agentFitness[s];
                        // ... update the best of the bests
                        if (bestAgentFitness[s] < bestAgentFitness[best])
                        {
                            best = s;
                        }
                    }
                }			// End of "for (s0=0 ...  "	
                // Check if finished
                initLinks = bestAgentFitness[best] < errorPrev ? 0 : 1;
                errorPrev = bestAgentFitness[best];
                // Trace fitness of best found solution.
                Trace(nEval, bestAgentFitness[best]);
            end:
                noStop = Problem.RunCondition.Continue(nEval, bestAgentFitness[best]) ? 0 : 1;
            } // End of "while nostop ...

            // Return best-found solution and fitness.
            return new Result(bestAgentPosition[best], bestAgentFitness[best], nEval);
        }
        #endregion
        public double[] CalculateParameters(int dimensions, int numInformed)
        {
            /*
             * S := swarm size
             * K := maximum number of particles _informed_ by a given one
             * p := probability threshold of random topology, typically calculated from K
             * w := first cognitive/confidence coefficient
             * c := second cognitive/confidence coefficient
             */
            int S = (int)(10 + 2 * Math.Sqrt(dimensions));	// Swarm size
            int K = numInformed; //number of informed particles
            double p = 1 - Math.Pow(1 - (double)1 / (S), K); //Probability threshold of random topology
            // (to simulate the global best PSO, set p=1)

            // According to Clerc's Stagnation Analysis
            double w = 1 / (2 * Math.Log(2.0)); // 0.721
            double c = 0.5 + Math.Log(2.0); // 1.193
            return new[] {S, K, p, w, c};
        }
    
    }
}