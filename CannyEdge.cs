using System;
using System.Collections.Generic;
using System.Linq;

namespace INFOIBV
{
    public static class CannyEdge
    {
        /// <summary>
        /// Perform Canny edge detection
        /// </summary>
        public static byte[,] CannyEdgeDetector(byte[,] input, int blur = 1, byte high = 90, byte low = 90)
        {
            var copy = BilateralFilter.BilateralFilter2D(input); // Preprocessing before detecting edges

            // ======================= Adapted Sobel edge detection from earlier implementation =======================
            var image1 = Convolution.Convolve(copy, EdgeDetection.SobelKernel3X3V, 1);
            var image2 = Convolution.Convolve(copy, EdgeDetection.SobelKernel3X3H, 1);
            var midImage1 = new double[image1.GetLength(0), image1.GetLength(1)];

            for (var x = 0; x < midImage1.GetLength(0); x++)
                for (var y = 0; y < midImage1.GetLength(1); y++)
                {
                    var final = Math.Sqrt(image1[x, y] * image1[x, y] + image2[x, y] * image2[x, y]);
                    midImage1[x, y] = (byte)EdgeDetection.ImageClamp2(Math.Round(final));
                }

            var image3 = Convolution.Convolve(copy, EdgeDetection.SobelKernel3X3Minv, 1);
            var image4 = Convolution.Convolve(copy, EdgeDetection.SobelKernel3X3Minh, 1);
            var midImage2 = new double[image1.GetLength(0), image1.GetLength(1)];

            for (var x = 0; x < midImage2.GetLength(0); x++)
                for (var y = 0; y < midImage2.GetLength(1); y++)
                {
                    var final = Math.Sqrt(image3[x, y] * image3[x, y] + image4[x, y] * image4[x, y]);
                    midImage2[x, y] = (byte)EdgeDetection.ImageClamp2(Math.Round(final));
                }

            var imageG = new double[image1.GetLength(0), image1.GetLength(1)];
            for (var x = 0; x < midImage2.GetLength(0); x++)
                for (var y = 0; y < midImage2.GetLength(1); y++)
                {
                    var final = midImage1[x, y] + midImage2[x, y];/*Math.Sqrt(midImage1[x, y] * midImage1[x, y] + midImage2[x, y] * midImage2[x, y]);*/
                    imageG[x, y] = (byte)EdgeDetection.ImageClamp2(Math.Round(final));
                }

            var imageT = new int[midImage1.GetLength(0), midImage1.GetLength(1)];

            for (var x = 0; x < imageT.GetLength(0); x++)
                for (var y = 0; y < imageT.GetLength(1); y++)
                    imageT[x, y] = (int)Math.Round(Math.Atan2(midImage2[x, y], midImage2[x, y]) * (5.0 / Math.PI) + 5) % 5;

            // ========================================================================================================


            var suppressed = Suppress(imageG, imageT); //Perform Non-maximum suppression

            var highEdges = HelperFunctions.ThresholdDouble(suppressed, high); // Keep the strong edges in one map
            var lowEdges = HelperFunctions.ThresholdDouble(suppressed, low);   // Keep weaker edges in another map

            var highEdgesByte = Double2DtoByte2D(highEdges);
            var lowEdgesByte = Double2DtoByte2D(lowEdges);

            var CombinedThresholds = BinaryOperators.ORoperator(highEdgesByte, lowEdgesByte);

            // Hysteresis to trace edges
            return Hysteresis(highEdgesByte, CombinedThresholds);
        }

        /// <summary>
        /// Convert image of double type to byte type using a clamp
        /// </summary>
        private static byte[,] Double2DtoByte2D(double[,] image)
        {
            var result = new byte[image.GetLength(0), image.GetLength(1)];
            for (var x = 0; x < image.GetLength(0); x++)
                for (var y = 0; y < image.GetLength(1); y++)
                    result[x, y] = (byte)EdgeDetection.ImageClamp2(Math.Round(image[x, y]));
            return result;
        }

        /// <summary>
        /// Apply Non-maximum suppression to given image
        /// </summary>
        private static double[,] Suppress(double[,] imageG, int[,] imageT)
        {
            var suppressed = imageG;
            for (var x = 0; x < imageG.GetLength(0); x++)
                for (var y = 0; y < imageG.GetLength(1); y++)
                {
                    if (x == 0 || x == imageG.GetLength(0) - 1 || y == 0 || y == imageG.GetLength(1) - 1)
                    {
                        imageG[x, y] = 0;
                        continue;
                    }
                    var localT = imageT[x, y] % 4;

                    switch (localT)
                    {
                        case 0:
                            if (imageG[x, y] <= imageG[x, y + 1] || imageG[x, y] <= imageG[x, y - 1])
                                suppressed[x, y] = 0;
                            break;
                        case 1:
                            if (imageG[x, y] <= imageG[x + 1, y - 1] || imageG[x, y] <= imageG[x - 1, y + 1])
                                suppressed[x, y] = 0;
                            break;
                        case 2:
                            if (imageG[x, y] <= imageG[x + 1, y] || imageG[x, y] <= imageG[x - 1, y])
                                suppressed[x, y] = 0;
                            break;
                        case 3:
                            if (imageG[x, y] <= imageG[x - 1, y - 1] || imageG[x, y] <= imageG[x + 1, y + 1])
                                suppressed[x, y] = 0;
                            break;
                    }

                }
            return suppressed;
        }

        /// <summary>
        /// Traverse the edges in the given image, keeping those weak edges that are close to a strong edge
        /// </summary>
        public static byte[,] Hysteresis(byte[,] highEdgesByte, byte[,] doubleThreshold)
        {
            var result = highEdgesByte;
            var pixels = new List<Tuple<int, int>>(); // Keeps track of what pixels still need considering

            for (var x = 1; x < result.GetLength(0) - 1; x++)
                for (var y = 1; y < result.GetLength(1) - 1; y++)
                {
                    if (doubleThreshold[x, y] != 255)
                        continue;

                    var localList = new List<byte>();
                    for (var i = -1; i < 2; i++)
                        for (var j = -1; j < 2; j++)
                            localList.Add(doubleThreshold[x + i, y + j]);

                    if (localList.Max() != 2)
                        continue;

                    pixels.Add(new Tuple<int, int>(x, y));
                    result[x, y] = 255;
                }

            while (pixels.Count > 0)
            {
                var newPixels = new List<Tuple<int, int>>();
                foreach (var pixel in pixels)
                {
                    for (var i = -1; i < 2; i++)
                        for (var j = -1; j < 2; j++)
                        {
                            if (i == 0 && j == 0)
                                continue;

                            var xLocal = pixel.Item1 + i;
                            var yLocal = pixel.Item2 + j;
                            if (doubleThreshold[xLocal, yLocal] != 255 || result[xLocal, yLocal] != 0)
                                continue;

                            newPixels.Add(new Tuple<int, int>(xLocal, yLocal));
                            result[xLocal, yLocal] = 255;
                        }
                }
                pixels = newPixels;
            }
            return result;
        }
    }
}
