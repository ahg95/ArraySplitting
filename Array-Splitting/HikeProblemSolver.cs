using System;

namespace Array_Splitting
{
    public class HikeProblemSolver
    {
        // Some parts of the problem were unclear, so I set some reasonable conditions for myself:
        // 1. Each day trip must consist of at least one stage.
        // - - This does not make the problem easier as adding extra day trips to any solution during which no distance is covered is trivial.
        // - - However, this condition enables me to generate unique hiking problems to validate my algorithm.
        // 2. Each stage has a minimal distance of 1 km and can be expressed as an integer value.
        // 3. The total distance of the hike does not exceed the maximum integer value of the processor
        // - - (Nobody wants to hike more than 2 147 483 647km anyways...)

        static void Main(string[] args)
        {
            // Check if a file path has been given. If not, exit the program.
            if (args.Length == 0)
            {
                Console.WriteLine("Please provide the file path as a command-line argument.");
                return;
            }

            // Read the file specified at the first command line argument.
            string filePath = args[0];
            int[] stageDistances;
            int days;

            if (!HikeProblemIO.ReadHikingProblem(filePath, out stageDistances, out days))
                return;
                
            var solution = SolveHikeProblem(stageDistances, days);

            HikeProblemIO.PrintSolution(solution);
        }

        /// <summary>
        /// Solves the specified hiking problem.
        /// </summary>
        /// <param name="distances">The distances for all stages of the hike.</param>
        /// <param name="days">The number of days among which the stages must be divided.</param>
        public static int[] SolveHikeProblem(int[] distances, int days)
        {
            // Firstly, calculate the accumulated stage distances and the maximum stage distance.
            // Each element in the accumulated stage distances array
            // will be the sum of the original stage distance and all previous stage distances.
            var accDistances = new int[distances.Length];
            var maxStageDistance = 0;
            var hikeDistance = 0;
            for (int i = 0; i < distances.Length; i++)
            {
                var stageDistance = distances[i];
                maxStageDistance = Math.Max(maxStageDistance, stageDistance);
                hikeDistance += stageDistance;
                accDistances[i] = hikeDistance;
            }



            // Secondly, extend all day trips based on a lower bound for the longest day trip distance.
            // Extending the day trips is not necessary to solve the problem,
            // but my performance tests show that this improves execution time by a factor of about 300.
            // - Calculate a lower bound for the longest day trip's distance.
            // - - The longest day trip must be at least as long as the average day trip.
            var lastIndex = distances.Length - 1;
            var lowerBound = (int)Math.Ceiling(hikeDistance / (float)days);

            // - - The longest day trip must also be at least as long as the longest stage.
            lowerBound = Math.Max(lowerBound, maxStageDistance);

            // - Use the lower bound for the maximum day trip distance to extend all day trips without excluding a minimal solution.
            var startIndex = 0;
            var distanceCovered = 0;

            int[] endIndices = new int[days];
            int[] dayTripDistances = new int[days];

            for (int i = 0; i < days - 1; i++)
            {
                // Calculate the highest possible end index for the day trip without excluding a more minimal solution.
                // - The day trip must end soon enough to ensure that all remaining day trips consist of at least one stage.
                var maxEndIndex = lastIndex - days + 1 + i;

                // - Instead of iterating over all stage distances, the accumulated stage distances can be used to directly find a fitting day trip distance.
                var endIndex = ModifiedBinarySearch(accDistances, startIndex, maxEndIndex, distanceCovered + lowerBound);

                // Update the covered distance until now.
                var dayTripDistance = accDistances[endIndex] - distanceCovered;
                distanceCovered += dayTripDistance;

                // Store the day trip's end index and distance for later.
                endIndices[i] = endIndex;
                dayTripDistances[i] = dayTripDistance;

                // Update the lower bound for the maximum day trip distance.
                // - Note that all established end indices for the day trips will never have to be lowered.
                // - This is because the established day trips do not exceed the lowest possible maximum day trip distance.
                // - This leaves two cases to consider; the end index of this day trip will move up or it will stay where it is.
                // - 1. Case: the end index stays where it is
                // - - We can calculate two average day trip distances;
                // - - one for all day trips before this end index and one for all day trips after this end index.
                // - - Both could be used to increase the lower bound.
                // - - However, with a high chance, the remaining day trips will have a higher average distance.
                var newLowerBound = (int)Math.Ceiling((hikeDistance - distanceCovered) / (float)(days - i - 1));
                // - 2. Case: the end index increases
                // - - We don't know how far the index will increase.
                // - - However, it will at least increase to the next stage.
                // - - We can thus calculate a lower bound for the average day trip distance.
                // - We must take the lower value of the two bounds to not exclude a minimal solution.
                newLowerBound = Math.Min(newLowerBound, (int)Math.Ceiling((distanceCovered + distances[endIndex + 1]) / (float)(i + 1)));

                // Only accept the updated lower bound if it is higher.
                lowerBound = Math.Max(lowerBound, newLowerBound);

                // Unfortunately, in practice, my performance tests revealed no speed improvement for updating the lower bounds.

                // Update the starting stage index for the next day trip.
                startIndex = endIndex + 1;
            }

            // - The last day trip will end at the last index and have the remaining distance.
            endIndices[days - 1] = lastIndex;
            dayTripDistances[days - 1] = hikeDistance - distanceCovered;

            // I experimented with extending the day trips a second time with the updated lower bounds.
            // However, this showed again no visible speed improvement.



            // Thirdly, shorten the longest day trips until one cannot be shortened.
            // If a day trip with a maximum distance cannot be shortened then a solution has been reached.
            int longestDayTripIndex;
            int maxDayTripDistance;
            do
            {
                // Find the longest day trip.
                // - Instead of using this simple search I've tried using a sorted set instead.
                // - However, using a sorted set almost tripled the execution time.
                // - My guess is that adding and removing the stage distances from the set was too costly.

                longestDayTripIndex = 0;
                maxDayTripDistance = 0;
                for (int i = 0; i < days; i++)
                {
                    if (dayTripDistances[i] >= maxDayTripDistance)
                    {
                        longestDayTripIndex = i;
                        maxDayTripDistance = dayTripDistances[i];
                    }
                }

                // Try to shorten the longest day trip.
            } while (TryShortenDayTripByAmount(longestDayTripIndex, endIndices[longestDayTripIndex], accDistances, 1, maxDayTripDistance, ref endIndices, ref dayTripDistances));

            return dayTripDistances;
        }

        /// <summary>
        /// Tries to shorten the specified day trip.
        /// </summary>
        /// <param name="dayTripIndex">Tries to shorten the day trip with the specified index.</param>
        /// <param name="endIndex">The end index of the day trip.</param>
        /// <param name="accDistances">The accumulated distances.</param>
        /// <param name="minReduction">The minimum desired reduction for the day trip.</param>
        /// <param name="maxDayTripDistance">The maximum day trip distance.</param>
        /// <param name="endIndices">The ending indices of all day trips.</param>
        /// <param name="dayTripDistances">The distances of all day trips.</param>
        /// <returns>If the day trip could be shortened.</returns>
        static bool TryShortenDayTripByAmount(int dayTripIndex, int endIndex, int[] accDistances, int minReduction, int maxDayTripDistance, ref int[] endIndices, ref int[] dayTripDistances)
        {
            // If the day trip is the first one then it cannot be shortened.
            if (dayTripIndex == 0)
                return false;

            // Calculate a new start index.
            // The new start index must be calculated s.t. the day trip's distance is reduced by at least the specified amount..
            var prevDayTripIndex = dayTripIndex - 1;
            var startIndex = endIndices[prevDayTripIndex] + 1;
            var searchValue = accDistances[startIndex - 1] + minReduction;
            var updatedStartIndex = ModifiedBinarySearch(accDistances, startIndex, endIndex, searchValue, true) + 1;

            // Calculate the previous day trip's distance with the updated start index.
            var prevDayTripStartIndex = dayTripIndex == 1 ? 0 : endIndices[prevDayTripIndex - 1] + 1;
            var prevDayTripEndIndex = updatedStartIndex - 1;
            var prevDayTripDistance = CalculateDayTripDistance(prevDayTripStartIndex, prevDayTripEndIndex, accDistances);

            // Calculate by how much the previous day trip exceeds the maximum day trip distance.
            var excessDistance = prevDayTripDistance - maxDayTripDistance;

            // If the previous day trip exceeds the maximum day trip distance
            // and the previous day trip cannot be shortened by the exceeded amount
            // then this day trip cannot be shortened either.
            if (excessDistance > 0 && !TryShortenDayTripByAmount(prevDayTripIndex, prevDayTripEndIndex, accDistances, excessDistance, maxDayTripDistance, ref endIndices, ref dayTripDistances))
                return false;

            // The day trip can be shortened without exceeding the maximum day trip distance.

            // Set the day trip to start later (by setting the previous day trip to end later).
            endIndices[prevDayTripIndex] = updatedStartIndex - 1;

            // Update the day trip distances.
            // - The start and end indices of the current day trip will not have changed through other calls of this function.
            dayTripDistances[dayTripIndex] = CalculateDayTripDistance(updatedStartIndex, endIndex, accDistances);

            // - The start index of the previous day trip might have been changed.
            prevDayTripStartIndex = dayTripIndex == 1 ? 0 : endIndices[prevDayTripIndex - 1] + 1;
            dayTripDistances[prevDayTripIndex] = CalculateDayTripDistance(prevDayTripStartIndex, updatedStartIndex - 1, accDistances);

            return true;
        }

        static int CalculateDayTripDistance(int startIndex, int endIndex, int[] accumulatedStageDistances)
        {
            if (startIndex == 0)
                return accumulatedStageDistances[endIndex];
            else
                return accumulatedStageDistances[endIndex] - accumulatedStageDistances[startIndex - 1];
        }


        /// <summary>
        /// A modified binary search. If the searched value is not in the container then a close alternative is returned based on the provided boolean.
        /// </summary>
        /// <param name="container">The container to search in.</param>
        /// <param name="low">The lowest index to search for the value (inclusive).</param>
        /// <param name="high">The highest index to search for the value (inclusive).</param>
        /// <param name="value">The value to search for in the container.</param>
        /// <param name="findNextHighest">If true, and the search does not find the value, it will return the index of the next highest value. If false, a next lowest value is searched instead.</param>
        /// <returns>The index of the searched value. If the searched value is not in the container then the index of the highest value below the searched value is returned.</returns>
        public static int ModifiedBinarySearch(int[] container, int low, int high, int value, bool findNextHighest = false)
        {
            int result = -1;

            while (low <= high)
            {
                int mid = (int)((low + high) * 0.5f);

                var a = container[mid];

                if (a == value)
                    return mid;

                if (a < value)
                {
                    // Save this value as the best one found yet.
                    if (!findNextHighest)
                        result = mid;

                    low = mid + 1;
                }
                else
                {
                    // Save this value as the best one found yet.
                    if (findNextHighest)
                        result = mid;

                    high = mid - 1;
                }
            }

            return result;
        }
    }
}