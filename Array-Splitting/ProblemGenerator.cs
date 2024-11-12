using System.Collections.Generic;
using System;

namespace Array_Splitting
{
    internal static class ProblemGenerator
    {
        /// <summary>
        /// This function generates a problem with a unique solution.
        /// </summary>
        /// <param name="days">The number of days of the hike.</param>
        /// <param name="maxDayTripDistance">The distance of the longest day trip.</param>
        /// <param name="dayTripDistances">The solution of the generated problem in distances per day trip.</param>
        /// <returns>A randomized problem with a unique solution expressed as stage distances.</returns>
        public static int[] GenerateProblemWithUniqueSolution(int days, int maxDayTripDistance, out List<int> dayTripDistances)
        {
            // One problem can have multiple solutions. Consider the following problem:
            // Stage distances: 1, 1, 1, 1, 1, 9   Days: 3
            // The maximum day trip distance will be 9 because of the last stage.
            // Because of this, the second, third, and fourth stages could be part of the first or second day trip.
            // This problem shows that one problem can have different solutions.
            // To reliably test my algorithm, which only finds one solution, I aim to generate problems with unique solutions.

            // Initialize a random number generator for later use.
            Random rnd = new Random();

            dayTripDistances = new List<int>();

            // First, check if the number of days is 1, and if it is, generate a trivial solution and problem.
            if (days == 1)
            {
                // Create the solution.
                dayTripDistances.Add(maxDayTripDistance);

                // Create the problem.
                var dayTrip = new List<int>();

                while (maxDayTripDistance > 0)
                {
                    // Generate a new distance for the stage to add.
                    var newStageDistance = rnd.Next(1, maxDayTripDistance + 1);

                    // Generate a random index at which to insert the stage.
                    int index = rnd.Next(dayTrip.Count + 1);

                    // Insert the stage.
                    dayTrip.Insert(index, newStageDistance);
                    maxDayTripDistance -= newStageDistance;
                }

                return dayTrip.ToArray();
            }


            // Assume that A and B are two consecutive daytrips with distances |A| and |B| respectively.
            // Also assume that A_last is the distance of the last stage of daytrip A,
            // and B_first is the distance of the first stage of daytrip A.
            // Additionally, assume that max is the day trip with the longest distance.

            // The following must hold to create a unique solution:

            // A_last + |B| > max       meaning: starting day trip B earlier invalidates the maximum day trip distance.
            // <=> A_last > max - |B|

            // B_first + |A| > max      meaning: starting the day trip B later invalidates the maximum day trip distance.
            // <=> B_first > max - |A|

            // => |B| > max - |A|
            // <=> |A| + |B| > max

            // Together, these requirements mean that no day trip can be changed without invalidating the maximum day trip distance.

            // And because of how the variables are defined:
            // 1 <= |A| <= max
            // 1 <= |B| <= max
            // 1 <= A_last <= |A|
            // 1 <= B_first <= |B|



            // First, generate the solution.

            // - Generate a random index for the day trip which is going to have the maximum distance.
            var maxDistanceDayTripIndex = rnd.Next(days);

            // - Generate a random distance for the first day trip or make it the longest one if it matches the index.
            int firstDayTripDistance;
            if (maxDistanceDayTripIndex == 0)
                firstDayTripDistance = maxDayTripDistance;
            else
                firstDayTripDistance = rnd.Next(1, maxDayTripDistance + 1);

            // - Add the first day trip to the solution list.
            dayTripDistances.Add(firstDayTripDistance);

            // - Generate the rest of the day trip distances based on the distance of the previous one.
            for (int i = 1; i < days; i++)
            {
                var previousDayTripDistance = dayTripDistances[i - 1];

                // Generate a random distance or make this day trip the longest one if it matches the index.
                int newDayTripDistance;
                if (i == maxDistanceDayTripIndex)
                    newDayTripDistance = maxDayTripDistance;
                else
                    newDayTripDistance = rnd.Next(maxDayTripDistance - previousDayTripDistance + 1, maxDayTripDistance + 1);

                dayTripDistances.Add(newDayTripDistance);
            }



            // Next, generate a problem based on the previously calculated solution.

            var dayTripStages = new List<int>[days];

            // - Iterate through all day trips and generate stage distances for them.
            for (int i = 0; i < days; i++)
            {
                dayTripStages[i] = new List<int>();

                var stages = dayTripStages[i];

                var distanceToAdd = dayTripDistances[i];

                if (i == 0)
                {
                    // We are currently handling the first day trip.
                    // First, generate a distance for the last stage of the day trip.
                    // Recall:
                    // A_last + |B| > max
                    // <=> A_last > max - |B|
                    // 1 <= A_last <= |A|
                    var nextDayTripDistance = dayTripDistances[i + 1];
                    var newStageDistance = rnd.Next(maxDayTripDistance - nextDayTripDistance + 1, distanceToAdd + 1);

                    // Add the stage.
                    stages.Add(newStageDistance);
                    distanceToAdd -= newStageDistance;

                    // Randomly insert stages BEFORE the last stage until the specified day trip distance is reached.
                    while (distanceToAdd > 0)
                    {
                        // Generate a new distance for the stage to add.
                        newStageDistance = rnd.Next(1, distanceToAdd + 1);

                        // Generate a random index at which to add the stage.
                        int index = rnd.Next(stages.Count);

                        // Add the stage.
                        stages.Insert(index, newStageDistance);
                        distanceToAdd -= newStageDistance;
                    }
                }
                else if (i == days - 1)
                {
                    // We are currently handling the last day trip.
                    // First, generate a distance for the first stage of the day trip.
                    // Recall:
                    // B_first + |A| > max
                    // <=> B_first > max - |A|
                    // 1 <= B_first <= |B|
                    var previousDayTripDistance = dayTripDistances[i - 1];
                    var newStageDistance = rnd.Next(maxDayTripDistance - previousDayTripDistance + 1, distanceToAdd + 1);

                    // Add the stage.
                    stages.Add(newStageDistance);
                    distanceToAdd -= newStageDistance;

                    // Randomly insert stages AFTER the first stage until the specified day trip distance is reached.
                    while (distanceToAdd > 0)
                    {
                        // Generate a new distance for the stage to add.
                        newStageDistance = rnd.Next(1, distanceToAdd + 1);

                        // Generate a random index at which to add the stage.
                        int index = rnd.Next(1, stages.Count + 1);

                        // Add the stage.
                        stages.Insert(index, newStageDistance);
                        distanceToAdd -= newStageDistance;
                    }
                }
                else
                {
                    // We are currently handling a day trip that is inbetween other day trips.
                    // We can extend our previous requirements to three consecutive day trips A, B, and C:
                    // B_first + |A| > max
                    // <=> B_first > max - |A|

                    // B_last + |C| > max
                    // <=> B_last > max - |C|

                    // First, check if it is possible for the day trip to have more than one stage.
                    // - If B_first + B_last > |B| then B_first and B_last must be the same stage.
                    var previousDayTripDistance = dayTripDistances[i - 1];
                    var minFirstStageDistance = maxDayTripDistance - previousDayTripDistance + 1;

                    var nextDayTripDistance = dayTripDistances[i + 1];
                    var minLastStageDistance = maxDayTripDistance - nextDayTripDistance + 1;

                    if (minFirstStageDistance + minLastStageDistance > distanceToAdd)
                    {
                        // Adding two stages to the day trip is impossible without overshooting its overall distance.
                        // Therefore, simply add a single stage.
                        stages.Add(distanceToAdd);
                        continue;
                    }



                    // Randomly generate a distance for the first stage.
                    var maxFirstStageDistance = distanceToAdd - minLastStageDistance;
                    var newStageDistance = rnd.Next(minFirstStageDistance, maxFirstStageDistance + 1);

                    // Add the stage to the start of the day trip.
                    stages.Add(newStageDistance);
                    distanceToAdd -= newStageDistance;

                    // Randomly generate a distance for the last stage.
                    newStageDistance = rnd.Next(minLastStageDistance, distanceToAdd + 1);

                    // Add the stage to the end of the day trip.
                    stages.Add(newStageDistance);
                    distanceToAdd -= newStageDistance;

                    // Randomly insert stages AFTER the first stage and BEFORE the last stage until the specified distance is reached.
                    while (distanceToAdd > 0)
                    {
                        // Generate a new distance for the stage to add.
                        newStageDistance = rnd.Next(1, distanceToAdd + 1);

                        // Generate a random index at which to insert the stage.
                        int index = rnd.Next(1, stages.Count + 1);

                        // Add the new stage.
                        stages.Insert(index, newStageDistance);
                        distanceToAdd -= newStageDistance;
                    }
                }
            }



            // As a last step, concatenate all problem day trips and return the result.
            var result = new List<int>();

            for (int i = 0; i < days; i++)
            {
                result.AddRange(dayTripStages[i]);
            }

            return result.ToArray();
        }
    }
}
