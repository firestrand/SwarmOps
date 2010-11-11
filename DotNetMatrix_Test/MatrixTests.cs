using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using DotNetMatrix;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DotNetMatrix_Test
{
    [TestClass]
    public class MatrixTests
    {
        GeneralMatrix A, B, C, Z, O, I, R, S, X, SUB, M, T, SQ, DEF, SOL;
        int errorCount = 0;
        int warningCount = 0;
        double tmp;
        double[] columnwise = new double[] { 1.0, 2.0, 3.0, 4.0, 5.0, 6.0, 7.0, 8.0, 9.0, 10.0, 11.0, 12.0 };
        double[] rowwise = new double[] { 1.0, 4.0, 7.0, 10.0, 2.0, 5.0, 8.0, 11.0, 3.0, 6.0, 9.0, 12.0 };
        double[][] avals = { new double[] { 1.0, 4.0, 7.0, 10.0 }, new double[] { 2.0, 5.0, 8.0, 11.0 }, new double[] { 3.0, 6.0, 9.0, 12.0 } };
        //double[][] rankdef = avals;
        double[][] tvals = { new double[] { 1.0, 2.0, 3.0 }, new double[] { 4.0, 5.0, 6.0 }, new double[] { 7.0, 8.0, 9.0 }, new double[] { 10.0, 11.0, 12.0 } };
        double[][] subavals = { new double[] { 5.0, 8.0, 11.0 }, new double[] { 6.0, 9.0, 12.0 } };
        double[][] rvals = { new double[] { 1.0, 4.0, 7.0 }, new double[] { 2.0, 5.0, 8.0, 11.0 }, new double[] { 3.0, 6.0, 9.0, 12.0 } };
        double[][] pvals = { new double[] { 1.0, 1.0, 1.0 }, new double[] { 1.0, 2.0, 3.0 }, new double[] { 1.0, 3.0, 6.0 } };
        double[][] ivals = { new double[] { 1.0, 0.0, 0.0, 0.0 }, new double[] { 0.0, 1.0, 0.0, 0.0 }, new double[] { 0.0, 0.0, 1.0, 0.0 } };
        double[][] evals = { new double[] { 0.0, 1.0, 0.0, 0.0 }, new double[] { 1.0, 0.0, 2e-7, 0.0 }, new double[] { 0.0, -2e-7, 0.0, 1.0 }, new double[] { 0.0, 0.0, 1.0, 0.0 } };
        double[][] square = { new double[] { 166.0, 188.0, 210.0 }, new double[] { 188.0, 214.0, 240.0 }, new double[] { 210.0, 240.0, 270.0 } };
        double[][] sqSolution = { new double[] { 13.0 }, new double[] { 15.0 } };
        double[][] condmat = { new double[] { 1.0, 3.0 }, new double[] { 7.0, 9.0 } };
        int rows = 3, cols = 4;
        int invalidld = 5; /* should trigger bad shape for construction with val */
        int raggedr = 0; /* (raggedr,raggedc) should be out of bounds in ragged array */
        int raggedc = 4;
        int validld = 3; /* leading dimension of intended test Matrices */
        int nonconformld = 4; /* leading dimension which is valid, but nonconforming */
        int ib = 1, ie = 2, jb = 1, je = 3; /* index ranges for sub GeneralMatrix */
        int[] rowindexset = new int[] { 1, 2 };
        int[] badrowindexset = new int[] { 1, 3 };
        int[] columnindexset = new int[] { 1, 2, 3 };
        int[] badcolumnindexset = new int[] { 1, 2, 4 };
        double columnsummax = 33.0;
        double rowsummax = 30.0;
        double sumofdiagonals = 15;
        double sumofsquares = 650;
        /// <summary>
        /// check that exception is thrown in packed constructor with invalid length 
        /// </summary>
        [TestMethod][ExpectedException(typeof(ArgumentException))]
        public void ArgumentExceptionIsThrownInPackedConstructorWithInvalidLength()
        {
            A = new GeneralMatrix(columnwise, invalidld);
        }
        /// <summary>
        /// check that exception is thrown in default constructor if input array is 'ragged' *
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void ArgumentExceptionIsThrownInConstructorWithRaggedInputArray()
        {
            A = new GeneralMatrix(rvals);
            tmp = A.GetElement(raggedr, raggedc);
        }
        /// <summary>
        /// check that exception is thrown in Create if input array is 'ragged' *
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void ArgumentExceptionIsThrownInCreateWithRaggedInputArray()
        {
            A = GeneralMatrix.Create(rvals);
            tmp = A.GetElement(raggedr, raggedc);
        }

        [TestMethod]
        public void Create()
        {
            A = new GeneralMatrix(columnwise, validld);
			B = new GeneralMatrix(avals);
			tmp = B.GetElement(0, 0);
			avals[0][0] = 0.0;
			C = B.Subtract(A);
			avals[0][0] = tmp;
			B = GeneralMatrix.Create(avals);
			tmp = B.GetElement(0, 0);
			avals[0][0] = 0.0;
            Assert.IsTrue((tmp - B.GetElement(0, 0)) != 0.0);
        }
    }
}
