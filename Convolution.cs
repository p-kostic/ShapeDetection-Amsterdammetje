using System;

namespace INFOIBV
{
    internal static class Convolution
    {
        /// <summary>
        /// Apply convolution with a given kernel a given amount of times
        /// </summary>
        public static byte[,] Convolve(byte[,] input, float[,] kernel, int times)
        {
            var image = new byte[input.GetLength(0), input.GetLength(1)];
            var startEnd = kernel.GetLength(0) / 2; // Adjust for all kernel sizes

            for (var x = startEnd; x < input.GetLength(0) - startEnd; x++)
                for (var y = startEnd; y < input.GetLength(1) - startEnd; y++)
                {
                    float total = 0;
                    for (byte i = 0; i < kernel.GetLength(0); i++)
                        for (byte j = 0; j < kernel.GetLength(1); j++)
                            total += kernel[i, j] * input[x - startEnd + i, y - startEnd + j];
                    image[x, y] = (byte)Math.Round(HelperFunctions.ImageClamp(total));
                }

            if (times > 1)
                image = Convolve(image, kernel, times - 1);

            HelperFunctions.GlobalMask = HelperFunctions.CropImage(HelperFunctions.GlobalMask, startEnd);
            return HelperFunctions.CropImage(image, startEnd);
        }

        public static byte[,] ApplyGaussianBlur(byte[,] input, int times)
        {
            Console.WriteLine("[Filtering]         GaussionBlur was applied " + times + " time(s) using a 5x5 kernel");
            return Convolve(input, HelperFunctions.GaussianBlur3X3, times);
        }
    }
}
