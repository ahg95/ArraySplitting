using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Array_Splitting.Tests
{
    [TestClass]
    public class Tests
    {
        [TestMethod]
        public void TestSolveHikeProblem()
        {
            var stopwatch = new Stopwatch();

            // Warm-up run to ensure that the JIT compiler optimizes the solving method.
            TestSingleHikeProblem(5, 10, stopwatch, 1);

            // Collect the garbage to reduce any impact of the GC on the time measurements.
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();

            int days = 1000;
            int maxDayTripDistance = 1000;

            for (int i = 0; i < 10; i++)
            {
                // Solve the problem.
                var averageExecutionTime = TestSingleHikeProblem(days, maxDayTripDistance, stopwatch, 10, i);

                // Print the status.
                Console.WriteLine("---Testing problem---");
                Console.WriteLine("Days: " + days + ", longest day trip distance: " + maxDayTripDistance);
                Console.WriteLine("Solved in: " + averageExecutionTime + " milliseconds.");
            }
        }

        /// <summary>
        /// Tests a single randomly generated problem with the specified parameters.
        /// </summary>
        /// <param name="days">The number of days for the problem.</param>
        /// <param name="maxDayTripDistance">The maximum day trip distance for the problem.</param>
        /// <param name="stopwatch">A reference to a stopwatch to measure the execution time.</param>
        /// <param name="repeats">How often execution should be repeated to gain a more accurate execution time measurement.</param>
        /// <param name="seed">A seed for the problem.</param>
        /// <returns></returns>
        private double TestSingleHikeProblem(int days, int maxDayTripDistance, Stopwatch stopwatch, int repeats = 5, int seed = 0)
        {
            // Arrange
            List<int> dayTripDistances;
            var stageDistances = HikeProblemGenerator.GenerateProblemWithUniqueSolution(days, maxDayTripDistance, out dayTripDistances, seed);

            // Act
            int[] result = new int[0];
            double executionTime = 0;
            for (int i = 0; i < repeats; i++)
            {
                stopwatch.Restart();
                result = SolveHikeProblem.SolveHikingProblem(stageDistances, days);
                stopwatch.Stop();
                executionTime += stopwatch.Elapsed.TotalMilliseconds;
            }
            executionTime /= repeats;

            // Assert
            for (int k = 0; k < result.Length; k++)
            {
                Assert.AreEqual(result[k], dayTripDistances[k]);
            }

            return executionTime;
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
                var result = SolveHikeProblem.ModifiedBinarySearchNextLowest(values.ToArray(), 0, problemSize - 1, searchValue);

                // ASSERT
                Assert.AreEqual(result, solutionIndex);
            }
        }
    }
}
