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
    public class SPSO : Optimizer
    {
        private const int D_max = 114;		// Max number of dimensions of the search space
        private const int R_max = 500;		// Max number of runs
        private const int S_max = 910;		// Max swarm size
        private const int zero = 0;			// 1.0e-30 // To avoid numerical instabilities
        #region Constructors.
        /// <summary>
        /// Construct the object.
        /// </summary>
        public SPSO()
            : base()
        {
        }

        /// <summary>
        /// Construct the object.
        /// </summary>
        /// <param name="problem">Problem to optimize.</param>
        public SPSO(Problem problem)
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
        public Result Optimize()
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

            int K = (int)System.Math.Round(parameters[1], System.MidpointRounding.AwayFromZero);
            double p = parameters[2]; //This is what matters for informed particles
            double w = parameters[3];
            double c = parameters[4];

            //Initialize Random for each particle
            RandomOps.Random rInit = RandomOps.Random.GetNewInstance(RandomChoice);
            RandomOps.Random[] pRandoms = new RandomOps.Random[S];
            for(int h = 0; h< S;h++)
            {
                pRandoms[h] = RandomOps.Random.GetNewInstance(RandomChoice,rInit);
            }

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
            bool[,] links = new bool[S,n];
            double[] agentFitness = new double[S];
            double[] bestAgentFitness = new double[S];

            // Allocate velocity boundaries.
            double[] velocityLowerBound = new double[n];
            double[] velocityUpperBound = new double[n];

            // Best-found position and fitness.
            double[] g = null;
            double gFitness = Problem.MaxFitness;
            object gLock = new object();

            // Initialize
            System.Threading.Tasks.Parallel.For(0, n, l =>
                                   {
                                       double range = System.Math.Abs(upperBound[l] - lowerBound[l]);

                                       velocityLowerBound[l] = -range;
                                       velocityUpperBound[l] = range;
                                   });

            // Initialize all agents.
            // This counts as iterations below.
            for(int j = 0; j < S; j++)
            {

                // Refer to the j'th agent as x and v.
                double[] x = agents[j];
                double[] v = velocities[j];

                // Initialize.
                for (int m = 0; m < n; m++)
                {
                    x[m] = pRandoms[j].Uniform()*(upperInit[m] - lowerInit[m]) + lowerInit[m];
                    v[m] = ((pRandoms[j].Uniform()*
                           (velocityUpperBound[m] - velocityLowerBound[m]) +
                           velocityLowerBound[m]) - x[m])/2.0;
                    //TODO: Add quanitzation support for each dimension. Ex: Contrain each dimension to steps of .1
                }

                // Compute fitness of initial position.
                bestAgentFitness[j] = Problem.Fitness(x);

                // Initialize best known position.
                // Contents must be copied because the agent
                // may move to worse positions.
                x.CopyTo(bestAgentPosition[j], 0);

                // Update swarm's best known position.
                // This must reference the agent's best-known
                // position because the current position changes.
                    if (bestAgentFitness[j] < gFitness)
                    {

                        gFitness = bestAgentFitness[j];
                        g = bestAgentPosition[j];

                    }
                    // Trace fitness of best found solution.
                    Trace(j, gFitness);
                
            }

            // Perform actual optimization iterations. Start with numAgents to include initialization fitness evaluations in RunCondition check
            int i = S;
            bool initLinks = true;
            while(Problem.RunCondition.Continue(i, gFitness))
            {
                if(initLinks)
                {
                    for (int j = 0; j < S; j++)
                    {
                        for (int k = 0; k < S; k++)
                        {
                            links[j,k] = pRandoms[j].Uniform() < p ? true : false;
                        }
                        links[j, j] = true;//Always inform self
                    }
                    
                }
                for(int j = 0; j < S; j++,i++)
                {
                    // Refer to the j'th agent as x and v.
                    double[] x = agents[j];
                    double[] v = velocities[j];
                    double[] pBest = bestAgentPosition[j];


                    // Pick random weights.
                    double rP = pRandoms[j].Uniform();
                    double rG = pRandoms[j].Uniform();

                    //Find best informant
                    int lBest = j;
                    for(int m = 0; m < S; m++) 
			        {	    
				        if (links[j,m] && agentFitness[m] < agentFitness[lBest])
					        lBest = m;
			        }
                    double[] nBest = agents[lBest];

                    // Update velocity.
                    for (int k = 0; k < n; k++)
                    {
                        if(j != lBest)
                        {
                            v[k] = w*v[k] + c*rP*(pBest[k] - x[k]) +
                                   c*rG*(nBest[k] - x[k]);
                        }
                        else
                        {
                            v[k] = w*v[k] + c*rP*(pBest[k] - x[k]);
                        }
                    }

                    // Fix denormalized floating-point values in velocity.
                    Tools.Denormalize(ref v);

                    // Enforce velocity bounds before updating position.
                    Tools.Bound(ref v, velocityLowerBound, velocityUpperBound);

                    // Update position.
                    for (int k = 0; k < n; k++)
                    {
                        x[k] = x[k] + v[k];
                    }

                    // Enforce bounds before computing new fitness.
                    Tools.Bound(ref x, lowerBound, upperBound);

                    // Compute new fitness.
                    double newFitness = Problem.Fitness(x, bestAgentFitness[j]);

                    // Update best-known position in case of fitness improvement.
                    if (newFitness < bestAgentFitness[j])
                    {
                        // Update best-known position.
                        // Contents must be copied because the agent
                        // will likely move to worse positions.
                        x.CopyTo(bestAgentPosition[j], 0);
                        bestAgentFitness[j] = newFitness;

                        // Update swarm's best known position.
                        // This must reference the agent's best-known
                        // position because the current position changes.
                        if (newFitness < gFitness)
                        {
                            gFitness = bestAgentFitness[j];
                            g = bestAgentPosition[j];
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