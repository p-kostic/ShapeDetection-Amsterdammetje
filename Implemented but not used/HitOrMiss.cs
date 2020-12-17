namespace INFOIBV
{
    internal static class HitOrMiss
    {
        public static readonly bool[,] StrucElement =
        {
            { false, false,  false },
            { true, false, false },
            { false, false, false }
        };


        public static byte[,] HitIt(byte[,] input, bool[,] strucElement)
        {
            var image = new byte[input.GetLength(0), input.GetLength(1)];
            var startEnd = strucElement.GetLength(0) / 2; // Adjust for all kernel sizes

            for (var x = startEnd; x < input.GetLength(0) - startEnd; x++)
                for (var y = startEnd; y < input.GetLength(1) - startEnd; y++)
                {
                    image[x, y] = HitCheck(x, y, strucElement, input);
                }
            return ORoperator(input, image);
        }

        public static byte HitCheck(int x, int y, bool[,] strucElement, byte[,] input)
        {
            var startEnd = strucElement.GetLength(0) / 2; // Adjust for all kernel sizes
            for (byte i = 0; i < strucElement.GetLength(0); i++)
                for (byte j = 0; j < strucElement.GetLength(1); j++)
                {
                    if (strucElement[i, j] && input[x - startEnd + i, y - startEnd + j] == 0)
                        return 0;
                    if (!strucElement[i, j] && input[x - startEnd + i, y - startEnd + j] == 255)
                        return 0;
                }
            return 255;
        }

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
