using System;
using System.Collections.Generic;
using System.Linq;

namespace INFOIBV
{
    public static class HistogramEqualization
    {
        /// <summary>
        /// Approximation by distributing the grey values as evenly as possible.
        /// </summary>
        /// <param name="image">The image it will create a histogram for </param>
        public static byte[,] Equalize(byte[,] image)
        {
            var newImage = new byte[image.GetLength(0), image.GetLength(1)];
            var histogram = CreateHistogram(image);
            var cv = CalculateCumulativeHistogram(histogram);

            // Determine the ideal number of pixels per bin: total number of pixels / number of bins
            var totalNrOfPixels = image.GetLength(0) * image.GetLength(1);
            var _i = (float)totalNrOfPixels / 256;
            var gv = CalculateRemapping(_i, cv);

            // Now that we have the equalized histogram, we loop over the image.
            for (var i = 0; i < image.GetLength(0); i++)
                for (var j = 0; j < image.GetLength(1); j++)
                {
                    // Switch the old location in image with the new location in gv
                    int oldBin = image[i, j];
                    newImage[i, j] = (byte)gv[oldBin];
                }

            Console.WriteLine("> Histogram Equalization was used on the image");
            return newImage;
        }

        /// <summary>
        /// Calculates the histogram by distributing the frequency of grey values of the image in 'bins' 
        /// </summary>
        /// <param name="image">The 2D image</param>
        /// <returns>An array representing a histogram</returns>
        public static int[] CreateHistogram(byte[,] image)
        {
            var histogram = new int[256];
            // Calculate the Histogram
            // Add the image values at the correct bins
            for (var i = 0; i < image.GetLength(0); i++)
                for (var j = 0; j < image.GetLength(1); j++)
                    histogram[image[i, j]]++;

            return histogram;
        }

        /// <summary>
        /// Calculates the number of pixel values that will be remapped according to the algorithm in the slides
        /// </summary>
        /// <param name="histogram">The histogram of the original image</param>
        /// <returns>An array representing all previous </returns>
        private static int[] CalculateCumulativeHistogram(IList<int> histogram)
        {
            var cv = new int[256];

            // Calculate cumulative 
            for (var i = 0; i < cv.Length; i++)
            {
                if (i > 0)
                    cv[i] = histogram[i] + cv[i - 1];
                else
                    cv[i] = histogram[i]; // No addition for the first bin
            }
            return cv;
        }

        /// <summary>
        /// Calculates the values according to the remapping algorithm in the slides.
        /// </summary>
        /// <param name="_i">The ideal number of pixels per bin</param>
        /// <param name="cv">The cumulative histogram</param>
        private static int[] CalculateRemapping(float _i, int[] cv)
        {
            var gv = new int[256];
            for (var i = 0; i < gv.Length; i++)
            {
                var gvCurrent = (int)Math.Floor((double)cv[i] / _i + 0.5 - 1); // The Math.Floor is carried out by the int cast.
                if (gvCurrent < 0)
                    gvCurrent = 0;
                gv[i] = gvCurrent;
            }
            return gv;
        }

        public static int ComputeMean(int[] histogram)
        {
            return histogram.Select((t, i) => i * t).Sum();
        }
    }
}