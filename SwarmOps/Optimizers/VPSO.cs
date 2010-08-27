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
using Random = System.Random;

namespace SwarmOps.Optimizers
{
    /// <summary>
    /// Variable Particle Swarm Optimization (VPSO) variant from Clerc.
    /// This variant uses the following features:
    /// 1. Bi-Directional Ring Topology for particle social communication
    /// 2. Variable swarm size based on global minimum improvement rate.
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
    public class VPSO : Optimizer
    {
        #region Constructors.
        /// <summary>
        /// Construct the object.
        /// </summary>
        public VPSO()
            : base()
        {
        }

        /// <summary>
        /// Construct the object.
        /// </summary>
        /// <param name="problem">Problem to optimize.</param>
        public VPSO(Problem problem)
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
            int SMax = 200; //Maximum number of particles in a swarm.
            Debug.Assert(S > 0);

            double p = parameters[2]; //This is what matters for informed particles
            double w = parameters[3];
            double c = parameters[4];
            //Memory
            Memory memory = new Memory(500);

            //TODO: Initialize Random for each particle
            Random rand = new Random();

            // Get problem-context.
            double[] lowerBound = Problem.LowerBound;
            double[] upperBound = Problem.UpperBound;
            double[] lowerInit = Problem.LowerInit;
            double[] upperInit = Problem.UpperInit;
            int n = Problem.Dimensionality;

            // Allocate agent positions and associated fitnesses.
            double[][] agents = Tools.NewMatrix(SMax, n);
            double[][] velocities = Tools.NewMatrix(SMax, n);
            double[][] bestAgentPosition = Tools.NewMatrix(SMax, n);

            int[,] links = new int[SMax, SMax];
            int[] index = new int[SMax];
            int g;
            double[] px = new double[n];
            double[] gx = new double[n];
            int nEval = 0;

            double[] agentFitness = new double[SMax];
            double[] bestAgentFitness = new double[SMax];

            double spreadProba = 0.9d;
            int spreadFormula = 3;
            int spread = spreadIter(spreadProba,S,spreadFormula);
            int improvTot = 0;

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
                memory.Save(agents[s]);
            }

            // First evaluations
            for (int s = 0; s < S; s++)
            {
                agentFitness[s] = Problem.Fitness(agents[s]);
                nEval++;
                agents[s].CopyTo(bestAgentPosition[s], 0);	// Best position = current one
                bestAgentFitness[s] = agentFitness[s];
                //Initialize index
                index[s] = s;
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
            int initLinkNb = 0;
            int noStop = 0;
            // ---------------------------------------------- ITERATIONS
            int sWorst;
            int stagnation = 0;
            int swarmMod;
            while (noStop == 0)
            {
                index = Enumerable.Range(0, S).ToArray();
                // Random numbering of the particles
                Tools.Shuffle(ref index);

                if (initLinks == 1)	// Bidirectional ring topology. Randomly built
                {
                    initLinks = 0;
                    initLinkNb = 0; // Count the number of iterations since the last re-init of the links

                    // Init to zero (no link)
                    for (int s = 0; s < S; s++)
                    {
                        for (int m = 0; m < S; m++)
                        {
                            links[m, s] = 0;
                        }
                    }
                    for (int s = 0; s < S-1; s++)
                    {
                        // Information links (bidirectional ring)
                        links[index[s],index[s + 1]] = 1;
                        links[index[s + 1],index[s]] = 1;
                    }
                    for (int s = 0; s < S; s++)
                    {
                        // Each particle informs itself
                        links[s, s] = 1;
                    }
                    links[index[0],index[S - 1]] = 1;
                    links[index[S - 1],index[0]] = 1;
                }
                // Loop on particles, for move
                improvTot = 0;
                // The swarm MOVES
                for (int s0 = 0; s0 < S; s0++)	// For each particle ...
                {
                    int s = index[s0];

                    // Find the best informant			
                    g = s;
                    for (int m = 0; m < S; m++)
                    {
                        if (m == s) continue;
                        if (links[m, s] == 1 && bestAgentFitness[m] < bestAgentFitness[g])
                            g = m;
                    }

                    //.. compute the new velocity, and move
                    // Exploration tendency
                    if (g != s)
                    {
                        for (int d = 0; d < n; d++)
                        {
                            velocities[s][d] = w * velocities[s][d];
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
                        memory.Save(agents[s]);
                        agents[s].CopyTo(bestAgentPosition[s], 0);
                        bestAgentFitness[s] = agentFitness[s];

                        improvTot++;
                        // ... update the best of the bests
                        if (bestAgentFitness[s] < bestAgentFitness[best])
                        {
                            best = s;
                        }
                    }
                }			// End of "for (s0=0 ...  "	
                /*-------------------------------------------------- Adaptations
		         Rule 1:
		         Check every "spread" iterations after each re-init of the links
		         If no improvement of the global best
		         => reinit links before the next iteration

		         Rule 2:
		         if no improvement of the global best during "spread" iterations
		         => Try to add a particle (and initialise it in a non-searched area)
		         => re-init links before the next iteration
		         Note that the condition is slightly different from the one of Rule 1

		         Rule 3:
		         if "enough" local improvements during the iteration
		         => try to remove a particle (keep at least D+1 ones)

		         */

                // Rule 1 - Re-initializing the information links
                // Check if improvement since last re-init of the links
                initLinkNb = initLinkNb + 1; // Number of iterations since the last check 

                if (initLinkNb >= spread) // It's time to check
                {
                    initLinkNb = 0; // Reset to zero the number of iterations since the last check
                    // The swarm size may have been modified, so must be "spread"                    
                    spread = spreadIter(spreadProba, S, spreadFormula);

                    initLinks = bestAgentFitness[best] < errorPrev ? 0 : 1;	
                }
                else 
                {
                    initLinks = 0;  // To early, no need to check 
                }

                sWorst = worst(bestAgentFitness,S); // Rank of the worst particle, before any adaptation

                // Rule 2 - Adding a particle
                // Check global stagnation (improvement of the global best) 
                if (bestAgentFitness[best] < errorPrev) stagnation = 0;	// Improvement
                else stagnation++; // No improvement during this iteration

                swarmMod = 0; // Information flag
                int sVal = 0;
                if (stagnation >= spread)  // Too many iterations without global improvement =>  add a particle
                {
                    if (S < SMax) // if not too many particles
                    {
                        sVal = S;
                        agents[sVal] = memory.InitializeFar(); // Init in a non-searched area
                        agentFitness[sVal] = Problem.Fitness(agents[sVal]);	 // Evaluation
                        nEval++;
                        for (int d = 0; d < n; d++)
                        {
                            velocities[sVal][d] = (rand.NextDouble(lowerBound[d], upperBound[d]) - agents[sVal][d]) / 2;// Init velocity						
                        }
                        agents[sVal].CopyTo(bestAgentPosition[sVal],0); // Previous best = current position								
                        S = S + 1; // Increase the swarm size

                        initLinks = 1; // Links will be reinitialised
                        stagnation = 0; // Reset the count for stagnation
                        swarmMod = 1; // A particle has been added
                    }
                }

                // Rule 3 - Removing a particle
                // If enough improvements of some particles, remove the worst 
                // (but keep at least D+1 particles)
                // NOTE: this is "the worst" without taking into account the particle
                // that has (possibly) been added
                // NOTE: it is perfectly possible to have a particle added
                // (because of no improvement of the global best)  AND
                // a particle removed (because enough _local_ improvements)

                
                if (S > Problem.Dimensionality + 1 && improvTot > 0.5 * S)
                {
                    if ((swarmMod == 0 && sWorst < S - 1) || swarmMod == 1)
                    // if the worst is not the last 
                    {
                        bestAgentPosition[sWorst] = (double[])bestAgentPosition[S - 1].Clone(); // ... replace it by the last
                        bestAgentFitness[sWorst] = bestAgentFitness[S - 1];
                        velocities[sWorst] = (double[])velocities[S - 1].Clone();
                        agents[sWorst] = (double[])agents[S - 1].Clone();

                        // Compact the matrix of the links
                        for (int s1 = 0; s1 < S; s1++)  // For each line, compact the columns
                            for (int s2 = sWorst; s2 < S - 1; s2++) links[s1,s2] = links[s1,s2 + 1];

                        for (int s2 = 0; s2 < S - 1; s2++)	// For each column, compact the lines
                            for (int s1 = sWorst; s1 < S - 1; s1++) links[s1,s2] = links[s1 + 1,s2];
                    }
                    S = S - 1; // Decrease the swarm size
                    if (sVal < best) best = best - 1; // The rank of the best may have been modified
                }

                // Check if finished
                //initLinks = bestAgentFitness[best] < errorPrev ? 0 : 1;
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
        int spreadIter(double spreadProba, int S, int formula)
        {
            double val;
            // Number of iterations to spread information
            switch(formula)
            {
	            case 2:
                    val = 0.5 + Math.Log(1.0 - spreadProba)/Math.Log(1.0 - 2.0/S);
	            return (int)val;
	            case 3:
                    val = 0.5 + Math.Log(1.0 - spreadProba)/Math.Log(1.0 - 3.0/S);
	            return (int)val;
                default: // 1
                    val = Math.Ceiling(0.5 + spreadProba * 0.5 * S);
	            return (int)val;
            }
        }
        int worst(double[] agentFitness, int size)
        {
	        // Find the rank of the worst position
	        int worst = 0;

	        for (int s = 1; s < size; s++)     
	        {
		        if(agentFitness[worst] > agentFitness[s])
			        worst = s;
	        }
	        return worst;
        }
        int best(double[] agentFitness, int size)
        {
	        // Find the rank of the best position
	        // 	Remember that f is fabs(fitness-objective)
	        // 	We want to minimise it
	        int best = 0;
	        for (int s = 1; s < size; s++)     
	        {
		        if(agentFitness[s] < agentFitness[best])
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
				Positions[Rank]=(double[])P.Clone();

				if(Size<MaxSize-1)
				{
					Size++;
					Rank++;
				}
				else Rank=0; // We re-use the memory cyclically 

            }
            public double[] InitializeFar()
            {
                if(Size < 2)
                    throw new InvalidOperationException("Can't Initialize until there are at least two values in memory.");
                // Try to find a new position that is "far" from all the memorised ones

	            //Note: memPos is a global variable
	            double[] coord = new double[MaxSize];
                int dimensionality = Positions[0].Length;
	            double delta;
	            double[] interv = new double[2];

	            int n;
	            double[] XFar = new double[dimensionality];;

	            for(int d=0;d<dimensionality;d++) // For each dimension
	            {

	                for(n=0;n<Size;n++)
                        coord[n]=Positions[n][d]; // All the coordinates on this dimension
	
		            Array.Sort(coord); // Sort them by increasing order

		            // Find the biggest intervall
		            interv[0]=coord[0];
		            interv[1]=coord[1];
		            delta=interv[1]-interv[0];

		            for(n=1;n<Size-1;n++)
		            {
			            if(coord[n+1]-coord[n] < delta) continue;

			            interv[0]=coord[n];
			            interv[1]=coord[n+1];
			            delta=interv[1]-interv[0];
		            }

		            XFar[d]=0.5*(interv[1]+interv[0]); // Take the middle

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