/// ------------------------------------------------------
/// Published under the GNU Lesser General Public License.
/// Please see the file license.txt for license details.
/// SwarmOps on the internet: http://www.Hvass-Labs.org/
/// ------------------------------------------------------

using System;
using System.Diagnostics;
using System.Linq;
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

        public int FunctionEvaluations { get; set; }
        #endregion

        #region Base-class overrides, Optimizer.
        /// <summary>
        /// Perform one optimization run and return the best found solution.
        /// </summary>
        /// <param name="parameters">Control parameters for the optimizer.</param>
        public override Result Optimize(double[] parameters)
        {
            Debug.Assert(parameters != null && parameters.Length == Dimensionality);

            // Retrieve parameter specific to method.
            int numAgents = 5;
            int numOfForeground = 3;
            int numOfLocalSearchTest = 3;
            int numOfLocalSearch = 100;
            int numOfLocalSearchBest = 150;
            double bonus1 = 10.0d;
            double bonus2 = 1.0d;
            var randomRangeA = new Tuple<double,double>(0.4,0.5);
            var randomeRangeB = new Tuple<double, double>(0.1, 0.3);
            var randomeRangeC = new Tuple<double, double>(0.0, 1.0);

            // Get problem-context.
            double[] lowerBound = Problem.LowerBound;
            double[] upperBound = Problem.UpperBound;
            double[] lowerInit = Problem.LowerInit;
            double[] upperInit = Problem.UpperInit;
            int n = Problem.Dimensionality;

            // Allocate agent position and search-range.
            double[][] X = Tools.NewMatrix(numAgents, n);					// Temp Array for SOA
            Agent[] agents = new Agent[numAgents];
            Agent best = new Agent();
            best.Fitness = Problem.MaxFitness;

            Tools.InitializeSOA(ref X,lowerInit,upperInit);
            //Get the fitness of each Agent
            for (int i = 0; i < numAgents; i++)
            {
                agents[i].X = X[i]; //Reference Copy
                agents[i].Fitness = Problem.Fitness(agents[i].X, best.Fitness);
                if(agents[i].Fitness > best.Fitness)
                {
                    best = agents[i].Clone() as Agent;
                }
                //Initial Search Range
                agents[i].SearchRange = Enumerable.Range(0, n).Select(k => (upperInit[k] - lowerInit[k]) / 2.0).ToArray();
                agents[i].Enable = true;
                agents[i].Improve = true;
            }

            double LS1_TestGrade, LS2_TestGrade, LS3_TestGrade;
            FunctionEvaluations = 0;
            while(Problem.RunCondition.Continue(FunctionEvaluations,best.Fitness))
            {
                foreach(var agent in agents)
                {
                    if (agent.Enable)
                    {
                        agent.Grade = 0.0d;
                        LS1_TestGrade = LS2_TestGrade = LS3_TestGrade = 0.0d;
                        for (int k = 0; k < numOfLocalSearchTest; k++)
                        {
                            LS1_TestGrade += LocalSearch1(agent.X, agent.SearchRange);
                            LS2_TestGrade += LocalSearch2(agent.X, agent.SearchRange);
                            LS3_TestGrade += LocalSearch3(agent.X, agent.SearchRange);
                        }
                        if(LS1_TestGrade > LS2_TestGrade && LS1_TestGrade > LS3_TestGrade)
                        {
                            for (int j = 0; j < numOfLocalSearch; j++)
                            {
                                agent.Grade += LocalSearch1(agent.X, agent.SearchRange);
                            }
                        }
                        else if(LS2_TestGrade > LS1_TestGrade && LS2_TestGrade > LS3_TestGrade)
                        {
                            for (int j = 0; j < numOfLocalSearch; j++)
                            {
                                agent.Grade += LocalSearch2(agent.X, agent.SearchRange);
                            }
                        }
                        else
                        {
                            for (int j = 0; j < numOfLocalSearch; j++)
                            {
                                agent.Grade += LocalSearch3(agent.X, agent.SearchRange);
                            }
                        }
                    }
                }
                //LocalSearchBest using LocalSearch #1
                for (int j = 0; j < numOfLocalSearchBest; j++)
                {
                    best.Grade += LocalSearch1(best.X, best.SearchRange);
                }

                
                foreach(var agent in agents)
                {
                    agent.Enable = false;
                }
                foreach (var agent in agents.OrderByDescending(a => a.Fitness).Take(numOfForeground))
                {
                    agent.Enable = true;
                }
                // Trace fitness of best found solution.
                Trace(FunctionEvaluations, best.Fitness);
            }

            //// Return best-found solution and fitness.
            return new Result(best.X, best.Fitness, FunctionEvaluations);
        }
        #endregion
        private double LocalSearch1(double[] Xi, double[] SearchRangeXi)
        {
            throw new NotImplementedException();
        }
        private double LocalSearch2(double[] Xi, double[] SearchRangeXi)
        {
            throw new NotImplementedException();
        }
        private double LocalSearch3(double[] Xi, double[] SearchRangeXi)
        {
            throw new NotImplementedException();
        }
        private class Agent : ICloneable
        {
            public bool Enable { get; set; }
            public bool Improve { get; set; }
            public double Grade { get; set; }
            public double Fitness { get; set; }
            public double[] SearchRange { get; set; }
            public double[] X { get; set; }


            public object Clone()
            {
                Agent clone = new Agent();
                clone.Enable = this.Enable;
                clone.Improve = this.Improve;
                clone.Grade = this.Grade;
                clone.Fitness = this.Fitness;
                clone.SearchRange = this.SearchRange.Clone() as double[];
                clone.X = this.X.Clone() as double[];
            }
        }
    }
}