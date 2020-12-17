using System;

namespace INFOIBV
{
    static class BilateralFilter
    {
        private static readonly float[,] GaussianKernel = {
            { 1, 4, 7, 4, 1},
            { 4,16,26,16, 4},
            { 7,26,41,26, 7},
            { 4,16,26,16, 4},
            { 1, 4, 7, 4, 1}
        };

        /// <summary>
        /// The bilateral filter is a weighted average of pixels that, unlike the gaussian blur, takes the variation of brightness into account to
        /// preserve edges as much as possible. It checks not only if two pixels are spatially close to one another, but also if they are close in
        /// terms of brightness. It uses a gaussian kernel for the spatial relation and a one-dimensional range kernel to measure the differences in
        /// brightness of the different pixels.
        /// </summary>
        /// <param name="image">The binary image to be filtered.</param> 
        public static byte[,] BilateralFilter2D(byte[,] image)
        {
            // Make a float version of the image to make sure no important information is lost in rounding
            var workingImage = CastToFloat(image);
            const float sigma = 80; // Sigma is used to generate the range kernel

            // Create the range kernel
            var rangeKernel = new float[256]; // Used for the brightness-based weighting.
            for (var i = 0; i < rangeKernel.Length; i++)
            {
                double val = (i * i) / (2 * sigma * sigma);
                var kernelVal = Math.Exp(-val);
                rangeKernel[i] = (float)kernelVal;
            }

            workingImage = Normalize(workingImage, 0, 1); // Normalize all the values to the [0,1] range

            for (var x = 2; x < image.GetLength(0) - 2; x++)
            {
                for (var y = 2; y < image.GetLength(1) - 2; y++)
                {
                    var targetPixel = (int)workingImage[x, y]; // The pixel that is to be replaced by a new value
                    float result = 0;
                    float sum = 0; // The sum of the weighted neighbour values
                    for (var i = -2; i < 2; i++) // Cycle through the kernel and compute the coefficients to add to the summation
                    {
                        for (var j = -2; j < 2; j++)
                        {
                            var curPixel = (int)workingImage[x + i, y + j];
                            var weight = GaussianKernel[i + 2, j + 2] * rangeKernel[Math.Abs(curPixel - targetPixel)];
                            result += curPixel * weight;
                            sum += weight;
                        }
                    }
                    workingImage[x, y] = result / sum; // Replace the pixel in the output image with the determined value
                }
            }

            return HelperFunctions.CropImage(CastToByte(workingImage), 2); // Return the image as an array of ints and without the borders
        }

        /// <summary>
        /// Casts an entire image array from int to float.
        /// </summary>
        public static float[,] CastToFloat(byte[,] image)
        {
            var newImage = new float[image.GetLength(0), image.GetLength(1)];

            for (var x = 0; x < image.GetLength(0); x++)
                for (var y = 0; y < image.GetLength(1); y++)
                    newImage[x, y] = image[x, y];
            return newImage;
        }

        /// <summary>
        /// Normalize an image to a given range
        /// </summary>
        public static float[,] Normalize(float[,] image, float newMin, float newMax)
        {
            var newImage = new float[image.GetLength(0), image.GetLength(1)];
            float min = 0;
            float max = 255;
            for (var x = 0; x < image.GetLength(0); x++)
            {
                for (var y = 0; y < image.GetLength(1); y++)
                {
                    if (image[x, y] < min)
                        min = image[x, y];
                    if (image[x, y] > max)
                        max = image[x, y];
                }
            }

            for (var x = 0; x < image.GetLength(0); x++)
                for (var y = 0; y < image.GetLength(1); y++)
                    newImage[x, y] = Clamp(((image[x, y] - min) * (newMax - newMin / max - min)) + newMin);

            return newImage;
        }

        public static float Clamp(float val)
        {
            float newVal;
            if (val < 0)
                newVal = 0;
            else if (val > 255)
                newVal = 255;
            else
                newVal = val;

            return newVal;
        }

        public static byte[,] CastToByte(float[,] image)
        {
            var newImage = new byte[image.GetLength(0), image.GetLength(1)];

            for (var x = 0; x < image.GetLength(0); x++)
                for (var y = 0; y < image.GetLength(1); y++)
                    newImage[x, y] = (byte)Math.Round(image[x, y]);
            return newImage;
        }
    }
}