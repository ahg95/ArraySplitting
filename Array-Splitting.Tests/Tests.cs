using HelloWorldNet45;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;

namespace Array_Splitting.Tests
{
    [TestClass]
    public class Tests
    {
        [TestMethod]
        public void TestArraySplitting()
        {



        }


        [TestMethod]
        public void TestModifiedBinarySearch()
        {
            var rnd = new Random();

            const int NROFPROBLEMS = 100;
            const int MINIMUMPROBLEMSIZE = 10;
            const int MAXIMUMPROBLEMSIZE = 30;
            const int SMALLESTSTEP = 1;
            const int LARGESTSTEP = 10;

            // Test the modified binary search on some randomly generated problems.
            for (int i = 0; i < NROFPROBLEMS; i++)
            {
                // ARRANGE
                // First, generate a random array to search in.
                var problemSize = rnd.Next(MINIMUMPROBLEMSIZE, MAXIMUMPROBLEMSIZE + 1);
                var values = new List<int>(problemSize);

                // - Populate the list with random values.
                var sum = 0;

                for (int j = 0; j < problemSize; j++)
                {
                    // Add a new element to the array.
                    var step = rnd.Next(SMALLESTSTEP, LARGESTSTEP + 1);
                    sum += step;
                    values.Add(sum);
                }



                // Then construct a solution.
                // - Select a random index to be the solution.
                var solutionIndex = rnd.Next(problemSize);



                // Afterwards, use the solution index to construct a value to search for.
                int searchValue;
                if (solutionIndex == problemSize - 1)
                {
                    // If the solution is the last index then the value to search for could be equal to or higher than the last value of the coontainer.
                    var lastValue = values[values.Count - 1];
                    searchValue = rnd.Next(lastValue, lastValue + LARGESTSTEP);
                } else
                {
                    // Otherwise, the value to search for must be between the value at the solution index and the next value in the container minus 1.
                    var solutionValue = values[solutionIndex];
                    var nextValue = values[solutionIndex + 1];
                    searchValue = rnd.Next(solutionValue, nextValue);
                }



                // ACT
                // Finally, test the search algorithm on the problem.
                var result = Program.ModifiedBinarySearch(values.ToArray(), 0, problemSize - 1, searchValue);

                // ASSERT
                Assert.AreEqual(result, solutionIndex);
            }
        }
    }
}
