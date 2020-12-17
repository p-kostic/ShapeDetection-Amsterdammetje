namespace INFOIBV
{
    internal static class BinaryOperators
    {
        /// <summary>
        /// Computes the complement of a given image
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
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
            return image;
        }

        /// <summary>
        /// Applies the AND operator to two images
        /// </summary>
        /// <param name="input1"></param>
        /// <param name="input2"></param>
        /// <returns></returns>
        public static byte[,] ANDoperator(byte[,] input1, byte[,] input2)
        {
            var image = new byte[input1.GetLength(0), input1.GetLength(1)];
            for (var x = 0; x < image.GetLength(0); x++)
                for (var y = 0; y < image.GetLength(1); y++)
                {
                    if (input1[x, y] == 255 && input2[x, y] == 255)
                        image[x, y] = 255;
                    else
                        image[x, y] = 0;
                }

            return image;
        }

        /// <summary>
        /// Applies the OR operator to two images
        /// </summary>
        /// <param name="input1"></param>
        /// <param name="input2"></param>
        /// <returns></returns>
        public static byte[,] ORoperator(byte[,] input1, byte[,] input2)
        {
            var image = new byte[input1.GetLength(0), input1.GetLength(1)];
            for (var x = 0; x < image.GetLength(0); x++)
                for (var y = 0; y < image.GetLength(1); y++)
                {
                    if (input1[x, y] == 255 || input2[x, y] == 255)
                        image[x, y] = 255;
                    else
                        image[x, y] = 0;
                }

            return image;
        }
    }
}