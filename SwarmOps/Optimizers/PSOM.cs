/// ------------------------------------------------------
/// SwarmOps - Numeric and heuristic optimization for C#
/// Published under the GNU Lesser General Public License.
/// Please see the file license.txt for license details.
/// SwarmOps on the internet: http://www.Hvass-Labs.org/
/// ------------------------------------------------------

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;
using System.Linq.Expressions;
using RandomOps;


namespace SwarmOps.Optimizers
{
    /// <summary>
    /// Particle Swarm Optimization Model with Centroid (PSOM)
    /// This variant uses the following features:
    /// 1. Adds a fourth term which involves the centroid of the particle positions and pbest positions
    /// 
    /// NOTE:benchmark results showed a degredation of performance on the mini-benchmark problems
    /// </summary>
    /// <remarks>
    /// References:
    /// 1. Song et al. Improved Particle Swarm Cooperative Optimization Algorithm Based on Chaos & Simplex Method. 
    /// 2010 Second International Workshop on Education Technology and Computer Science (2010) pp. 45-48
    /// </remarks>
    public class PSOM : Optimizer
    {
        #region Constructors.
        /// <summary>
        /// Construct the object.
        /// </summary>
        public PSOM()
            : base()
        {
        }

        /// <summary>
        /// Construct the object.
        /// </summary>
        /// <param name="problem">Problem to optimize.</param>
        public PSOM(Problem problem)
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
            public static readonly double[] HandTuned = { 50.0, 3.0, 1.0, 0.72984, 1.193 };
        }
        #endregion

        #region Base-class overrides, Problem.
        /// <summary>
        /// Name of the optimizer.
        /// </summary>
        public override string Name
        {
            get { return "PSOM"; }
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
            get { return this.CalculateParameters(30, 3); }
        }

        static readonly double[] _lowerBound = { 1.0, 0.0, 1.0, -2.0, -4.0 };

        /// <summary>
        /// Lower search-space boundary for control parameters.
        /// </summary>
        public override double[] LowerBound
        {
            get { return _lowerBound; }
        }

        static readonly double[] _upperBound = { 200.0, 200.0, 0.0, 2.0, 4.0 };

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

            // Retrieve parameter specific to PSO method.
            int numAgents = (int)System.Math.Round(parameters[0], System.MidpointRounding.AwayFromZero); ;
            double omega = parameters[3];
            double phiP = parameters[4]; // phi1
            double phiG = parameters[4]; // phi2
            double phiC = parameters[4]; // phi for centroid term

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
            double[] bestAgentFitness = new double[numAgents];

            // Allocate velocity boundaries.
            double[] velocityLowerBound = new double[n];
            double[] velocityUpperBound = new double[n];

            // Iteration variables.
            int i, j;

            //Random
            RandomOps.Random rand = new MersenneTwister();

            //Centroids
            double[] xCentroid;
            double[] pCentroid;

            // Best-found position and fitness.
            double[] g = null;
            double gFitness = Problem.MaxFitness;

            // Initialize velocity boundaries.
            System.Threading.Tasks.Parallel.For(0, n, l =>
            {
                double range = System.Math.Abs(upperBound[l] - lowerBound[l]);

                velocityLowerBound[l] = -range;
                velocityUpperBound[l] = range;
            });

            // Initialize all agents.
            // This counts as iterations below.));
            for (j = 0; j < numAgents; j++)
            {
                // Refer to the j'th agent as x and v.
                double[] x = agents[j];
                double[] v = velocities[j];

                // Initialize agent-position in search-space.
                Tools.InitializeUniform(ref x, lowerInit, upperInit);

                // Initialize velocity.
                Tools.InitializeUniform(ref v, velocityLowerBound, velocityUpperBound);

                // Compute fitness of initial position.
                bestAgentFitness[j] = Problem.Fitness(x);

                // Initialize best known position.
                // Contents must be copied because the agent
                // will likely move to worse positions.
                x.CopyTo(bestAgentPosition[j], 0);

                // Update swarm's best known position.
                // This must reference the agent's best-known
                // position because the current position changes.
                if (g == null || bestAgentFitness[j] < gFitness)
                {
                    g = bestAgentPosition[j];
                    gFitness = bestAgentFitness[j];
                }

                // Trace fitness of best found solution.
                Trace(j, gFitness);
            }

            // Perform actual optimization iterations.
            for (i = numAgents; Problem.RunCondition.Continue(i, gFitness); )
            {
                Debug.Assert(numAgents > 0);

                //Calculate Centroids before update loop
                xCentroid = Enumerable.Repeat(0.0, n).ToArray();
                pCentroid = Enumerable.Repeat(0.0, n).ToArray();

                for(int s = 0; s < numAgents; s++)
                {
                    for (int d = 0; d < n; d++)
                    {
                        xCentroid[d] += agents[s][d];
                        pCentroid[d] += bestAgentPosition[s][d];
                    }
                }
                for (int d = 0; d < n; d++)
                {
                    xCentroid[d] /= n;
                    pCentroid[d] /= n;
                }

                //Loop through the agents and move.
                for (j = 0; j < numAgents && Problem.RunCondition.Continue(i, gFitness); j++, i++)
                {
                    // Refer to the j'th agent as x and v.
                    double[] x = agents[j];
                    double[] v = velocities[j];
                    double[] p = bestAgentPosition[j];
                    
                    // Update velocity.
                    for (int d = 0; d < n; d++)
                    {
                        v[d] = omega * v[d] 
                            + phiP * rand.Uniform() * (p[d] - x[d]) 
                            + phiG * rand.Uniform() * (g[d] - x[d]) 
                            + phiC * rand.Uniform() * (0.5*xCentroid[d] + 0.5*pCentroid[d] - x[d]); //TODO: setup weight adjustment factor as a parameter
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
                            g = bestAgentPosition[j];
                            gFitness = bestAgentFitness[j];
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
            int S = (int)(40 + 2 * Math.Sqrt(dimensions));	// Swarm size 
            int K = numInformed; //number of informed particles
            double p = 1 - Math.Pow(1 - (double)1 / (S), K); //Probability threshold of random topology
            // (to simulate the global best PSO, set p=1)

            // According to Clerc's Stagnation Analysis
            double w = 1 / (2 * Math.Log(2.0)); // 0.721
            double c = 0.5 + Math.Log(2.0); // 1.193
            return new[] { S, K, p, w, c };
        }
        int spreadIter(double spreadProba, int S, int formula)
        {
            double val;
            // Number of iterations to spread information
            switch (formula)
            {
                case 2:
                    val = 0.5 + Math.Log(1.0 - spreadProba) / Math.Log(1.0 - 2.0 / S);
                    return (int)val;
                case 3:
                    val = 0.5 + Math.Log(1.0 - spreadProba) / Math.Log(1.0 - 3.0 / S);
                    return (int)val;
                default: // 1
                    val = Math.Ceiling(0.5 + spreadProba * 0.5 * S);
                    return (int)val;
            }
        }
        static int worst(double[] agentFitness, int size)
        {
            // Find the rank of the worst position
            int worst = 0;

            for (int s = 1; s < size; s++)
            {
                if (agentFitness[worst] > agentFitness[s])
                    worst = s;
            }
            return worst;
        }
        static int GetBest(double[] agentFitness, int size)
        {
            // Find the rank of the best position
            // 	Remember that f is fabs(fitness-objective)
            // 	We want to minimise it
            int best = 0;
            for (int s = 1; s < size; s++)
            {
                if (agentFitness[s] < agentFitness[best])
                    best = s;
            }
            return best;
        }

        private class Memory
        {
            public int MaxSize { get; private set; }
            public int Size { get; set; }
            public int Rank { get; set; }
            public double[][] Positions { get; private set; }
            public Memory(int maxSize)
            {
                MaxSize = maxSize;
                Positions = new double[maxSize][];
                Size = 0;
                Rank = 0;
            }
            public void Save(double[] P)
            {
                // Save a position
                // Is useful to generate a new particle in a promising area
                // The Positions list is a global variable
                Positions[Rank] = (double[])P.Clone();

                if (Size < MaxSize - 1)
                {
                    Size++;
                    Rank++;
                }
                else Rank = 0; // We re-use the memory cyclically 

            }
            public double[] InitializeFar()
            {
                if (Size < 2)
                    throw new InvalidOperationException("Can't Initialize until there are at least two values in memory.");
                // Try to find a new position that is "far" from all the memorised ones

                //Note: memPos is a global variable
                double[] coord = new double[MaxSize];
                int dimensionality = Positions[0].Length;
                double delta;
                double[] interv = new double[2];

                int n;
                double[] XFar = new double[dimensionality]; ;

                for (int d = 0; d < dimensionality; d++) // For each dimension
                {

                    for (n = 0; n < Size; n++)
                        coord[n] = Positions[n][d]; // All the coordinates on this dimension

                    Array.Sort(coord); // Sort them by increasing order

                    // Find the biggest intervall
                    interv[0] = coord[0];
                    interv[1] = coord[1];
                    delta = interv[1] - interv[0];

                    for (n = 1; n < Size - 1; n++)
                    {
                        if (coord[n + 1] - coord[n] < delta) continue;

                        interv[0] = coord[n];
                        interv[1] = coord[n + 1];
                        delta = interv[1] - interv[0];
                    }

                    XFar[d] = 0.5 * (interv[1] + interv[0]); // Take the middle

                    //NOTE: The caller is responsible for bounds checks and fitness evaluation
                    // Particular case, xMax
                    //if(pb.SS.max[d]-coord[memPos.size-1] > delta)
                    //{
                    //    XFar.x[d]=pb.SS.max[d];
                    //    delta=pb.SS.max[d]-coord[memPos.size-1]; 
                    //}

                    //// Particular case, xMin
                    //if(coord[0]-pb.SS.min[d]> delta)
                    //{
                    //    XFar.x[d]=pb.SS.min[d];
                    //}
                }

                //XFar=discrete(XFar, pb);
                //XFar.f=perf (XFar, pb);
                return XFar;
            }
        }

    }
}