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

        public double Bonus1 { get; set; }
        public double Bonus2 { get; set; }

        string[] _parameterName = { "Stepsize" };

        /// <summary>
        /// Control parameter names.
        /// </summary>
        public override string[] ParameterName
        {
            get { return _parameterName; }
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
            //TODO: Use parameters
            // Retrieve parameter specific to method.
            int numAgents = 5;
            int numOfForeground = 3;
            int numOfLocalSearchTest = 3;
            int numOfLocalSearch = 100;
            int numOfLocalSearchBest = 150;
            Bonus1 = 10.0d;
            Bonus2 = 1.0d;

            // Get problem-context.
            //TODO: Add bounds checks
            double[] lowerBound = Problem.LowerBound;
            double[] upperBound = Problem.UpperBound;
            double[] lowerInit = Problem.LowerInit;
            double[] upperInit = Problem.UpperInit;
            int n = Problem.Dimensionality;

            // Allocate agent position and search-range.
            double[][] X = Tools.NewMatrix(numAgents, n);					// Temp Array for SOA
            Agent[] agents = new Agent[numAgents];
            Agent best = new Agent { Fitness = Problem.MaxFitness };

            Tools.InitializeSOA(ref X, lowerInit, upperInit);
            //Get the fitness of each Agent
            for (int i = 0; i < numAgents; i++)
            {
                agents[i] = new Agent();
                agents[i].X = X[i]; //Reference Copy
                //Initial Search Range
                agents[i].SearchRange = Enumerable.Range(0, n).Select(k => (upperInit[k] - lowerInit[k]) / 2.0).ToArray();
                agents[i].Enable = true;
                agents[i].Improve = true;
                //Initial Fitness
                agents[i].Fitness = Problem.Fitness(agents[i].X);
                if (agents[i].Fitness < best.Fitness)
                {
                    best = agents[i].Clone();
                }
            }

            double LS1_TestGrade, LS2_TestGrade, LS3_TestGrade;
            FunctionEvaluations = 0;
            while (Problem.RunCondition.Continue(FunctionEvaluations, best.Fitness))
            {
                foreach (var agent in agents)
                {
                    if (agent.Enable)
                    {
                        agent.Grade = 0.0d;
                        LS1_TestGrade = LS2_TestGrade = LS3_TestGrade = 0.0d;
                        for (int k = 0; k < numOfLocalSearchTest; k++)
                        {
                            LS1_TestGrade += LocalSearch1(agent, best);
                            LS2_TestGrade += LocalSearch2(agent, best);
                            LS3_TestGrade += LocalSearch3(agent, best);
                        }
                        if (LS1_TestGrade > LS2_TestGrade && LS1_TestGrade > LS3_TestGrade)
                        {
                            for (int j = 0; j < numOfLocalSearch; j++)
                            {
                                agent.Grade += LocalSearch1(agent, best);
                            }
                        }
                        else if (LS2_TestGrade > LS1_TestGrade && LS2_TestGrade > LS3_TestGrade)
                        {
                            for (int j = 0; j < numOfLocalSearch; j++)
                            {
                                agent.Grade += LocalSearch2(agent, best);
                            }
                        }
                        else
                        {
                            for (int j = 0; j < numOfLocalSearch; j++)
                            {
                                agent.Grade += LocalSearch3(agent, best);
                            }
                        }
                    }
                }
                //LocalSearchBest using LocalSearch #1
                for (int j = 0; j < numOfLocalSearchBest; j++)
                {
                    best.Grade += LocalSearch1(best, best);
                }


                foreach (var agent in agents)
                {
                    agent.Enable = false;
                }
                foreach (var agent in agents.OrderByDescending(a => a.Fitness).Take(numOfForeground))
                {
                    agent.Enable = true;
                }
                
            }

            //// Return best-found solution and fitness.
            return new Result(best.X, best.Fitness, FunctionEvaluations);
        }
        #endregion
        private double LocalSearch1(Agent agent, Agent best)
        {
            double grade = 0.0d;
            double fitness;
            double origVal;
            if (!agent.Improve)
            {
                for (int i = 0; i < agent.SearchRange.Length; i++)
                {
                    agent.SearchRange[i] /= 2.0;
                    if (agent.SearchRange[i] < 1e-15)
                    {
                        agent.SearchRange[i] = (Problem.UpperBound[i] - Problem.LowerBound[i]) * 0.4;
                    }
                }
            }
            agent.Improve = false;
            for (int i = 0; i < agent.SearchRange.Length; i++)
            {
                origVal = agent.X[i];
                agent.X[i] -= agent.SearchRange[i];
                fitness = Problem.Fitness(agent.X, agent.Fitness);
                FunctionEvaluations++;

                if (fitness >= agent.Fitness)
                {
                    agent.X[i] = origVal;
                    agent.X[i] += 0.5 * agent.SearchRange[i]; //TODO: Seems strange to Add 1/2 the searchrange after checking subtraction of searchrange
                    fitness = Problem.Fitness(agent.X, agent.Fitness);
                    FunctionEvaluations++;

                    if (fitness >= agent.Fitness)
                    {
                        agent.X[i] = origVal;
                    }
                    else
                    {
                        grade += Bonus2;
                        agent.Fitness = fitness;
                        agent.Improve = true;
                    }
                }
                else //Improvement
                {
                    grade += Bonus2;
                    agent.Fitness = fitness;
                    agent.Improve = true;
                }
                //Check if a new best if found
                if (fitness < best.Fitness)//new best found
                {
                    grade += Bonus1;
                    if (best != agent)
                    {
                        best = agent.Clone();
                    }
                }
            }
            return grade;
        }
        private double LocalSearch2(Agent agent, Agent best)
        {
            double grade = 0.0d;
            if (!agent.Improve)
            {
                for (int i = 0; i < agent.SearchRange.Length; i++)
                {
                    agent.SearchRange[i] /= 2.0;
                    if (agent.SearchRange[i] < 1e-15)
                    {
                        agent.SearchRange[i] = (Problem.UpperBound[i] - Problem.LowerBound[i]) * 0.4;
                    }
                }
            }
            agent.Improve = false;
            int[] r = new int[agent.SearchRange.Length];
            double[] D = new double[agent.SearchRange.Length];
            double[] origVal = agent.X.Clone() as double[];
            for (int l = 0; l < agent.SearchRange.Length; l++)
            {
                for (int i = 0; i < agent.SearchRange.Length; i++)
                {
                    r[i] = Globals.Random.Byte()%4; //Randome 0,1,2,3
                    D[i] = Globals.Random.Uniform(-1.0,1.0); //Random -1,1
                    if(r[i] == 0)
                    {
                        agent.X[i] -= agent.SearchRange[i]*D[i];
                    }
                }
                double fitness = Problem.Fitness(agent.X, agent.Fitness);
                FunctionEvaluations++;

                if (fitness >= agent.Fitness)
                {
                    agent.X = origVal.Clone() as double[];
                    for (int i = 0; i < agent.SearchRange.Length; i++)
                    {
                        if (r[i] == 0)
                        {
                            agent.X[i] += 0.5 * agent.SearchRange[i] * D[i];//TODO: Seems strange to Add 1/2 the searchrange after checking subtraction of searchrange
                        }
                    }
                    fitness = Problem.Fitness(agent.X, agent.Fitness);
                    FunctionEvaluations++;

                    if (fitness >= agent.Fitness)
                    {
                        agent.X = origVal; //Don't have to clone, this is the last reset
                    }
                    else
                    {
                        grade += Bonus2;
                        agent.Fitness = fitness;
                        agent.Improve = true;
                    }
                }
                else //Improvement
                {
                    grade += Bonus2;
                    agent.Fitness = fitness;
                    agent.Improve = true;
                }
                //Check if a new best if found
                if (fitness < best.Fitness) //new best found
                {
                    grade += Bonus1;
                    if (best != agent)
                    {
                        best = agent.Clone();
                    }
                }
            }
            return grade;
        }
        private double LocalSearch3(Agent agent, Agent best)
        {
            //TODO: This whole method seems strange, not sure what is trying to be done
            double grade = 0.0d;
            Agent original = agent.Clone();
            Agent X = agent.Clone();
            Agent Y = agent.Clone();
            Agent Z = agent.Clone();
            double a,b,c;
            double d1, d2, d3;
            //TODO: Oddly no Improve set reset?
            for (int i = 0; i < agent.X.Length; i++)
            {
                X.X[i] += 0.1;
                Y.X[i] -= 0.1;
                Z.X[i] += 0.2;
                X.Fitness = Problem.Fitness(X.X, X.Fitness);
                Y.Fitness = Problem.Fitness(Y.X, Y.Fitness);
                Z.Fitness = Problem.Fitness(Z.X, Z.Fitness);

                if(X.Fitness < best.Fitness)
                {
                    best.Fitness = X.Fitness;
                    best.X = X.X.Clone() as double[];
                    //best.Improve = true;
                    grade += Bonus1;
                }
                RegisterFunctionEval(best);
                if (Y.Fitness < best.Fitness)
                {
                    best.Fitness = Y.Fitness;
                    best.X = Y.X.Clone() as double[];
                    //agent.Improve = true;
                    grade += Bonus1;
                }
                RegisterFunctionEval(best);
                if (Z.Fitness < best.Fitness)
                {
                    best.Fitness = Z.Fitness;
                    best.X = Z.X.Clone() as double[];
                    //agent.Improve = true;
                    grade += Bonus1;
                }
                RegisterFunctionEval(best);
                d1 = agent.Fitness - X.Fitness;
                d2 = agent.Fitness - Y.Fitness;
                d3 = agent.Fitness - Z.Fitness;
                if(d1 > 0.0d) //X improved
                {
                    if(d1 > d2 && d1 > d3)
                    {
                        agent.X = X.X.Clone() as double[];
                        agent.Fitness = X.Fitness;
                    }
                    grade += Bonus2;
                }
                
                if(d2 > 0.0d)
                {
                    if(d2 > d1 && d2 > d3)
                    {
                        agent.X = Y.X.Clone() as double[];
                        agent.Fitness = Y.Fitness;
                    }
                    grade += Bonus2;
                }
               
                if(d3 > 0.0d)
                {
                    if(d3 > d1 && d3 > d2)
                    {
                        agent.X = Z.X.Clone() as double[];
                        agent.Fitness = Z.Fitness;
                    }
                    grade += Bonus2;
                }
                
                a = Globals.Random.Uniform(0.4, 0.5); //TODO:Refactor to parameters
                b = Globals.Random.Uniform(0.1, 0.3);
                c = Globals.Random.Uniform(0.0, 1.0);
                //X[i] = X[i] + a(D1 - D2) + b(D3-2D1) + c
                agent.X[i] += a * (d1 - d2) + b*(d3 - 2*d1) + c; //What is this craziness?
            }
            agent.Fitness = Problem.Fitness(agent.X, agent.Fitness);
            RegisterFunctionEval(best);
            if(agent.Fitness >= original.Fitness)
            {
                //Restore original agent
                agent = original;
            }
            else
            {
                grade += Bonus2;
            }

            return grade;
        }
        private void RegisterFunctionEval(Agent best)
        {
            FunctionEvaluations++;
            // Trace fitness of best found solution.
            Trace(FunctionEvaluations, best.Fitness);
        }
        private class Agent
        {
            public bool Enable { get; set; }
            public bool Improve { get; set; }
            public double Grade { get; set; }
            public double Fitness { get; set; }
            public double[] SearchRange { get; set; }
            public double[] X { get; set; }


            public Agent Clone()
            {
                Agent clone = new Agent();
                clone.Enable = this.Enable;
                clone.Improve = this.Improve;
                clone.Grade = this.Grade;
                clone.Fitness = this.Fitness;
                clone.SearchRange = this.SearchRange.Clone() as double[];
                clone.X = this.X.Clone() as double[];
                return clone;
            }
        }
    }
}