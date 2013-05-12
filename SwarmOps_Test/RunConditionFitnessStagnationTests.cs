using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SwarmOps;

namespace SwarmOps_Test
{
    [TestClass]
    public class RunConditionFitnessStagnationTests
    {
        [TestMethod]
        public void ContinueReturnsFalseWhenStagnationCountExceeded()
        {
            var runCondition = new RunConditionFitnessStagnation(10000, 10, 0.0001);
            //Initial Continue
            runCondition.Continue(1, 1000);
            var result = runCondition.Continue(100, 1000);
            Assert.IsFalse(result);
        }
    }
}
