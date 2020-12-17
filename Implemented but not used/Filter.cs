using System;
using System.Collections.Generic;
using System.Linq;

namespace INFOIBV
{
    internal static class Filter
    {
        static readonly float[,] Laplacian3X3 =
        {
            {0,  1, 0 },
            {1, -4, 1 },
            {0,  1, 0 }
        };

        public static byte[,] Sharpen(byte[,] input, int w)
        {
            var imageToSharpen = Convolution.Convolve(input, Laplacian3X3, 1);
            var image = new byte[imageToSharpen.GetLength(0), imageToSharpen.GetLength(1)];

            for (var x = 0; x < imageToSharpen.GetLength(0); x++)
                for (var y = 0; y < imageToSharpen.GetLength(1); y++)
                    image[x, y] = (byte)Math.Round(HelperFunctions.ImageClamp(input[x, y] - w * imageToSharpen[x, y]));


            return image;
        }

        public static byte[,] MedianFilter(byte[,] input, int kernelSize)
        {
            var image = new byte[input.GetLength(0), input.GetLength(1)];

            var startEnd = kernelSize / 2;

            for (var x = startEnd; x < input.GetLength(0) - startEnd; x++)
                for (var y = startEnd; y < input.GetLength(1) - startEnd; y++)
                {
                    var window = new List<byte>();
                    for (byte i = 0; i < kernelSize; i++)
                        for (byte j = 0; j < kernelSize; j++)
                            window.Add(input[x - startEnd + i, y - startEnd + j]);

                    window.Sort();
                    image[x, y] = window.ElementAt(window.Count / 2);
                }

            return HelperFunctions.CropImage(image, startEnd);
        }
    }
}
