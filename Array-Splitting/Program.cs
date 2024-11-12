using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Array_Splitting
{
    public class Program
    {
        // My program has the following requirements:
        // - The hikers must cover some distance each day. 
        // - There are no accomodations at the same place, meaning all distances are positive.
        // - - This requirement helps me to generate unique solutions
        // - All the distances are expressed as integers
        // These requirements 



        static void Main(string[] args)
        {
            // Check if a file path has been given. If not, exit the program.
            if (args.Length == 0)
            {
                Console.WriteLine("Please provide the file path as a command-line argument.");
                return;
            }

            // Generate a random problem and output it.
            List<int> solution;

            var problem = ProblemGenerator.GenerateProblemWithUniqueSolution(10, 20, out solution);

            Console.WriteLine("Problem:");
            for (int i = 0; i < problem.Length; i++)
            {
                Console.WriteLine("" + problem[i]);
            }

            Console.WriteLine("Solution:");
            for (int i = 0; i < solution.Count; i++)
            {
                Console.WriteLine("" + solution[i]);
            }


            /*
            // Parse the file specified at the first command line argument.
            string filePath = args[0];
            List<int> distances;
            int days;

            ParseProblem(filePath, out distances, out days);
            */

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="distances"></param>
        /// <param name="days"></param>
        static List<int> Solve(List<int> distances, List<int> accumulatedDistances, int maxStageDistance, int days)
        {
            // Calculate a lower bound for the day trip distance.
            // - The day trip distance must at least be as high as the greatest stage distance
            // - or at least as high as the average day trip distance.



            int[] endIndices = new int[days];
            int[] dayTripDistances = new int[days];

            // Loop over all day trips except the last one and greedily set the ending indices.
            // This will result in a good rough estimate for the day trips.
            var startIndex = 0;
            var lastIndex = accumulatedDistances.Count - 1;
            var totalHikeDistance = accumulatedDistances[lastIndex];
            var distanceCovered = 0;

            var minimumDayTripDistance = (int)Math.Ceiling(totalHikeDistance / (float)days);
            minimumDayTripDistance = Math.Max(minimumDayTripDistance, maxStageDistance);

            for (int i = 0; i < days - 1; i++)
            {
                // Calculate the largest possible end index for the day trip without excluding a more minimal solution.
                // - The day trip must end soon enough so that each remaining day trip still includes at least one stage.
                var highestEndingIndex = lastIndex - days + 1 + i;
                var endIndex = ModifiedBinarySearch(accumulatedDistances, startIndex, highestEndingIndex, distanceCovered + minimumDayTripDistance);

                // Update the covered distance until now.
                var dayTripDistance = accumulatedDistances[endIndex] - distanceCovered;
                distanceCovered += dayTripDistance;

                // Store the day trip distance for later use.
                dayTripDistances[i] = dayTripDistance;

                // Update the minimum day trip distance.
                // - There are two cases:
                // - 1. The current day trip will be extended. The lower bound will increase to the extended sum.
                // - 2. The day trip will not be extended. The lower bound will be the average of the remaining day trip distances.
                var remainingAverageDayTripDistance = (int)Math.Ceiling((totalHikeDistance - distanceCovered) / (float)(days - i - 1));
                var extendedDayTripDistance = dayTripDistance + distances[endIndex + 1];
                var updatedMinimumDayTripDistance = Math.Min(extendedDayTripDistance, remainingAverageDayTripDistance);
                minimumDayTripDistance = Math.Max(updatedMinimumDayTripDistance, minimumDayTripDistance);

                // Update the starting stage index for the next day trip.
                startIndex = endIndex + 1;
            }

            // The ending index of the last day trip will always be the last index of the stages.
            endIndices[days - 1] = lastIndex;
            dayTripDistances[days - 1] = totalHikeDistance - distanceCovered;


            // Iterate over the longest day trips and move their starting indices to the right.
            var sortedDayTrips = new SortedSet<IndexDistancePair>(new IndexDistancePairComparer());

            for (int i = 0; i < dayTripDistances.Length; i++)
            {
                sortedDayTrips.Add(new IndexDistancePair(i, dayTripDistances[i]));
            }

            while (true)
            {
                var longestDayTrip = sortedDayTrips.Max;

                var previousDayTripIndex = longestDayTrip.Index - 1;

                // Check if the longest day trip is the first one or consists of just one stage.
                if (longestDayTrip.Index == 0 || longestDayTrip.Index - endIndices[previousDayTripIndex] == 1)
                {
                    // In that case the longest day trip distance cannot be reduced.
                    // Construct and return the solution.
                }

                var secondLongestDayTrip = sortedDayTrips.Reverse().ElementAt(1);

                var longestDayTripDistance = longestDayTrip.Distance;

                var checkIndex = endIndices[previousDayTripIndex] - 1;

                while (true)
                {
                    // We cannot 
                    if (checkIndex)
                }


                // Search for the value where the longest day trip becomes just below the second longest day trip.

                // Begin the search where the previous day trip starts, just one stage later to ensure that the previous day trip still has one stage.
                // This can probably be expressed simpler in code with a calculation.
                var low = longestDayTrip.Index == 1 ? 1 : endIndices[longestDayTrip.Index - 2] + 1;
                // End the search where the 
                //var high = endIndices[]
                var searchValue =
                var newBoundary = ModifiedBinarySearch(distances, low, high, endIndices[])



                // You want to reduce the current day trip to below the second longest day trip.
                // Case 1: The second longest day trip is not the previous day trip.

                // Always stop as soon as we have lowered the longest day trip to below the second longest day trip.
                // 
                // [..., a_last] [ b_first, ...]
                // 



                // You must check if you will raise the distance of the previous day trip above the previous maximum.






                // Check if reducing the day trip distance increases the distance of the previous day trip to above the maximum.
                // In that case we must reduce the day trip distance of the previous day trip.


                // Reduce the distance of the longest day trip.


            }





            // This is where the next stage begins.

            // 1. Find the day trip with the longest distance.
            // 2. Move its starting index to the right until another day trip becomes the longest one.
            // What if the previous day trip becomes the new longest?
            // Well, if the maximum day trip distance is still lowered then we must make the move.
            // It doesn't really matter which one becomes the longest as long as we are not increasing the maximum day trip distance.
            // What if the maximum day trip distance does not change?
            // We would essentially just move the day trip with the longest distance to the left.
            // However, we still have to do it if we want to find a lower day trip distance.
            // After all, we must attempt to lower the current longest day trip. There is no other way of finding a better solution
            // We can shuffle the longest trip to the left until it can be lowered.

            // What if it makes the maximum day trip distance longer than the previous one?
            // That can only happen with the previous day trip because we are moving its end index.
            // In that case, the previous day trip needs to be lowered by some amount to decrease the maximum day trip distance.
            // We can quantify that amount and move the border to the right of that day trip. If we can't without 
            // I don't really know what should happen then.
            // There could be a better solution, but it might be "hidden" behind a worse 



            // next_lower_bound = ceil(rest / days_left)
            // Assume previous_lower_bound = ceil((rest + |A|) / (days_left + 1))
            // previous_lower_bound <= next_lower_bound
            // previous_lower_bound < |A| + b_first     Otherwise, b_first would have been a part of |A|
            // Is ceil(rest / days_left) <= |A| + b_first always true?
            // Can I find an example where |A| + b_first < ceil(rest / days_left)



            // There is probably some clever way to prove that ceil((total - |A|) / (days - 1)) <= |A| + B_first.
            // This is my attempt:
            // |A| <= total / days
            // => |A| + B_first <= (total / days) + B_first


            // (total - |A|) / (days - 1) >= |A|
            // <=> (total - |A|) / (days - 1) + B_first >= |A| + B_first


            // ceil(|A|) = |A|
            // ceil(B_first) = B_first


            // 
            // ceil((total - |A|) / (days - 1)) < total - |A|


            // If the lower one of the two is actually the |A| + B_first then we can extend the first day trip!
            // We could repeat these steps until the second one is lower. But is that realistic? Can you find an example for this case?

            // Then we can go on with the second day trip and do the same for that one, but now with the updated lower bound.
            // So we add stages to that one until we would exceed the lower bound.
            // At that point we can do the same thing that we already did with the first day trip.

            // At some point we will reach the last day trip.
            // All we can do with that one is to add all the remaining stages to it.


            // Actually, it might be that we have come to a new lower bound that allows extending previous day trips.
            // We could go back and extend each previous day trip so that each distance is still below or equal to the lower bound.
            // Just increase all of the day trips until they reach the limit of the lower bound.
            // We should start with the first one and go from there.
            // But actually, this might lead to new lower bounds!


            // The next stage of the algorithm will involve selectively moving the starting/ending points of the day trips to minimize the longest day trip.
            // The only way that the day trip distance can be minimized is reducing the length of the longest day trip.
            // We have already established that moving any start/end points to the left of the array does not lead to a better solution.
            // Therefore, the only way that we can shorten the day trip with the longest distance is moving its start point to the right.
            // We can do that, and we must do that to reach a better solution. There is no other way to minimize it.
            // We must move the starting point of the day trip with the longest distance, even if the previous day trip is increased.
            // However, we will at some point reach a point where another day trip has become the longest one.
            // Then obviously that one has to be minimised to reach the best solution.

            // The question is: when do we stop?
            // And what happens if 

            return null;

        }

        /// <summary>
        /// A standard binary search. However, if the searched value is not in the container then the index of the highest value below the searched value is returned.
        /// </summary>
        /// <param name="container"></param>
        /// <param name="low"></param>
        /// <param name="high"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static int ModifiedBinarySearch(List<int> container, int low, int high, int value)
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
        /// Parses the text file that contains the problem.
        /// </summary>
        /// <param name="distances">The distances between accomodations.</param>
        /// <param name="days">The duration of the hike in days.</param>
        /// <returns>If parsing the file succeded.</returns>
        static bool ParseProblem(string filePath, out List<int> distances, out int days, out List<int> accumulatedDistances, out int maxStageDistance)
        {
            distances = new List<int>();
            days = 0;
            accumulatedDistances = new List<int>();
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

                // Read the file line by line.
                foreach (string line in File.ReadLines(filePath))
                {
                    // Trim the line to remove any extra spaces or newline characters.
                    string trimmedLine = line.Trim();

                    // Attempt to parse the line as an integer.
                    int number;

                    if (int.TryParse(trimmedLine, out number) && number > 0)
                    {
                        if (lineIndex == 0)
                        {
                            numberOfStages = number;
                            Console.WriteLine("Number of stages: " + numberOfStages);
                        }
                        else if (lineIndex == 1)
                        {
                            days = number;
                            Console.WriteLine("Number of days: " + days);
                        }
                        else if (lineIndex < numberOfStages + 2)
                        {
                            distances.Add(number);
                            accumulatedDistance += number;
                            accumulatedDistances.Add(number);
                            maxStageDistance = Math.Max(maxStageDistance, number);
                            Console.WriteLine((lineIndex - 1) + ". stage distance: " + number);
                        }
                        else
                        {
                            // If there are extra lines, simply ignore them.
                            Console.WriteLine("Ignoring extra line: " + trimmedLine);
                        }
                    }
                    else
                    {
                        Console.WriteLine("Encountered invalid or non-positive integer: " + trimmedLine);
                        return false;
                    }

                    lineIndex++;
                }

                // Validate if enough stages were provided
                if (distances.Count != numberOfStages)
                {
                    Console.WriteLine("The file does not contain the correct number of stages. Expected: " + numberOfStages + ", found: " + distances.Count);
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
    }
}