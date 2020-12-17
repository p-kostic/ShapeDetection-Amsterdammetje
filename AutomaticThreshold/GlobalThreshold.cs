using System;

namespace INFOIBV
{
    internal static class GlobalThreshold
    {
        /// <summary>
        /// Apply Otsu's dynamic threshold method
        /// </summary>
        public static byte OtsuThreshold(byte[,] image)
        {
            var pixelCount = image.GetLength(0) * image.GetLength(1);
            var histogram = HistogramEqualization.CreateHistogram(image);

            var meanTotal = HistogramEqualization.ComputeMean(histogram); // Mean of the whole histogram

            var i0 = 0;
            var curMean = 0;

            float maxVar = 0;
            byte q = 0;

            for (var Q = 0; Q < histogram.Length; Q++) // Iterate over potential thresholds
            {
                // Compute P0 for current K
                i0 += histogram[Q];
                if (i0 == 0)
                    continue;

                var i1 = pixelCount - i0;
                if (i1 != 0)
                {
                    curMean += Q * histogram[Q];
                    float mean0 = curMean / i0; // Mean of first class
                    float mean1 = (meanTotal - curMean) / i1; // Mean of second class
                    var varianceBetween = (mean1 - mean0) * (mean1 - mean0) * i0 * i1; // Between class variance

                    // Check if new maximum found
                    if (!(varianceBetween > maxVar))
                        continue;

                    maxVar = varianceBetween;
                    q = (byte)Q;
                }
                else
                    break;
            }
            Console.WriteLine("[Pre-processing]    Otsu's method found a value of q = " + q + " and will now apply it.");
            return q;
        }
    }
}


