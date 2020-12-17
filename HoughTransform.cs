using System;
using System.Collections.Generic;
using System.Linq;

namespace INFOIBV
{
    /// <summary>
    /// Struct used to keep track of polar coordinate lines found using the Hough transform
    /// </summary>
    public struct FoundLines
    {
        public double Rho;
        public double Theta;
        public int V;

        public FoundLines(double Theta, double Rho, int V)
        {
            this.Rho = Rho;
            this.Theta = Theta;
            this.V = V;
        }
    }

    public static class HoughTransform
    {
        private static int threshold = 50;
        private static int[,] neighbourhood = new int[21, 21];

        public static int numPoints;

        /// <summary>
        /// Find lines by generating a Hough transform, doing Non-maximum suppression and thresholding.
        /// </summary>
        /// <param name="image"></param>
        /// <param name="min"></param>
        /// <param name="max"></param>
        /// <returns></returns>
        public static List<FoundLines> FoundLines(byte[,] image, int min, int max)
        {
            Console.WriteLine("[Feature Selection] Initializing Hough Transform for lines");
            var houghSpace = CreateHoughSpace(image, 180);
            NonMaximumSupression(houghSpace);
            var list = GetLines(houghSpace, threshold);
            var newList = list.GroupBy(x => x.Rho).Select(y => y.First()).ToList();
            return newList;
        }

        /// <summary>
        /// Create parameter space from original input image and the amount of angles checked for lines.
        /// </summary>
        /// <param name="input"></param>
        /// <param name="maxTheta"></param>
        /// <returns></returns>
        public static int[,] CreateHoughSpace(byte[,] input, int maxTheta)
        {
            var houghHeight = (int)(Math.Sqrt(2) * Math.Max(input.GetLength(0), input.GetLength(1)) / 2);
            var houghArray = new int[maxTheta, 2 * input.GetLength(1)];
            float centerX = input.GetLength(0) / 2;
            float centerY = input.GetLength(1) / 2;
            numPoints = 0;
            var thetaStep = (0.5 * Math.PI) / maxTheta;

            for (var u = 0; u < input.GetLength(0); u++)
                for (var v = 0; v < input.GetLength(1); v++)
                    if (input[u, v] == 255)
                        for (var t = 0; t < maxTheta; t++)
                        {
                            var realTheta = t * thetaStep + 0.85 * Math.PI;
                            var r = (int)Math.Round((u - centerX) * Math.Cos(realTheta) + (v - centerY) * Math.Sin(realTheta));
                            r += houghHeight;
                            if (r < 0 || r >= 2 * input.GetLength(1))
                                continue;
                            houghArray[t, r]++;
                        }
            numPoints++;
            threshold = (int)(houghArray.Cast<int>().Max() * 0.4); // Sets the threshold used for the hough space to 4/10th of the highest peak
            return houghArray;
        }

        /// <summary>
        /// Return the thresholded peaks in hough space as lines in polar notation
        /// </summary>
        /// <param name="houghSpace"></param>
        /// <param name="threshold"></param>
        /// <returns></returns>
        public static List<FoundLines> GetLines(int[,] houghSpace, int threshold)
        {
            List<FoundLines> results = new List<FoundLines>(); // List for unknown length
            if (numPoints == 0)
            {
                Console.WriteLine("No lines found with the hough transform!");
                return results;
            }

            for (int t = 0; t < houghSpace.GetLength(0); t++)
                for (int r = 0; r < houghSpace.GetLength(1); r++)
                    // Apply threshold, store these values in an array of structs
                    if (houghSpace[t, r] >= threshold)
                        results.Add(new FoundLines(t * (0.5 * (Math.PI / 180)) + 0.85 * Math.PI, r, houghSpace[t, r]));

            Console.WriteLine("[Feature Selection] Hough Transform found " + results.Count + " lines");
            return results;
        }

        /// <summary>
        /// Keep only local maxima in Hough space
        /// </summary>
        /// <param name="houghSpace"></param>
        public static void NonMaximumSupression(int[,] houghSpace)
        {
            var startEnd = neighbourhood.GetLength(0) / 2; // Adjust for neighbouring cell size
            for (var t = startEnd; t < houghSpace.GetLength(0) - startEnd; t++)
                for (var r = startEnd; r < houghSpace.GetLength(1) - startEnd; r++)
                    for (byte i = 0; i < neighbourhood.GetLength(0); i++)
                        for (byte j = 0; j < neighbourhood.GetLength(1); j++)
                            if (houghSpace[t - startEnd + i, r - startEnd + j] > houghSpace[t, r])
                                houghSpace[t, r] = 0;
            Console.WriteLine("[Feature Selection] Non Maximum Suppression");
        }
    }
}
