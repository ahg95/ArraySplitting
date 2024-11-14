using System.Collections.Generic;
using System;

namespace Array_Splitting
{
    public static class ProblemGenerator
    {
        /// <summary>
        /// This function generates a problem with a unique solution.
        /// </summary>
        /// <param name="days">The number of days of the hike.</param>
        /// <param name="maxDayTripDistance">The distance of the longest day trip.</param>
        /// <param name="dayTripDistances">The solution of the generated problem in distances per day trip.</param>
        /// <returns>A randomized problem with a unique solution expressed as stage distances.</returns>
        public static int[] GenerateProblemWithUniqueSolution(int days, int maxDayTripDistance, out List<int> dayTripDistances, int seed = 0)
        {
            // One problem can have multiple solutions. Consider the following problem:
            // Stage distances: 1, 1, 1, 1, 1, 9   Days: 3
            // The maximum day trip distance will be 9 because of the last stage.
            // Because of this, the second, third, and fourth stages could be part of the first or second day trip.
            // This problem shows that one problem can have different solutions.
            // To reliably test my algorithm, which only finds one solution, I aim to generate problems with unique solutions.



            // Initialize a random number generator for later use.
            Random rnd = new Random(seed);



            // Initialize a variable for the solution of the problem.
            dayTripDistances = new List<int>();



            // First, check if the number of days is 1, and if it is, generate a trivial solution and problem.
            if (days == 1)
            {
                // Create the solution, which contains one day trip.
                dayTripDistances.Add(maxDayTripDistance);

                // Generate the problem by generating stage distances.
                var stageDistances = new List<int>();

                while (maxDayTripDistance > 0)
                {
                    // Generate a distance for the new stage.
                    var newStageDistance = rnd.Next(1, maxDayTripDistance + 1);

                    // Generate a random index at which to insert the stage.
                    int index = rnd.Next(stageDistances.Count + 1);

                    // Insert the stage.
                    stageDistances.Insert(index, newStageDistance);

                    // Update how much distance remains to add to the day trip.
                    maxDayTripDistance -= newStageDistance;
                }

                // return the problem.
                return stageDistances.ToArray();
            }


            // Generate a complex problem with a unique solution.

            // GENERAL IDEA BEHIND THE ALGORITHM
            // A problem has a unique solution if no day trip can start sooner or later
            // without exceeding the longest day trip's distance.
            // Consider two consecutive day trips A and B with distances |A| and |B| respectively.
            // Also consider that the last stage of A has distance A_last and the first stage of B has distance B_first.
            // Additionally, assume that max is the distance of the longest day trip.
            // To ensure that B cannot start sooner the following must hold:
            // A_last + |B| > max       meaning starting B one stage sooner makes it longer than the longest day trip.
            // To ensure that A cannot end later the following must hold:
            // |A| + B_first > max      meaning ending A one stage later makes it longer than the longest day trip.
            // If taken together, the conditions ensure that the boundary between A and B cannot be moved
            // without making either A or B longer than the longest day trip.
            // If the conditions are applied to every boundary between day trips then no boundary can be moved
            // without making one of the adjacent day trips longer than the longest day trip.


            // - First, generate the day trip distances as a solution.
            // - - At least one day trip needs to have the specified max day trip distance.
            // - - Randomly select one day trip to be that long.
            var longestDayTripIndex = rnd.Next(days);

            // - - Generate the first day trip distance.
            dayTripDistances.Add(longestDayTripIndex == 0 ? maxDayTripDistance : rnd.Next(1, maxDayTripDistance + 1));

            // GENERATING DAY TRIP DISTANCES
            // The previously outlined conditions do not allow generating day trip distances at random.
            // Consider that the following two conditions must hold:
            // A_last + |B| > max
            // A_last <= |A|        Since A_last is a stage in A
            // So the following condition must hold as well:
            // |A| + |B| > max

            // - - Generate the remaining day trip distances.
            for (int i = 1; i < days; i++)
            {
                dayTripDistances.Add(longestDayTripIndex == i ? maxDayTripDistance : rnd.Next(maxDayTripDistance - dayTripDistances[i - 1] + 1, maxDayTripDistance + 1));
            }



            // - Next, generate a problem based on the previously calculated solution.
            // - - The following variable will hold all stage distances per day trip.
            var dayTripStageDistances = new List<int>[days];

            // - - Iterate through all day trips and generate stage distances for them.
            for (int i = 0; i < days; i++)
            {
                // Initialize the list for the stage distances.
                var stageDistances = new List<int>();
                dayTripStageDistances[i] = stageDistances;

                // The remaining distance to add to the day trip.
                var distanceToAdd = dayTripDistances[i];

                if (i == 0)
                {
                    // Generate a distance for stage A_last.
                    // - Recall     A_last + |B| > max
                    // - So         A_last > max - |B| 
                    var newStageDistance = rnd.Next(maxDayTripDistance - dayTripDistances[i + 1] + 1, distanceToAdd + 1);

                    // Add the stage.
                    stageDistances.Add(newStageDistance);

                    // Update the remaining distance to add to this day trip.
                    distanceToAdd -= newStageDistance;

                    // If there is distance remaining to add to the day trip then randomly generate and insert new stages.
                    while (distanceToAdd > 0)
                    {
                        // Generate a distance for the new stage.
                        newStageDistance = rnd.Next(1, distanceToAdd + 1);

                        // Select a random index before the last stage to insert the new stage.
                        int index = rnd.Next(stageDistances.Count);

                        // Add the stage.
                        stageDistances.Insert(index, newStageDistance);

                        // Update the remaining distance to add.
                        distanceToAdd -= newStageDistance;
                    }
                }
                else if (i == days - 1)
                {
                    // Generate a distance for B_first
                    // - Recall     |A| + B_first > max
                    // - So         B_first > max - |A|
                    var newStageDistance = rnd.Next(maxDayTripDistance - dayTripDistances[i - 1] + 1, distanceToAdd + 1);

                    // Add the stage.
                    stageDistances.Add(newStageDistance);

                    // Update the remaining distance to add to this day trip.
                    distanceToAdd -= newStageDistance;

                    // If there is distance remaining to add to the day trip then randomly generate and insert new stages.
                    while (distanceToAdd > 0)
                    {
                        // Generate a distance for the new stage.
                        newStageDistance = rnd.Next(1, distanceToAdd + 1);

                        // Select a random index after the first stage to insert the new stage.
                        int index = rnd.Next(1, stageDistances.Count + 1);

                        // Add the stage.
                        stageDistances.Insert(index, newStageDistance);

                        // Update the remaining distance to add.
                        distanceToAdd -= newStageDistance;
                    }
                }
                else
                {
                    // Considering consecutive day trips A, B, and C, the previous conditions can be formulated as follows:
                    // B_first + |A| > max
                    // <=> B_first > max - |A|

                    // B_last + |C| > max
                    // <=> B_last > max - |C|

                    // First, check if it is possible for the day trip to have more than one stage.
                    // - Calcluate the minimum value for B_first.
                    var minFirstStageDistance = maxDayTripDistance - dayTripDistances[i - 1] + 1;

                    // - Calculate the minimum value for B_last.
                    var minLastStageDistance = maxDayTripDistance - dayTripDistances[i + 1] + 1;

                    // - If the sum of the minimum values exceeds the day trip distance for B then the day trip must consist of a single stage.
                    if (minFirstStageDistance + minLastStageDistance > distanceToAdd)
                    {
                        // B_first and B_last reference the same stage, meaning the day trip has only one stage.
                        stageDistances.Add(distanceToAdd);
                        continue;
                    }



                    // Create B_first.
                    // - Generate a distance for B_first.
                    var newStageDistance = rnd.Next(minFirstStageDistance, distanceToAdd - minLastStageDistance + 1);

                    // - Add B_first.
                    stageDistances.Add(newStageDistance);

                    // - Update the remaining distance to add.
                    distanceToAdd -= newStageDistance;



                    // Create B_last.
                    // - Generate a distance for B_last.
                    newStageDistance = rnd.Next(minLastStageDistance, distanceToAdd + 1);

                    // - Add B_last.
                    stageDistances.Add(newStageDistance);

                    // - Update the remaining distance to add.
                    distanceToAdd -= newStageDistance;



                    // If there is distance remaining to add to the day trip then randomly generate and insert new stages.
                    while (distanceToAdd > 0)
                    {
                        // Generate a distance for the new stage.
                        newStageDistance = rnd.Next(1, distanceToAdd + 1);

                        // Select a random index after the first stage and before the last stage to insert the new stage.
                        int index = rnd.Next(1, stageDistances.Count);

                        // Add the stage.
                        stageDistances.Insert(index, newStageDistance);

                        // Update the remaining distance to add.
                        distanceToAdd -= newStageDistance;
                    }
                }
            }



            // As a last step, concatenate all problem day trips and return the result.
            var result = new List<int>();

            for (int i = 0; i < days; i++)
            {
                result.AddRange(dayTripStageDistances[i]);
            }

            return result.ToArray();
        }
    }
}
