using System;
using System.Collections.Generic;
using System.Drawing;

namespace INFOIBV
{
    internal static class HelperFunctions
    {
        public static byte[,] GlobalMask;

        public static readonly float[,] GaussianBlur5X5 =
        {
            {1 / 256f, 4 / 256f, 6 / 256f, 4 / 256f, 1 / 256f},
            {4 / 256f, 16 / 256f, 24 / 256f, 16 / 256f, 4 / 256f},
            {6 / 256f, 24 / 256f, 36 / 256f, 24 / 256f, 6 / 256f},
            {4 / 256f, 16 / 256f, 24 / 256f, 16 / 256f, 4 / 256f},
            {1 / 256f, 4 / 256f, 6 / 256f, 4 / 256f, 1 / 256f}
        };

        public static readonly float[,] GaussianBlur3X3 =
        {
            {1 / 16f, 1 / 8f, 1 / 16f},
            {1 / 8f, 1 / 4f, 1 / 8f},
            {1 / 16f, 1 / 8f, 1 / 16f}
        };


        /// <summary>
        /// Take a color image and make it single-channel greyscale
        /// </summary>
        public static byte[,] Greyscalize(Color[,] input)
        {
            var image = new byte[input.GetLength(0), input.GetLength(1)];
            for (var i = 0; i < input.GetLength(0); i++)
                for (var j = 0; j < input.GetLength(1); j++)
                {
                    var pixelColor = input[i, j];
                    // Weights from section 8.2.1 of the book: Principles of Digital Image Processing
                    var average = (byte)(pixelColor.R * 0.299f + pixelColor.B * 0.114f + pixelColor.G * 0.587);
                    image[i, j] = average;
                }

            Console.WriteLine("[Pre-processing]    Greyscalized the image/mask");
            return image;
        }

        /// <summary>
        /// Perform given threshold on given image
        /// </summary>
        public static byte[,] Threshold(byte[,] input, byte threshold)
        {
            var image = new byte[input.GetLength(0), input.GetLength(1)];
            for (var x = 0; x < image.GetLength(0); x++)
                for (var y = 0; y < image.GetLength(1); y++)
                {
                    if (input[x, y] <= threshold)
                        image[x, y] = 0;
                    else
                        image[x, y] = 255;
                }

            Console.WriteLine("[Pre-processing]    Threshold of the image/mask at q = " + threshold);
            return image;
        }

        /// <summary>
        /// Crop image by given amount of pixels on each side. Used to keep the mask in check after the
        /// working image has been shrunk by kernel applications.
        /// </summary>
        public static byte[,] CropImage(byte[,] input, int startEnd)
        {
            var image = new byte[input.GetLength(0) - startEnd * 2, input.GetLength(1) - startEnd * 2];
            for (var x = startEnd; x < input.GetLength(0) - startEnd; x++)
                for (var y = startEnd; y < input.GetLength(1) - startEnd; y++)
                    image[x - startEnd, y - startEnd] = input[x, y];

            WriteCrop(startEnd, input);
            return image;
        }

        /// <summary>
        /// Clamp a float into byte range
        /// </summary>
        public static float ImageClamp(float n)
        {
            if (n < 0)
                n = 0;
            if (n > 255)
                n = 255;
            return n;
        }

        /// <summary>
        /// Get the complement of a binary image
        /// </summary>
        public static byte[,] Complement(byte[,] input)
        {
            var image = new byte[input.GetLength(0), input.GetLength(1)];
            for (var x = 0; x < image.GetLength(0); x++)
                for (var y = 0; y < image.GetLength(1); y++)
                {
                    if (input[x, y] == 255)
                        image[x, y] = 0;
                    else
                        image[x, y] = 255;
                }
            Console.WriteLine("[Pre-processing]    The inverse of the image/mask was taken");
            return image;
        }

        private static void WriteCrop(int startEnd, byte[,] input)
        {
            if (input == GlobalMask)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("[Notice]            The mask  was reduced by " + startEnd * 2 + " pixels in height and width because of a filter");
                Console.ResetColor();
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("[Notice]            The image was reduced by " + startEnd * 2 + " pixels in height and width because of a filter");
                Console.ResetColor();
            }
        }

        /// <summary>
        /// Perform threshold on a map consisting of doubles
        /// </summary>
        /// <param name="input"></param>
        /// <param name="threshold"></param>
        /// <returns></returns>
        public static double[,] ThresholdDouble(double[,] input, byte threshold)
        {
            var image = new double[input.GetLength(0), input.GetLength(1)];
            for (var x = 0; x < image.GetLength(0); x++)
                for (var y = 0; y < image.GetLength(1); y++)
                {
                    if (input[x, y] <= threshold)
                        image[x, y] = 0;
                    else
                        image[x, y] = 255;
                }

            Console.WriteLine("[Pre-processing]    Threshold of the image/mask at q = " + threshold);
            return image;
        }

        /// <summary>
        /// Create a list of subimages out of just the surroundings of a list of pairs in an image
        /// </summary>
        /// <param name="pairs"></param>
        /// <param name="image"></param>
        /// <returns></returns>
        public static List<PartialImage> CutPairs(List<Pair> pairs, byte[,] image) {
            var result = new List<PartialImage>();

            foreach (var p in pairs) {
                var x0 = -((p.L1.B - image.GetLength(1)) / p.L1.A);
                var x1 = -((p.l2.B - image.GetLength(1)) / p.l2.A);
                result.Add(x0 < x1
                    ? CutImage(image, (int) x0, 0, (int) x1, image.GetLength(1))
                    : CutImage(image, (int) x1, 0, (int) x0, image.GetLength(1)));
            }

            return result;
        }

        /// <summary>
        /// Given coordinates within an image, return just that part of the image as a PartialImage object
        /// </summary>
        /// <param name="image"></param>
        /// <param name="x0"></param>
        /// <param name="y0"></param>
        /// <param name="x1"></param>
        /// <param name="y1"></param>
        /// <returns></returns>
        public static PartialImage CutImage(byte[,] image, int x0, int y0, int x1, int y1) {
            var subImage = new byte[Math.Abs(x1 - x0), Math.Abs(y1 - y0)];
            for (var x = x0; x < x1; x++) {
                for (var y = y0; y < y1; y++) {
                    subImage[x - x0, y - y0] = image[x, y];
                }
            }
            return new PartialImage(x0, y0, subImage);
        }
    }
}
