using System;
using System.IO;

namespace Array_Splitting
{
    public class SolveHikeProblem
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

            if (!ReadHikingProblem(filePath, out stageDistances, out days))
                return;
                
            var solution = SolveHikingProblem(stageDistances, days);

            PrintSolution(solution);
        }

        /// <summary>
        /// Solves the specified hiking problem.
        /// </summary>
        /// <param name="distances">The distances for all stages of the hike.</param>
        /// <param name="accDistances">The distances of all stages added to the distance of all previous stage distances.</param>
        /// <param name="maxStageDistance">The greatest distance among stages.</param>
        /// <param name="days">The number of days among which the stages must be divided.</param>
        public static int[] SolveHikingProblem(int[] distances, int days)
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
                // - This leaves two cases to consider; the end index of this day trip will move up or it will stay where it is.
                // - 1. Case: the end index stays where it is
                // - - With a high chance, the average distance of the remaining day trips will be higher than that of the previous day trips.
                // - - 
                // - 2. Case: the end index increases
                // - - 

                var averageRemainingDayTripDistance = (int)Math.Ceiling((hikeDistance - distanceCovered) / (float)(days - i - 1));
                var averagePreviousDayTripDistance = (int)Math.Ceiling(distanceCovered / (float)(i + 1));

                var averageRemainingDayTripDistanceExtended = (int)Math.Ceiling((hikeDistance - distanceCovered - distances[endIndex + 1]) / (float)(days - i - 1));
                var averagePreviousDayTripDistanceExtended = (int)Math.Ceiling((distanceCovered + distances[endIndex + 1]) / (float)(i + 1));

                var newLowerBound = Math.Min(Math.Max(averageRemainingDayTripDistance, averagePreviousDayTripDistance), Math.Max(averageRemainingDayTripDistanceExtended, averagePreviousDayTripDistanceExtended));

                // Only accept the updated lower bound if it is higher.
                lowerBound = Math.Max(lowerBound, newLowerBound);

                // Update the starting stage index for the next day trip.
                startIndex = endIndex + 1;
            }

            // - The last day trip will end at the last index and have the remaining distance.
            endIndices[days - 1] = lastIndex;
            dayTripDistances[days - 1] = hikeDistance - distanceCovered;



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
            var updatedStartIndex = ModifiedBinarySearchNextHighest(accDistances, startIndex, endIndex, searchValue) + 1;

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

        public static int CalculateDayTripDistance(int startIndex, int endIndex, int[] accumulatedStageDistances)
        {
            if (startIndex == 0)
                return accumulatedStageDistances[endIndex];
            else
                return accumulatedStageDistances[endIndex] - accumulatedStageDistances[startIndex - 1];
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
        static bool ReadHikingProblem(string filePath, out int[] stageDistances, out int days)
        {
            stageDistances = new int[0];
            days = 0;

            // Check if the file exists.
            if (!File.Exists(filePath))
            {
                Console.WriteLine("The specified file does not exist.");
                return false;
            }

            // The file exists; try to read its contents.
            try
            {
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

                        // Initialize the distances array.
                        stageDistances = new int[numberOfStages];

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