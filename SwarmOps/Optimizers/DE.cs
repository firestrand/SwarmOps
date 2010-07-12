/// ------------------------------------------------------
/// SwarmOps - Numeric and heuristic optimization for C#
/// Copyright (C) 2003-2009 Magnus Erik Hvass Pedersen.
/// Published under the GNU Lesser General Public License.
/// Please see the file license.txt for license details.
/// SwarmOps on the internet: http://www.Hvass-Labs.org/
/// ------------------------------------------------------

using System.Diagnostics;

namespace SwarmOps.Optimizers
{
    /// <summary>
    /// Differential Evolution (DE) optimizer originally
    /// due to Storner and Price (1). This simple and efficient
    /// variant is based on the The Joker variant by Pedersen et
    /// al. (2). It has been found to be very versatile and works
    /// well on a wide range of optimization problems, but may
    /// require tuning (or meta-optimization) of its parameters.
    /// </summary>
    /// <remarks>
    /// References:
    /// (1) R. Storn and K. Price. Differential evolution - a simple
    ///     and efficient heuristic for global optimization over
    ///     continuous spaces. Journal of Global Optimization,
    ///     11:341-359, 1997.
    /// (2) M.E.H. Pedersen and A.J. Chipperfield. Parameter tuning
    ///     versus adaptation: proof of principle study on differential
    ///     evolution. Technical Report HL0802, Hvass Laboratories, 2008
    /// </remarks>
    public class DE : Optimizer
    {
        #region Constructors.
        /// <summary>
        /// Construct the object.
        /// </summary>
        public DE()
            : base()
        {
        }

        /// <summary>
        /// Construct the object.
        /// </summary>
        /// <param name="problem">Problem to optimize.</param>
        public DE(Problem problem)
            : base(problem)
        {
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
            public static readonly double[] HandTuned = { 50.0, 0.9, 0.6 };

            /// <summary>
            /// Good choice of control parameters for use in meta-optimization.
            /// </summary>
            public static readonly double[] ForMetaOptimization = {14.0, 0.8434, 0.7660};//{ 24.0, 0.9495, 0.7612 };

            /// <summary>
            /// Control parameters tuned for all benchmark problems in
            /// 30 dimensions and 6000 fitness evaluations in one optimization run.
            /// </summary>
            public static readonly double[] AllBenchmarks30Dim6000Iter = { 136.0, 0.9813, 0.279 };

            /// <summary>
            /// Control parameters tuned for all benchmark problems in
            /// 30 dimensions and 60000 fitness evaluations in one optimization run.
            /// </summary>
            public static readonly double[] AllBenchmarks30Dim60000Iter = { 186.0, 0.8493, 0.4818 };

            /// <summary>
            /// Control parameters tuned for four benchmark problems (Rastrigin,
            /// Schwefel1-2, Schwefel2-21, Schwefel2-22) in 30 dimensions each
            /// and 6000 fitness evaluations in one optimization run.
            /// </summary>
            public static readonly double[] FourBenchmarks30Dim6000Iter = { 103.0, 0.9794, 0.3976 };

            /// <summary>
            /// Control parameters tuned for four benchmark problems (Ackley,
            /// Rastrigin, Rosenbrock, Schwefel1-2) in 30 dimensions each
            /// and 60000 fitness evaluations in one optimization run.
            /// </summary>
            public static readonly double[] FourBenchmarks30Dim60000Iter = { 191.0, 0.8448, 0.51 };

            /// <summary>
            /// Control parameters tuned for four benchmark problems (Ackley,
            /// Rastrigin, Rosenbrock, Schwefel1-2) in 30 dimensions each
            /// and 600000 fitness evaluations in one optimization run.
            /// </summary>
            public static readonly double[] FourBenchmarks30Dim600000Iter = { 120.0, 0.4852, 0.6413 };

            /// <summary>
            /// Good choice of control parameters for use with benchmark problems,
            /// when using 10000*Dim fitness evaluations per optimization run.
            /// Parameters were tuned for Dim=10 and with optima displaced.
            /// Problems used in tuning: Step, Sphere, Griewank, Ackley, Rosenbrock,
            /// Rastrigin.
            /// </summary>
            public static readonly double[] SixBenchmarks10Dim100000Iter = { 40.0, 0.0103, 0.8991 };

            /// <summary>
            /// Control parameters tuned for Sphere and Rosenbrock problems in 30
            /// dimensions each and 60000 fitness evaluations in one optimization run.
            /// </summary>
            public static readonly double[] SphereRosenbrock_30Dim60000Iter = { 126.0, 0.9211, 0.4027 };

            /// <summary>
            /// Control parameters tuned for Rastrigin and Schwefel1-2 problems in 30
            /// dimensions each and 60000 fitness evaluations in one optimization run.
            /// </summary>
            public static readonly double[] RastriginSchwefel12_30Dim60000Iter = { 185.0, 0.8932, 0.5125 };

            /// <summary>
            /// Control parameters tuned for QuarticNoise, Sphere, Step problems in 30
            /// dimensions each and 60000 fitness evaluations in one optimization run.
            /// </summary>
            public static readonly double[] QuarticNoiseSphereStep_30Dim60000Iter = { 106.0, 0.3345, 0.586 };

            /// <summary>
            /// Control parameters tuned for the Ackley problem in 30 dimensions and
            /// 60000 fitness evaluations in one optimization run.
            /// </summary>
            public static readonly double[] Ackley_30Dim60000Iter = { 19.0, 0.013, 1.2935 };

            /// <summary>
            /// Control parameters tuned for the Rastrigin problem in 30 dimensions and
            /// 60000 fitness evaluations in one optimization run.
            /// </summary>
            public static readonly double[] Rastrigin_30Dim60000Iter = { 42.0, 0.0082, 0.9417 };

            /// <summary>
            /// Control parameters tuned for the (10000) Ackley, (1) Rastrigin, (1) Rosenbrock,
            /// (1) Schwefel1-2 problems in 30 dimensions and 60000 fitness evaluations in one
            /// optimization run.
            /// </summary>
            public static readonly double[] Ackley_4Bnch_30Dim60000Iter = { 20.0, 0.1139, 0.8742 };

            /// <summary>
            /// Control parameters tuned for the (1) Ackley, (10000) Rastrigin, (1) Rosenbrock,
            /// (1) Schwefel1-2 problems in 30 dimensions and 60000 fitness evaluations in one
            /// optimization run.
            /// </summary>
            public static readonly double[] Rastrigin_4Bnch_30Dim60000Iter = { 25.0, 0.0399, 0.9704 };

            /// <summary>
            /// Control parameters tuned for the (1) Ackley, (1) Rastrigin, (10000) Rosenbrock,
            /// (1) Schwefel1-2 problems in 30 dimensions and 60000 fitness evaluations in one
            /// optimization run.
            /// </summary>
            public static readonly double[] Rosenbrock_4Bnch_30Dim60000Iter = { 72.0, 0.7722, 0.4594 };

            /// <summary>
            /// Control parameters tuned for the (1) Ackley, (1) Rastrigin, (1) Rosenbrock,
            /// (10000) Schwefel1-2 problems in 30 dimensions and 60000 fitness evaluations in one
            /// optimization run.
            /// </summary>
            public static readonly double[] Schwefel12_4Bnch_30Dim60000Iter = { 195.0, 0.9618, 0.5027 };
        
            /// <summary>
            /// Control parameters tuned for the (0.01) Ackley, (0.01) Rastrigin, (1) Rosenbrock,
            /// (1) Schwefel1-2 problems in 30 dimensions and 60000 fitness evaluations in one
            /// optimization run.
            /// </summary>
            public static readonly double[] Rosenbrock_Schwefel12_4Bnch_30Dim60000Iter = { 179.0, 0.8473, 0.3325 };

            /// <summary>
            /// Control parameters tuned for the (10000) Ackley, (1000) Rastrigin, (1) Rosenbrock,
            /// (1) Schwefel1-2 problems in 30 dimensions and 60000 fitness evaluations in one
            /// optimization run.
            /// </summary>
            public static readonly double[] Ackley_Rastrigin_4Bnch_30Dim60000Iter = { 21.0, 0.0403, 0.8817 };
        }
        #endregion

        #region Get individual control parameters.
        /// <summary>
        /// Get parameter, Number of agents, aka. population size.
        /// </summary>
        /// <param name="parameters">Optimizer parameters.</param>
        public int GetNumAgents(double[] parameters)
        {
            return (int)System.Math.Round(parameters[0], System.MidpointRounding.AwayFromZero);
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
            get { return "DE-Simple"; }
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

        static readonly double[] _defaultParameters = { 37.0, 0.496, 0.5313 };

        /// <summary>
        /// Default control parameters.
        /// </summary>
        public override double[] DefaultParameters
        {
            get { return _defaultParameters; }
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

            // Get problem-context.
            double[] lowerBound = Problem.LowerBound;
            double[] upperBound = Problem.UpperBound;
            double[] lowerInit = Problem.LowerInit;
            double[] upperInit = Problem.UpperInit;
            int n = Problem.Dimensionality;

            // Allocate agent positions and associated fitnesses.
            double[][] agents = Tools.NewMatrix(numAgents, n);
            double[] agentFitness = new double[numAgents];
            double[] t = new double[n];

            // Iteration variables.
            int i, j, k;

            // Fitness variables.
            double gFitness = Problem.MaxFitness;
            double[] g = null;

            // Initialize all agents.
            // This counts as iterations below.
            for (j = 0; j < numAgents && Problem.RunCondition.Continue(j, gFitness); j++)
            {
                // Refer to the j'th agent as x.
                double[] x = agents[j];

                // Initialize agent-position in search-space.
                Tools.InitializeUniform(ref x, lowerInit, upperInit);

                // Compute fitness of initial position.
                agentFitness[j] = Problem.Fitness(x);

                // Update swarm's best known position.
                if (g == null || agentFitness[j] < gFitness)
                {
                    g = x;
                    gFitness = agentFitness[j];
                }

                // Trace fitness of best found solution.
                Trace(j, gFitness);
            }

            for (i = numAgents; Problem.RunCondition.Continue(i, gFitness); i++)
            {
                Debug.Assert(numAgents > 0);

                // Pick a random agent-index.
                j = Globals.Random.Index(numAgents);

                // Refer to the j'th agent as x.
                double[] x = agents[j];

                // Pick a random dimension.
                int R = Globals.Random.Index(n);

                // Pick random and distinct agent-indices.
                // Not necessarily distinct from x though.
                int R1, R2;
                Globals.Random.Index2(numAgents, out R1, out R2);

                // Refer to the randomly picked agents as a and b.
                double[] a = agents[R1];
                double[] b = agents[R2];

                // Store old position.
                x.CopyTo(t, 0);

                // Compute potentially new position.
                for (k = 0; k < n; k++)
                {
                    if (k == R || Globals.Random.Uniform() < CR)
                    {
                        x[k] = g[k] + F * (a[k] - b[k]);
                    }
                }

                // Enforce bounds before computing new fitness.
                Tools.Bound(ref x, lowerBound, upperBound);

                // Compute new fitness.
                double newFitness = Problem.Fitness(x, agentFitness[j]);

                // Update agent in case of fitness improvement.
                if (newFitness < agentFitness[j])
                {
                    // Update agent's fitness. Position is already updated.
                    agentFitness[j] = newFitness;

                    // Update swarm's best known position.
                    if (newFitness < gFitness)
                    {
                        g = x;
                        gFitness = newFitness;
                    }
                }
                else // Fitness was not an improvement.
                {
                    // Restore old position.
                    t.CopyTo(x, 0);
                }

                // Trace fitness of best found solution.
                Trace(i, gFitness);
            }

            // Return best-found solution and fitness.
            return new Result(g, gFitness, i);
        }
        #endregion
    }
}