/// ------------------------------------------------------
/// Published under the GNU Lesser General Public License.
/// Please see the file license.txt for license details.
/// SwarmOps on the internet: http://www.Hvass-Labs.org/
/// ------------------------------------------------------

using System.Diagnostics;

namespace SwarmOps.Optimizers
{
    /// <summary>
    /// Multiple Trajectory Search.
    /// </summary>
    public class MTS : Optimizer
    {
        #region Constructors.
        /// <summary>
        /// Construct the object.
        /// </summary>
        /// <remarks>
        /// References:
        /// (1) L. Tseng, C. Chen. Multiple trajectory search for Large Scale Global Optimization
        /// </remarks>
        public MTS()
            : base()
        {
        }

        /// <summary>
        /// Construct the object.
        /// </summary>
        /// <param name="problem">Problem to optimize.</param>
        public MTS(Problem problem)
            : base(problem)
        {
        }
        #endregion

        #region Get control parameters.
        /// <summary>
        /// Get parameter, Stepsize.
        /// </summary>
        /// <param name="parameters">Optimizer parameters.</param>
        public double GetStepsize(double[] parameters)
        {
            return parameters[0];
        }
        #endregion

        #region Base-class overrides, Problem.
        /// <summary>
        /// Name of the optimizer.
        /// </summary>
        public override string Name
        {
            get { return "MTS"; }
        }

        /// <summary>
        /// Number of control parameters for optimizer.
        /// </summary>
        public override int Dimensionality
        {
            get { return 1; }
        }

        string[] _parameterName = { "Stepsize" };

        /// <summary>
        /// Control parameter names.
        /// </summary>
        public override string[] ParameterName
        {
            get { return _parameterName ; }
        }

        static readonly double[] _defaultParameters = { 0.05 };

        /// <summary>
        /// Default control parameters.
        /// </summary>
        public override double[] DefaultParameters
        {
            get { return _defaultParameters; }
        }

        static readonly double[] _lowerBound = { 0 };

        /// <summary>
        /// Lower search-space boundary for control parameters.
        /// </summary>
        public override double[] LowerBound
        {
            get { return _lowerBound; }
        }

        static readonly double[] _upperBound = { 2.0 };

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

            // Retrieve parameter specific to GD method.
            double stepsize = GetStepsize(parameters);
            int numAgents = 5;

            // Get problem-context.
            double[] lowerBound = Problem.LowerBound;
            double[] upperBound = Problem.UpperBound;
            double[] lowerInit = Problem.LowerInit;
            double[] upperInit = Problem.UpperInit;
            int n = Problem.Dimensionality;

            // Allocate agent position and search-range.
            double[][] x = Tools.NewMatrix(numAgents, n);					// Current position.
            double[][] v = Tools.NewMatrix(numAgents, n);					// Gradient/velocity.
            double[] g = new double[n];					// Best-found position.
            double gBest = Problem.MaxFitness;

            //Algorithm variables
            bool[] enable = new bool[numAgents];
            bool[] improve = new bool[numAgents];
            double[][] searchRange = Tools.NewMatrix(numAgents, n);
            Tools.NewMatrix(numAgents, n);

            //for (int j = 0; j < numAgents; j++)
            //{
            //    // Initialize agent-position in search-space.
            //    Tools.InitializeUniform(ref x[j], lowerInit, upperInit);
            //    Tools.InitializeUniform(ref v[j], lowerInit, upperInit);
            //    Tools.Initialize(ref searchRange[j],0.0d);

            //    enable[j] = true;
            //    improve[j] = true;
            //    for(int k = 0; k < n; k++)
            //    {
            //        searchRange[j][k] = (upperInit[k] - lowerInit[k])/2.0;
            //    }
            //    // Compute fitness of initial position.
            //    // This counts as an iteration below.
            //    double fitness = Problem.Fitness(x);
            //}


            

            //// This is the best-found position.
            //x.CopyTo(g, 0);

            //// Trace fitness of best found solution.
            //Trace(0, gBest);

            int i = 0;
            //for (i = 1; Problem.RunCondition.Continue(i, gBest); i++)
            //{
            //    // Compute gradient.
            //    i += Problem.Gradient(x, ref v);

            //    // Compute norm of gradient-vector.
            //    double gradientNorm = Tools.Norm(v);

            //    // Compute current stepsize.
            //    double normalizedStepsize = stepsize / gradientNorm;

            //    // Move in direction of steepest descent.
            //    for (int j = 0; j < n; j++)
            //    {
            //        x[j] -= normalizedStepsize * v[j];
            //    }

            //    // Enforce bounds before computing new fitness.
            //    Tools.Bound(ref x, lowerBound, upperBound);

            //    // Compute new fitness.
            //    double newFitness = Problem.Fitness(x, fitness);

            //    // Update best position and fitness found in this run.
            //    if (newFitness < fitness)
            //    {
            //        // Update this run's best known position.
            //        x.CopyTo(g, 0);

            //        // Update this run's best know fitness.
            //        fitness = newFitness;
            //    }

            //    // Trace fitness of best found solution.
            //    Trace(i, gBest);
            //}

            //// Return best-found solution and fitness.
            return new Result(g, gBest, i);
        }
        #endregion
    }
}