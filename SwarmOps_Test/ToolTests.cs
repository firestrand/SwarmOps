using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SwarmOps;

namespace SwarmOps_Test
{
    [TestClass]
    public class ToolTests
    {
        [TestMethod]
        public void ArrayToMatrixIntTest()
        {
            var array = new[]{1,2,3,4,5,6};
            var expected = new [,]{{1,2,3},{4,5,6}};
            var result = Tools.ArrayToMatrix(array, 2, 3);
            Assert.IsTrue(expected.ValuesEqual(result));

            expected = new [,]{{1,2},{3,4},{5,6}};
            result = Tools.ArrayToMatrix(array, 3, 2);
            Assert.IsTrue(expected.ValuesEqual(result));
        }
        [TestMethod]
        public void ShuffleTest()
        {
            var array = new[] {1, 2, 3, 4, 5, 6};
            var result = array.Clone() as int[];
            Tools.Shuffle(ref result);
            Assert.IsFalse(result.Equals(array));
        }

    }
}
