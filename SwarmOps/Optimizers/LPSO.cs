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

namespace SwarmOps.Optimizers
{
    /// <summary>
    /// Standard Particle Swarm Optimization (SPSO) originally due to
    /// Eberhart et al. This the 'Standard PSO' variant which uses 
    /// neighborhood best and can have its  parameters tuned (or 
    /// meta-optimized) to work well on a range of optimization problems. 
    /// 
    /// Typically when N neighborhood size = S the number of particles in
    /// the swarm this variant reverts to the 'Plain Vanilla' gBest
    /// performance.
    /// </summary>
    /// <remarks>
    /// References:
    /// (1) Eberhart, R. C. and Kennedy, J. A new optimizer using particle
    ///     swarm theory. Proceedings of the Sixth International Symposium
    ///     on Micromachine and Human Science, Nagoya, Japan pp. 39-43.
    /// (2) J. Kennedy and R. Eberhart. Particle swarm optimization.
    ///     In Proceedings of IEEE International Conference on Neural
    ///     Networks, volume IV, pages 1942-1948, Perth, Australia, 1995
    /// </remarks>
    public class LPSO : Optimizer
    {
        #region Constructors.
        /// <summary>
        /// Construct the object.
        /// </summary>
        public LPSO()
            : base()
        {
        }

        /// <summary>
        /// Construct the object.
        /// </summary>
        /// <param name="problem">Problem to optimize.</param>
        public LPSO(Problem problem)
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
            /// S = Number of particles
            /// N = Neighborhood size
            /// 
            /// </summary>
            public static readonly double[] HandTuned = { 50.0, 5.0, 0.72984378812835756567558911626891, 1.49445, 1.49445 };
        }
        #endregion

        #region Get control parameters.
        /// <summary>
        /// Get parameter, Number of agents, aka. swarm-size.
        /// </summary>
        /// <param name="parameters">Optimizer parameters.</param>
        public int GetNumAgents(double[] parameters)
        {
            return (int)System.Math.Round(parameters[0], System.MidpointRounding.AwayFromZero);
        }

        public int GetNeighborhoodSize(double[] parameters)
        {
            return (int) System.Math.Round(parameters[1], System.MidpointRounding.AwayFromZero);
        }
        /// <summary>
        /// Get parameter, Omega.
        /// </summary>
        /// <param name="parameters">Optimizer parameters.</param>
        public double GetOmega(double[] parameters)
        {
            return parameters[2];
        }

        /// <summary>
        /// Get parameter, PhiP.
        /// </summary>
        /// <param name="parameters">Optimizer parameters.</param>
        public double GetPhiP(double[] parameters)
        {
            return parameters[3];
        }

        /// <summary>
        /// Get parameter, PhiG.
        /// </summary>
        /// <param name="parameters">Optimizer parameters.</param>
        public double GetPhiG(double[] parameters)
        {
            return parameters[4];
        }
        public RandomAlgorithm RandomChoice { get; set; }
        #endregion

        #region Base-class overrides, Problem.
        /// <summary>
        /// Name of the optimizer.
        /// </summary>
        public override string Name
        {
            get { return "LPSO"; }
        }

        /// <summary>
        /// Number of control parameters for optimizer.
        /// </summary>
        public override int Dimensionality
        {
            get { return 5; }
        }

        string[] _parameterName = { "S", "N", "omega", "phi_p", "phi_g" };

        /// <summary>
        /// Control parameter names.
        /// </summary>
        public override string[] ParameterName
        {
            get { return _parameterName; }
        }

        static readonly double[] _defaultParameters = Parameters.HandTuned;

        /// <summary>
        /// Default control parameters.
        /// </summary>
        public override double[] DefaultParameters
        {
            get { return _defaultParameters; }
        }

        static readonly double[] _lowerBound = { 1.0, 0.0, -2.0, -4.0, -4.0 };

        /// <summary>
        /// Lower search-space boundary for control parameters.
        /// </summary>
        public override double[] LowerBound
        {
            get { return _lowerBound; }
        }

        static readonly double[] _upperBound = { 200.0, 200.0, 2.0, 4.0, 4.0 };

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

            // Retrieve parameter specific to SPSO method.
            int numAgents = GetNumAgents(parameters);
            Debug.Assert(numAgents > 0);

            int neighborhoodSize = GetNeighborhoodSize(parameters) > numAgents?numAgents:GetNeighborhoodSize(parameters);
            double omega = GetOmega(parameters);
            double phiP = GetPhiP(parameters); // phi1
            double phiG = GetPhiG(parameters); // phi2

            //Initialize Random for each particle
            //Initialize Random for each particle
            RandomOps.Random rInit = RandomOps.Random.GetNewInstance(RandomChoice);
            RandomOps.Random[] pRandoms = new RandomOps.Random[numAgents];
            for (int h = 0; h < numAgents; h++)
            {
                pRandoms[h] = RandomOps.Random.GetNewInstance(RandomChoice, rInit);
            }

            // Get problem-context.
            double[] lowerBound = Problem.LowerBound;
            double[] upperBound = Problem.UpperBound;
            double[] lowerInit = Problem.LowerInit;
            double[] upperInit = Problem.UpperInit;
            int n = Problem.Dimensionality;

            // Allocate agent positions and associated fitnesses.
            double[][] agents = Tools.NewMatrix(numAgents, n);
            double[][] velocities = Tools.NewMatrix(numAgents, n);
            double[][] bestAgentPosition = Tools.NewMatrix(numAgents, n);
            double[][] bestAgentNeighborhoodPosition = Tools.NewMatrix(numAgents, n);

            double[] bestAgentNeighborhoodFitness = new double[numAgents];
            double[] bestAgentFitness = new double[numAgents];

            // Allocate velocity boundaries.
            double[] velocityLowerBound = new double[n];
            double[] velocityUpperBound = new double[n];

            // Iteration variables.
            int i;

            // Best-found position and fitness.
            double[] g = null;
            double gFitness = Problem.MaxFitness;
            object gLock = new object();

            //Initialize bestAgentNeighborhoodFitness and BestAgentFitness to max
            System.Threading.Tasks.Parallel.For(0, numAgents, l =>
                                           {
                                               bestAgentFitness[l] = Problem.MaxFitness;
                                               bestAgentNeighborhoodFitness[l] = Problem.MaxFitness;
                                           });

            // Initialize velocity boundaries.
            System.Threading.Tasks.Parallel.For(0, n, l =>
                                   {
                                       double range = System.Math.Abs(upperBound[l] - lowerBound[l]);

                                       velocityLowerBound[l] = -range;
                                       velocityUpperBound[l] = range;
                                   });

            // Initialize all agents.
            Tools.InitializeSOA(ref agents, lowerInit, upperInit);
            // This counts as iterations below.
            for(int j = 0; j < numAgents; j++)
            {
                // Refer to the j'th agent as x and v.
                double[] x = agents[j];
                double[] v = velocities[j];

                // Initialize agent-position in search-space.
                for (int m = 0; m < n; m++)
                {
                    //x[m] = pRandoms[j].NextDouble()*(upperInit[m] - lowerInit[m]) + lowerInit[m];
                    v[m] = pRandoms[j].Uniform()*
                            (velocityUpperBound[m] - velocityLowerBound[m]) +
                            velocityLowerBound[m];
                }
                //Tools.InitializeUniform(ref x, lowerInit, upperInit);

                // Initialize velocity.
                //Tools.InitializeUniform(ref v, velocityLowerBound, velocityUpperBound);

                // Compute fitness of initial position.
                bestAgentFitness[j] = Problem.Fitness(x);

                // Initialize best known position.
                // Contents must be copied because the agent
                // will likely move to worse positions.
                x.CopyTo(bestAgentPosition[j], 0);

                // Update swarm's best known position.
                // This must reference the agent's best-known
                // position because the current position changes.
                for (int l = 0; l < neighborhoodSize; l++)
                {
                    if (bestAgentFitness[j] <
                        bestAgentNeighborhoodFitness[(l + j)%numAgents])
                    {
                        bestAgentPosition[(l + j)%numAgents].CopyTo(bestAgentNeighborhoodPosition[j], 0);
                        bestAgentNeighborhoodFitness[(l + j)%numAgents] =bestAgentFitness[j];
                    }
                }
                    if (bestAgentFitness[j] < gFitness)
                    {

                        gFitness = bestAgentFitness[j];
                        g = bestAgentPosition[j];

                    }
                    // Trace fitness of best found solution.
                    Trace(j, gFitness);
                
            }

            // Perform actual optimization iterations. Start with numAgents to include initialization fitness evaluations in RunCondition check
            i = numAgents;
            while(Problem.RunCondition.Continue(i, gFitness))
            {
                for(int j = 0; j < numAgents; j++,i++)
                {
                    // Refer to the j'th agent as x and v.
                    double[] x = agents[j];
                    double[] v = velocities[j];
                    double[] p = bestAgentPosition[j];
                    double[] nBest = bestAgentNeighborhoodPosition[j];

                    // Pick random weights.
                    double rP = pRandoms[j].Uniform();
                    double rG = pRandoms[j].Uniform();

                    // Update velocity.
                    for (int k = 0; k < n; k++)
                    {
                        v[k] = omega*v[k] + phiP*rP*(p[k] - x[k]) +
                                phiG*rG*(nBest[k] - x[k]);
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
                        for (int l = 0; l < neighborhoodSize; l++)
                        {
                            if (bestAgentFitness[j] <
                                bestAgentNeighborhoodFitness[(l + j)%numAgents])
                            {
                                bestAgentPosition[j].CopyTo(
                                    bestAgentNeighborhoodPosition[(l + j)%numAgents], 0);
                                bestAgentNeighborhoodFitness[(l + j)%numAgents] =
                                    bestAgentFitness[j];
                            }
                        }
       
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
    }
}