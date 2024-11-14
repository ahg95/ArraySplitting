using System;
using System.IO;

namespace Array_Splitting
{
    public static class HikeProblemIO
    {
        /// <summary>
        /// Reads a text file and translates the text into a hiking problem.
        /// </summary>
        /// <param name="filePath">The path to the file to read the problem from.</param>
        /// <param name="stageDistances">The distances of stages.</param>
        /// <param name="days">The duration of the hike in days.</param>
        /// <param name="accumulatedStageDistances">The accumulated stage distances. These are sum of the current stage distance and all previous stage distances.</param>
        /// <param name="maxStageDistance">The longest stage's distance.</param>
        /// <returns>If the file was parsed successfully.</returns>
        public static bool ReadHikingProblem(string filePath, out int[] stageDistances, out int days)
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

                        //Console.WriteLine("Number of stages: " + numberOfStages);
                    }
                    else if (lineIndex == 1)
                    {
                        // The second line contains the number of days.
                        days = number;

                        //Console.WriteLine("Number of days: " + days);
                    }
                    else if (lineIndex < numberOfStages + 2)
                    {
                        // All following lines contain stage distances.
                        // Store the stage distance.
                        stageDistances[lineIndex - 2] = number;

                        //Console.WriteLine((lineIndex - 1) + ". stage distance: " + number);
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

        /// <summary>
        /// Prints a specified solution.
        /// </summary>
        /// <param name="dayTripDistances"></param>
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
