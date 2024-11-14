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
            // ARRANGE
            // Generate some random problems.
            for (int i = 1; i < 5; i++)
            {
                for(int j = 1; j < 8; j++)
                {
                    int days = 1;
                    for (int k = 0; k < i; k++)
                    {
                        days *= 2;
                    }

                    int maxDayTripDistance = 1;
                    for (int k = 0; k < j; k++)
                    {
                        maxDayTripDistance *= 2;
                    }

                    List<int> solution;
                    var stageDistances = ProblemGenerator.GenerateProblemWithUniqueSolution(days, maxDayTripDistance, out solution);

                    // Print the problem.
                    Console.WriteLine("--- Testing problem ---");
                    Console.WriteLine("Days: " + days + ", maximum day trip distance: " + maxDayTripDistance);
                    Console.WriteLine("Stage distances:");
                    for (int k = 0; k < stageDistances.Length; k++)
                    {
                        Console.WriteLine(stageDistances[k]);
                    }

                    // Print the solution.
                    Console.WriteLine("");
                    Console.WriteLine("Solution day trip distances:");
                    for (int k = 0; k < solution.Count; k++)
                    {
                        Console.WriteLine(solution[k]);
                    }

                    // Calculate the accumulated stage distances and the maximum stage distance.
                    var accumulatedStageDistances = new int[stageDistances.Length];
                    var distanceSum = 0;

                    var maxStageDistance = 1;

                    for (int k = 0; k < accumulatedStageDistances.Length; k++)
                    {
                        var distance = stageDistances[k];

                        distanceSum += distance;
                        accumulatedStageDistances[k] = distanceSum;

                        maxStageDistance = Math.Max(maxStageDistance, distance);
                    }

                    // ACT
                    var result = Program.SolveHikingProblem(stageDistances, accumulatedStageDistances, maxStageDistance, days);

                    Console.WriteLine("");
                    Console.WriteLine("Result day trip distances:");

                    // ASSERT
                    for (int k = 0; k < result.Length; k++)
                    {
                        // Print the result.
                        Console.WriteLine(result[k]);

                        Assert.AreEqual(result[k], solution[k]);
                    }
                }
            }
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
                var result = Program.ModifiedBinarySearchNextLowest(values.ToArray(), 0, problemSize - 1, searchValue);

                // ASSERT
                Assert.AreEqual(result, solutionIndex);
            }
        }
    }
}
