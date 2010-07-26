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

namespace SwarmOps.Optimizers
{
    /// <summary>
    /// Meta Particle Swarm Optimization (MPSO) 
    /// This the 'MPSO' variant uses a swarm of swarms.
    /// I have added in the neighborhood size concepts when N = numParticles the algorithm reverts to gBest characteristics.
    /// 
    /// Meta PSO (MPSO) uses two indicies j=1...NumSwarms and i=1...NumParticles in the swarm.
    /// The velocity update then becomes
    /// v[k] = omega * v[k] + phiP * rP * (p[k] - x[k]) + phiS * rS * (sBest[k] - x[k]) + phiN * rN * (nBest[k] - x[k]);
    /// 
    /// </summary>
    /// <remarks>
    /// References:
    /// (1) 
    /// </remarks>
    //TODO: How does this compare to pbest, nbest, gbest with nSize = swarm size of this algorithm, if they are equivalent then why the complication of multiple swarms.
    public class MPSO : Optimizer
    {
        #region Constructors.
        /// <summary>
        /// Construct the object.
        /// </summary>
        public MPSO()
            : base()
        {
        }

        /// <summary>
        /// Construct the object.
        /// </summary>
        /// <param name="problem">Problem to optimize.</param>
        public MPSO(Problem problem)
            : base(problem)
        {
            //TODO: This class is currently hardwired for minimization. Fix and allow for maximization as well based on the problem.
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
            public static readonly double[] HandTuned = { 20.0, 50.0, 2.0, 0.72984378812835756567558911626891, 1.366666666, 1.366666666, 1.366666666 };
        }
        #endregion

        #region Get control parameters.
        /// <summary>
        /// Get parameter, Number of swarm
        /// </summary>
        /// <param name="parameters">Optimizer parameters.</param>
        public int GetNumSwarms(double[] parameters)
        {
            return (int)System.Math.Round(parameters[0], System.MidpointRounding.AwayFromZero);
        }
        /// <summary>
        /// Get parameter, Number of agents, aka. swarm-size.
        /// </summary>
        /// <param name="parameters">Optimizer parameters.</param>
        public int GetNumAgents(double[] parameters)
        {
            return (int)System.Math.Round(parameters[1], System.MidpointRounding.AwayFromZero);
        }

        public int GetNeighborhoodSize(double[] parameters)
        {
            return (int) System.Math.Round(parameters[2], System.MidpointRounding.AwayFromZero);
        }
        /// <summary>
        /// Get parameter, Omega.
        /// </summary>
        /// <param name="parameters">Optimizer parameters.</param>
        public double GetOmega(double[] parameters)
        {
            return parameters[3];
        }

        /// <summary>
        /// Get parameter, PhiP.
        /// </summary>
        /// <param name="parameters">Optimizer parameters.</param>
        public double GetPhiP(double[] parameters)
        {
            return parameters[4];
        }

        /// <summary>
        /// Get parameter, PhiS.
        /// </summary>
        /// <param name="parameters">Optimizer parameters.</param>
        public double GetPhiS(double[] parameters)
        {
            return parameters[5];
        }

        /// <summary>
        /// Get parameter, PhiS.
        /// </summary>
        /// <param name="parameters">Optimizer parameters.</param>
        public double GetPhiN(double[] parameters)
        {
            return parameters[6];
        }
        #endregion

        #region Base-class overrides, Problem.
        /// <summary>
        /// Name of the optimizer.
        /// </summary>
        public override string Name
        {
            get { return "MPSO"; }
        }

        /// <summary>
        /// Number of control parameters for optimizer.
        /// </summary>
        public override int Dimensionality
        {
            get { return 7; }
        }

        string[] _parameterName = { "S", "N", "omega", "phi_p", "phi_s", "phi_n" };

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

        static readonly double[] _lowerBound = { 1.0, 1.0, 0.0, -2.0, -5.0, -5.0, -5.0 };

        /// <summary>
        /// Lower search-space boundary for control parameters.
        /// </summary>
        public override double[] LowerBound
        {
            get { return _lowerBound; }
        }

        static readonly double[] _upperBound = { 100.0, 200.0, 200.0, 2.0, 5.0, 5.0, 5.0 };

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

            // Retrieve parameter specific to MPSO method.
            int numAgents = GetNumAgents(parameters);
            int numSwarms = GetNumSwarms(parameters);
            Debug.Assert(numAgents > 0);

            int neighborhoodSize = GetNeighborhoodSize(parameters) > numAgents?numAgents:GetNeighborhoodSize(parameters);
            double omega = GetOmega(parameters);
            double phiP = GetPhiP(parameters); // phi1
            double phiS = GetPhiS(parameters); // phi2
            double phiN = GetPhiN(parameters); //phi3


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
            double[][] bestSwarmPosition = Tools.NewMatrix(numSwarms, n);
            double[][] bestAgentNeighborhoodPosition = Tools.NewMatrix(numAgents, n);
            double[] bestAgentNeighborhoodFitness = new double[numAgents];
            double[] bestAgentFitness = new double[numAgents];

            // Allocate velocity boundaries.
            double[] velocityLowerBound = new double[n];
            double[] velocityUpperBound = new double[n];

            // Iteration variables.
            int i, j;

            // Best-found position and fitness.
            double[] g = null;
            double gFitness = Problem.MaxFitness;

            //Initialize bestAgentNeighborhoodFitness and BestAgentFitness to max
            Parallel.For(0, numAgents, l =>
                                           {
                                               bestAgentFitness[l] = Problem.MaxFitness;
                                               bestAgentNeighborhoodFitness[l] = Problem.MaxFitness;
                                           });

            // Initialize velocity boundaries.
            Parallel.For(0, n, l =>
                                   {
                                       double range = System.Math.Abs(upperBound[l] - lowerBound[l]);

                                       velocityLowerBound[l] = -range;
                                       velocityUpperBound[l] = range;
                                   });

            // Initialize all agents.
            // This counts as iterations below.
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
                for (int l = 0; l < neighborhoodSize; l++)
                {
                    if (bestAgentFitness[j] < bestAgentNeighborhoodFitness[(l+j)%numAgents])
                    {
                        bestAgentPosition[(l + j) % numAgents].CopyTo(bestAgentNeighborhoodPosition[j], 0);
                        bestAgentNeighborhoodFitness[(l + j) % numAgents] = bestAgentFitness[j];
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
           // for(int s = 0,GetNumSwarms())
            for (i = numAgents; Problem.RunCondition.Continue(i, gFitness); )
            {
                for (j = 0; j < numAgents && Problem.RunCondition.Continue(i, gFitness); j++, i++)
                {
                    // Refer to the j'th agent as x and v.
                    double[] x = agents[j];
                    double[] v = velocities[j];
                    double[] p = bestAgentPosition[j];
                    double[] sBest = bestSwarmPosition[i];
                    double[] nBest = bestAgentNeighborhoodPosition[j];

                    // Pick random weights.
                    double rP = Globals.Random.Uniform();
                    double rS = Globals.Random.Uniform();
                    double rN = Globals.Random.Uniform();

                    // Update velocity.)
                    for (int k = 0; k < n; k++)
                    {
                        v[k] = omega * v[k] + phiP * rP * (p[k] - x[k]) + phiS * rS * (sBest[k] - x[k]) + phiN * rN * (nBest[k] - x[k]);
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
                            if (bestAgentFitness[j] < bestAgentNeighborhoodFitness[(l + j) % numAgents])
                            {
                                bestAgentPosition[j].CopyTo(bestAgentNeighborhoodPosition[(l + j) % numAgents], 0);
                                bestAgentNeighborhoodFitness[(l + j) % numAgents] = bestAgentFitness[j];
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