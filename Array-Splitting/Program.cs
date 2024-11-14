using System;
using System.IO;

namespace Array_Splitting
{
    public class Program
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
            int[] accumulatedDistances;
            int maxStageDistance;

            if (!ReadHikingProblem(filePath, out stageDistances, out days, out accumulatedDistances, out maxStageDistance))
                return;
                
            var solution = SolveHikingProblem(stageDistances, accumulatedDistances, maxStageDistance, days);

            PrintSolution(solution);
        }

        /// <summary>
        /// Solves the specified hiking problem.
        /// </summary>
        /// <param name="distances">The distances for all stages of the hike.</param>
        /// <param name="accDistances">The distances of all stages added to the distance of all previous stage distances.</param>
        /// <param name="maxStageDistance">The greatest distance among stages.</param>
        /// <param name="days">The number of days among which the stages must be divided.</param>
        public static int[] SolveHikingProblem(int[] distances, int[] accDistances, int maxStageDistance, int days)
        {
            // Calculate a lower bound for the longest day trip's distance.
            // - The longest day trip must be at least as long as the average day trip.
            var lastIndex = distances.Length - 1;
            var hikeDistance = accDistances[lastIndex];
            var lowerBound = (int)Math.Ceiling(hikeDistance / (float)days);

            // - The longest day trip must also be at least as long as the longest stage.
            lowerBound = Math.Max(lowerBound, maxStageDistance);

            // Use the lower bound for the maximum day trip distance to extend all day trips without excluding a minimal solution.
            var startIndex = 0;
            var distanceCovered = 0;

            int[] endIndices = new int[days];
            int[] dayTripDistances = new int[days];

            for (int i = 0; i < days - 1; i++)
            {
                // Calculate the largest possible end index for the day trip without excluding a more minimal solution.
                // - The day trip must end soon enough to ensure that all remaining day trips consist of at least one stage.
                var maxEndIndex = lastIndex - days + 1 + i;

                // - Instead of iterating over all stage distances, the accumulated stage distances can be used to directly find a fitting day trip distance.
                var endIndex = ModifiedBinarySearchNextLowest(accDistances, startIndex, maxEndIndex, distanceCovered + lowerBound);

                // Update the covered distance until now.
                var dayTripDistance = accDistances[endIndex] - distanceCovered;
                distanceCovered += dayTripDistance;

                // Store the day trip's end index and distance for later.
                endIndices[i] = endIndex;
                dayTripDistances[i] = dayTripDistance;

                // Update the lower bound for the maximum day trip distance.
                // - Note that all established end indices for the day trips will never have to be lowered.
                // - This is because the established day trips do not exceed the lowest possible maximum day trip distance.
                // - This leaves two cases to consider; the end index of this day trip will move up or will stay where it is.
                // - If the end index of this day trip stays where it is,
                // - then the lower bound for the maximum day trip distance can be increased to the average distance of the remaining day trips.
                var averageRemainingDayTripDistance = (int)Math.Ceiling((hikeDistance - distanceCovered) / (float)(days - i - 1));
                var averagePreviousDayTripDistance = (int)Math.Ceiling(distanceCovered / (float)(i + 1));

                var averageRemainingDayTripDistanceExtended = (int)Math.Ceiling((hikeDistance - distanceCovered - distances[endIndex + 1]) / (float)(days - i - 1));
                var averagePreviousDayTripDistanceExtended = (int)Math.Ceiling((distanceCovered + distances[endIndex + 1]) / (float)(i + 1));

                var updatedLowerBound = Math.Min(Math.Max(averageRemainingDayTripDistance, averagePreviousDayTripDistance), Math.Max(averageRemainingDayTripDistanceExtended, averagePreviousDayTripDistanceExtended));

                // - If the end index of this day trip will increase then it will increase by at least one stage.
                // - Therefore, the lower bound for the maximum day trip distance can be increased to the average day trip distance of the previous day trips if the next stage was included.

                // Only update the lower bound if the newly calculated lower bound is higher.
                lowerBound = Math.Max(lowerBound, updatedLowerBound);

                // Update the starting stage index for the next day trip.
                startIndex = endIndex + 1;
            }

            // The last day trip will end at the last index and have the remaining distance.
            endIndices[days - 1] = lastIndex;
            dayTripDistances[days - 1] = hikeDistance - distanceCovered;



            // Create a simpler algorithm:
            // 1. Select the day trip with the longest distance. Call that distance max.
            // 2. If that is the first day trip, then we are done with everything. There is no way to shorten that day trip.
            // 3. If that day trip has only one stage, then we are done. Some day trip will have that stage, and will be maximal.
            // 4. Otherwise: Increase its starting index by 1.

            // 5. If the previous day trip is shorter than max then go back to step 1.
            // 6. If the previous day trip is equal to max then go back to step 1, but select the previous day trip to shorten.

            // 7. If the previous day trip has become longer than max, we need to release the conflict:
            // 8. If the previous day trip is the first day trip, then the conflict cannot be resolved. Revert all changes, and we are done.
            // 9. Otherwise: Increase the starting index of the previous day trip until the conflict is resolved.
            // (Note that the previous day trip must contain multiple stages because it was just extended.)
            // (Also note that the conflict can definitely be resolved:)
            // (The day trip could be reduced to a single stage, which cannot be greater than max.)
            // (If the stage was part of the longest day trip then that stage alone cannot be higher than max.)
            // 11. Go back to step 7.



            // More advanced algorithm:


            


            /*
            int longestDayTripIndex;
            int longestDayTripDistance;
            do
            {
                // Find the longest day trip.
                // 1. Select the day trip with the longest distance. Call that distance max.
                longestDayTripIndex = 0;
                longestDayTripDistance = -1;
                for (int i = 0; i < days; i++)
                {
                    if (dayTripDistances[i] >= longestDayTripDistance)
                    {
                        longestDayTripIndex = i;
                        longestDayTripDistance = dayTripDistances[i];
                    }
                }

                // 2. If that is the first day trip, then we are done with everything. There is no way to shorten that day trip.
                if (longestDayTripIndex == 0)
                    break;

                // 3. If that day trip has only one stage, then we are done. Some day trip will have that stage, and then that day trip will be maximal.
                var previousDayTripIndex = longestDayTripIndex - 1;
                
                if (endIndices[previousDayTripIndex] == endIndices[longestDayTripIndex] - 1)
                    break;

                // 4. Check if the previous day trip would violate the maximum day trip distance if its end index was increased by one.
                var switchStageIndex = endIndices[previousDayTripIndex] + 1;
                var switchStageValue = stageDistances[switchStageIndex];

                if (dayTripDistances[previousDayTripIndex] + switchStageValue > longestDayTripDistance)
                {
                    // If YES:
                    //      If the previous day trip is the first one then the current day trips are the solution.
                    if (longestDayTripIndex == 1)
                        break;

                    var updatedStartIndex = CalculateSatisfyingStartIndex();


                    //      Otherwise: Suppose the current day trip's starting index was increased by 1,
                    //      and calculate a starting index for the previous day trip at which it would not violate the maximum day trip distance anymore.
                    //      (Note that calculating such a start index is always possible.)
                    //      (The supposed previous day trip must contain multiple stages because it was just extended.)
                    //      (Also note that the conflict can definitely be resolved:)
                    //      (The day trip could be reduced to a single stage, which cannot be greater than max.)
                    //      Check if that supposed start index would violate the max day trip distance of the previous day trip. If it does




                }
                else
                {
                    // If NO: Increase the day trip's starting index by 1.
                    // Set the day trip to start one stage later (by setting the previous day trip to end one stage later).
                    endIndices[previousDayTripIndex]++;

                    // Also update the day trip distances accordingy.
                    dayTripDistances[previousDayTripIndex] += switchStageValue;
                    dayTripDistances[longestDayTripIndex] -= switchStageValue;
                }


                // Attempt to shorten the longest day trip.
            } while (TryDelayDayTripStartBySingleStage(longestDayTripIndex, stageDistances, longestDayTripDistance, ref dayTripDistances, ref endIndices));

            return dayTripDistances;
            */


            // Try to shorten (one of) the longest day trips until there is none that can be shortened.
            // If a day trip with a maximum distance cannot be shortened then a solution has been reached.
            int longestDayTripIndex;
            int maxDayTripDistance;
            do
            {
                // Find the longest day trip.
                longestDayTripIndex = 0;
                maxDayTripDistance = -1;
                for (int i = 0; i < days; i++)
                {
                    if (dayTripDistances[i] >= maxDayTripDistance)
                    {
                        longestDayTripIndex = i;
                        maxDayTripDistance = dayTripDistances[i];
                    }
                }

                // Attempt to shorten the longest day trip.
            } while (TryShortenDayTripByAmount(longestDayTripIndex, endIndices[longestDayTripIndex], accDistances, 1, maxDayTripDistance, ref endIndices, ref dayTripDistances));

            return dayTripDistances;
        }

        private static bool TryShortenDayTripByAmount(int dayTripIndex, int endIndex, int[] accumulatedStageDistances, int minReduction, int maxDayTripDistance, ref int[] endIndices, ref int[] dayTripDistances)
        {
            // If the day trip is the first one then it cannot be shortened.
            if (dayTripIndex == 0)
                return false;

            // Calculate a new start index for the day trip for which its distance would be reduced by at least the specified amount.
            var previousDayTripIndex = dayTripIndex - 1;
            var currentStartIndex = endIndices[previousDayTripIndex] + 1;
            var searchValue = accumulatedStageDistances[currentStartIndex - 1] + minReduction;
            var updatedStartIndex = ModifiedBinarySearchNextHighest(accumulatedStageDistances, currentStartIndex, endIndex, searchValue) + 1;

            // Calculate the previous day trip's distance with the updated start index.
            var previousDayTripStartIndex = dayTripIndex == 1 ? 0 : endIndices[previousDayTripIndex - 1] + 1;
            var previousDayTripEndIndex = updatedStartIndex - 1;
            var previousDayTripDistance = CalculateDayTripDistance(previousDayTripStartIndex, previousDayTripEndIndex, accumulatedStageDistances);

            // Calculate by how much the previous day trip exceeds the maximum day trip distance.
            var excessDistance = previousDayTripDistance - maxDayTripDistance;

            // If the previous day trip exceeds the maximum day trip distance
            // and the previous day trip cannot be shortened by the exceeded amount
            // then this day trip cannot be shortened either.
            if (excessDistance > 0 && !TryShortenDayTripByAmount(previousDayTripIndex, previousDayTripEndIndex, accumulatedStageDistances, excessDistance, maxDayTripDistance, ref endIndices, ref dayTripDistances))
                return false;

            // The day trip can be shortened without exceeding the maximum day trip distance.

            // Set the day trip to start later (by setting the previous day trip to end later).
            endIndices[previousDayTripIndex] = updatedStartIndex - 1;

            // Also update the day trip distances accordingy.
            // - The start and end indices of the current day trip will not have changed.
            dayTripDistances[dayTripIndex] = CalculateDayTripDistance(updatedStartIndex, endIndex, accumulatedStageDistances);
            // - The start index of the previous day trip might have been changed.
            previousDayTripStartIndex = dayTripIndex == 1 ? 0 : endIndices[previousDayTripIndex - 1] + 1;
            dayTripDistances[previousDayTripIndex] = CalculateDayTripDistance(previousDayTripStartIndex, updatedStartIndex - 1, accumulatedStageDistances);

            return true;
        }

        public static int CalculateDayTripDistance(int startIndex, int endIndex, int[] accumulatedStageDistances)
        {
            if (startIndex == 0)
                return accumulatedStageDistances[endIndex];
            else
                return accumulatedStageDistances[endIndex] - accumulatedStageDistances[startIndex - 1];
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="startIndex"></param>
        /// <param name="endIndex"></param>
        /// <param name="accumulatedStageDistances"></param>
        /// <param name="minDayTripReduction"></param>
        /// <returns></returns>
        public static int UpdateStartIndex(int startIndex, int endIndex, int[] accumulatedStageDistances, int minDayTripReduction)
        {
            if (minDayTripReduction == 0)
                return startIndex;

            return ModifiedBinarySearchNextHighest(accumulatedStageDistances, startIndex, endIndex, accumulatedStageDistances[startIndex - 1] + minDayTripReduction) + 1;
        }

        /// <summary>
        /// Calculates a start index for the 
        /// </summary>
        /// <param name="startIndex"></param>
        /// <param name="endIndex"></param>
        /// <param name="accumulatedStageDistances"></param>
        /// <param name="maxDayTripDistance"></param>
        /// <returns></returns>
        public static int CalculateSatisfyingStartIndex(int startIndex, int endIndex, int[] accumulatedStageDistances, int maxDayTripDistance)
        {
            if (startIndex > endIndex || startIndex == 0)
                throw new Exception();

            var dayTripDistance = accumulatedStageDistances[endIndex] - accumulatedStageDistances[startIndex - 1];

            var necessaryReduction = dayTripDistance - maxDayTripDistance;

            // Increase the start index until dayTripDistance is equal to or below maxDayTripDistance.
            // TODO: There might be an error here. Debug!
            var searchValue = accumulatedStageDistances[startIndex - 1] + necessaryReduction + 1;

            return ModifiedBinarySearchNextHighest(accumulatedStageDistances, startIndex, endIndex, searchValue);
        }




        /// <summary>
        /// Attempts to delay the start of the specified day trip by a single stage without exceeding the maximum day trip distance.
        /// </summary>
        /// <param name="dayTripIndex">The index of the day trip to shorten.</param>
        /// <param name="stageDistances">The distances of all stages of the problem.</param>
        /// <param name="maxDayTripDistance">The longest day trip's distance.</param>
        /// <param name="dayTripDistances">The distances of the day trips established until now.</param>
        /// <param name="endIndices">The stage indices at which each day trip ends.</param>
        /// <returns>If the day trip could be delayed without invalidating the max day trip distance.</returns>
        public static bool TryDelayDayTripStartBySingleStage(int dayTripIndex, int[] stageDistances, int maxDayTripDistance, ref int[] dayTripDistances, ref int[] endIndices)
        {
            // The start of the day trip cannot be delayed if it is the first day trip.
            if (dayTripIndex == 0)
                return false;

            // Calculate the index of the stage that will be added/removed to/from the previous/specified day trip.
            var previousDayTripIndex = dayTripIndex - 1;
            var switchStageIndex = endIndices[previousDayTripIndex] + 1;

            // Check if the day trip consists of a single stage.
            // A single-stage day trip can only be delayed if it is not the last day trip
            // and if the start of the next day trip can be delayed.
            if (endIndices[dayTripIndex] == switchStageIndex &&
                (dayTripIndex == dayTripDistances.Length - 1
                || !TryDelayDayTripStartBySingleStage(dayTripIndex + 1, stageDistances, maxDayTripDistance, ref dayTripDistances, ref endIndices)))
                    return false;

            var switchStageValue = stageDistances[switchStageIndex];

            // Calculate the maximum allowed distance for the previous day trip.
            // The previous day trip may not be so long that adding the switch stage to it invalidates the maximum day trip distance.
            var maxAllowedDistance = maxDayTripDistance - switchStageValue;

            do
            {
                // Check if the previous day trip's distance is short enough.
                if (dayTripDistances[previousDayTripIndex] <= maxAllowedDistance)
                {
                    // Set the day trip to start one stage later (by setting the previous day trip to end one stage later).
                    endIndices[previousDayTripIndex]++;

                    // Also update the day trip distances accordingy.
                    dayTripDistances[previousDayTripIndex] += switchStageValue;
                    dayTripDistances[dayTripIndex] -= switchStageValue;

                    return true;
                }

                // Delaying the start of this day trip would invalidate the maximum day trip distance.
                // Therefore try to delay the start of the previous day trip to shorten it.
            }
            while (TryDelayDayTripStartBySingleStage(previousDayTripIndex, stageDistances, maxDayTripDistance, ref dayTripDistances, ref endIndices));

            return false;
        }

        /// <summary>
        /// A modified binary search. If the searched value is not in the container then a close alternative is returned based on the provided comparer.
        /// </summary>
        /// <param name="container">The container to search in.</param>
        /// <param name="low">The lowest index to search for the value (inclusive).</param>
        /// <param name="high">The highest index to search for the value (inclusive).</param>
        /// <param name="value">The value to search for in the container.</param>
        /// <returns>The index of the searched value. If the searched value is not in the container then the index of the highest value below the searched value is returned.</returns>
        public static int ModifiedBinarySearchNextLowest(int[] container, int low, int high, int value)
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
                    result = mid;

                    low = mid + 1;
                }
                else
                {
                    high = mid - 1;
                }
            }

            return result;
        }


        /// <summary>
        /// A modified binary search. If the searched value is not in the container then a close alternative is returned based on the provided comparer.
        /// </summary>
        /// <param name="container">The container to search in.</param>
        /// <param name="low">The lowest index to search for the value (inclusive).</param>
        /// <param name="high">The highest index to search for the value (inclusive).</param>
        /// <param name="value">The value to search for in the container.</param>
        /// <returns>The index of the searched value. If the searched value is not in the container then the index of the highest value below the searched value is returned.</returns>
        public static int ModifiedBinarySearchNextHighest(int[] container, int low, int high, int value)
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
                    low = mid + 1;
                }
                else
                {
                    // Save this value as the best one found yet.
                    result = mid;

                    high = mid - 1;
                }
            }

            return result;
        }


        /// <summary>
        /// Reads a text file and translates the text into a hiking problem.
        /// </summary>
        /// <param name="filePath">The path to the file to read the problem from.</param>
        /// <param name="stageDistances">The distances of stages.</param>
        /// <param name="days">The duration of the hike in days.</param>
        /// <param name="accumulatedStageDistances">The accumulated stage distances. These are sum of the current stage distance and all previous stage distances.</param>
        /// <param name="maxStageDistance">The longest stage's distance.</param>
        /// <returns>If the file was parsed successfully.</returns>
        static bool ReadHikingProblem(string filePath, out int[] stageDistances, out int days, out int[] accumulatedStageDistances, out int maxStageDistance)
        {
            // This function calculates two additional variables; the accumulated distances array and the maximum stage distance.
            // These variables are used when solving the problem to calculate a lower bound for the maximum day trip distance.

            stageDistances = new int[0];
            days = 0;
            accumulatedStageDistances = new int[0];
            maxStageDistance = 0;

            // Check if the file exists.
            if (!File.Exists(filePath))
            {
                Console.WriteLine("The specified file does not exist.");
                return false;
            }

            // The file exists; try to read its contents.
            try
            {
                int accumulatedDistance = 0;

                int numberOfStages = 0;

                int lineIndex = 0;

                foreach (string line in File.ReadLines(filePath))
                {
                    // Trim the line to remove any extra spaces or newline characters.
                    string trimmedLine = line.Trim();

                    // Attempt to parse the line as an integer.
                    int number;

                    if (!int.TryParse(trimmedLine, out number) || number <= 0)
                    {
                        Console.WriteLine("Encountered invalid or non-positive integer: " + trimmedLine);
                        return false;
                    }

                    // Process the number depending on the line index.
                    if (lineIndex == 0)
                    {
                        // The first line contains the number of stages.
                        numberOfStages = number;

                        // Initialize the distances and accumulated distances arrays.
                        stageDistances = new int[numberOfStages];
                        accumulatedStageDistances = new int[numberOfStages];

                        Console.WriteLine("Number of stages: " + numberOfStages);
                    }
                    else if (lineIndex == 1)
                    {
                        // The second line contains the number of days.
                        days = number;

                        Console.WriteLine("Number of days: " + days);
                    }
                    else if (lineIndex < numberOfStages + 2)
                    {
                        // All following lines contain stage distances.
                        // Store the stage distance.
                        stageDistances[lineIndex - 2] = number;

                        // Calculate and store the accumulated stage distance.
                        accumulatedDistance += number;
                        accumulatedStageDistances[lineIndex - 2] = accumulatedDistance;

                        // Update the max stage distance if applicable.
                        maxStageDistance = Math.Max(maxStageDistance, number);

                        Console.WriteLine((lineIndex - 1) + ". stage distance: " + number);
                    }
                    else
                    {
                        // Ignore extra lines.
                        Console.WriteLine("Ignoring extra line: " + trimmedLine);
                    }

                    lineIndex++;
                }

                // Validate if enough stages were provided.
                if (stageDistances.Length != numberOfStages)
                {
                    Console.WriteLine("The file does not contain the correct number of stages. Expected: " + numberOfStages + ", found: " + stageDistances.Length);
                    return false;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("An error occurred while reading the file: " + ex.Message);

                return false;
            }

            return true;
        }

        public static void PrintSolution(int[] dayTripDistances)
        {
            var maxDayTripDistance = 0;

            for (int i = 0; i < dayTripDistances.Length; i++)
            {
                var dayTripDistance = dayTripDistances[i];
                maxDayTripDistance = Math.Max(maxDayTripDistance, dayTripDistance);

                Console.WriteLine((i + 1) + ". Tag: " + dayTripDistance + " km");
            }

            Console.WriteLine("\nMaximum: " + maxDayTripDistance + " km");
        }
    }
}