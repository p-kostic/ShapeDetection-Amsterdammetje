using System;

namespace INFOIBV
{
    internal static class EdgeDetection
    {
        public static readonly float[,] SobelKernel3X3V =
        {
            { -1, 0, 1 },
            { -1, 0, 1 },
            { -1, 0 ,1 }
        };

        public static readonly float[,] SobelKernel3X3H =
        {
            { -1, -1, -1 },
            {  0,  0,  0 },
            {  1,  1,  1 }
        };

        public static readonly float[,] SobelKernel3X3Minv =
        {
            { 1, 0, -1 },
            { 1, 0, -1 },
            { 1, 0 ,-1 }
        };

        public static readonly float[,] SobelKernel3X3Minh =
        {
            {  1,  1,  1 },
            {  0,  0,  0 },
            { -1, -1, -1 }
        };


        public static readonly float[,] SobelKernel3X3WeightedH =
        {
            {  1, 2, 1 },
            {  0,  0,  0 },
            { -1, -2, -1 }
        };

        public static readonly float[,] SobelKernel3X3WeightedV =
        {
            {  -1, -2, -1 },
            {  0,  0,  0 },
            { 1, 2, 1 }
        };


        /// <summary>
        /// Edge detection using 4 Sobel kernels
        /// </summary>
        public static byte[,] DetectEdges(byte[,] input)
        {
            Console.WriteLine("[Pre-processing]    Applying Edge Detection using 4 Sobel kernels...");
            var image1 = Convolution.Convolve(input, SobelKernel3X3V, 1);
            var image2 = Convolution.Convolve(input, SobelKernel3X3H, 1);
            var midImage1 = new byte[image1.GetLength(0), image1.GetLength(1)];


            for (var x = 0; x < midImage1.GetLength(0); x++)
                for (var y = 0; y < midImage1.GetLength(1); y++)
                {
                    var final = Math.Sqrt(image1[x, y] * image1[x, y] + image2[x, y] * image2[x, y]); 
                    midImage1[x, y] = (byte)ImageClamp2(Math.Round(final));
                }

            var image3 = Convolution.Convolve(input, SobelKernel3X3Minv, 1);
            var image4 = Convolution.Convolve(input, SobelKernel3X3Minh, 1);
            var midImage2 = new byte[image1.GetLength(0), image1.GetLength(1)];

            for (var x = 0; x < midImage2.GetLength(0); x++)
                for (var y = 0; y < midImage2.GetLength(1); y++)
                {
                    var final = Math.Sqrt(image3[x, y] * image3[x, y] + image4[x, y] * image4[x, y]);
                    midImage2[x, y] = (byte)ImageClamp2(Math.Round(final));
                }

            var finalImage = new byte[image1.GetLength(0), image1.GetLength(1)];
            for (var x = 0; x < midImage2.GetLength(0); x++)
                for (var y = 0; y < midImage2.GetLength(1); y++)
                {
                    var final = Math.Sqrt(midImage1[x, y] * midImage1[x, y] + midImage2[x, y] * midImage2[x, y]);
                    finalImage[x, y] = (byte)ImageClamp2(Math.Round(final));
                }

            return finalImage;
        }

        /// <summary>
        /// Used to clamp a double in byte range
        /// </summary>
        public static double ImageClamp2(double n)
        {
            if (n < 0)
                n = 0;
            if (n > 255)
                n = 255;
            return n;
        }
    }
}
