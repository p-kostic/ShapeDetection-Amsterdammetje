using System;
using System.Collections.Generic;

namespace INFOIBV
{
    internal abstract class MorphologicalFilter
    {
        public float[,] Kernel;
        public int StartEnd;
        public int Times;

        public enum UseMask
        {
            No,
            Binary,
            Greyscale
        }

        protected MorphologicalFilter(float[,] kernel, int times)
        {
            this.Kernel = kernel;
            this.Times = times;
            this.StartEnd = kernel.GetLength(0) / 2; // Adjust for all kernel sizes
        }

        public virtual byte[,] Morph(byte[,] input, UseMask E)
        {
            var image = new byte[input.GetLength(0), input.GetLength(1)];

            for (var x = this.StartEnd; x < input.GetLength(0) - this.StartEnd; x++)
                for (var y = this.StartEnd; y < input.GetLength(1) - this.StartEnd; y++)
                    switch (E)
                    {
                        case UseMask.Binary when HelperFunctions.GlobalMask[x, y] == 255:
                        {
                            var localArea = new List<float>(this.Kernel.GetLength(0) * this.Kernel.GetLength(0)); // storage of local area
                            for (byte i = 0; i < this.Kernel.GetLength(0); i++)
                            for (byte j = 0; j < this.Kernel.GetLength(1); j++)
                                if (HelperFunctions.GlobalMask[x - this.StartEnd + i, y - this.StartEnd + j] == 255)
                                    image[x, y] = Filter(x, y, i, j, localArea, input, 100, E);
                            break;
                        }
                        // don't filter this pixel 
                        case UseMask.Binary:
                            image[x, y] = input[x, y];
                            break;
                        case UseMask.Greyscale:
                        {
                            var localArea = new List<float>(this.Kernel.GetLength(0) * this.Kernel.GetLength(0)); // storage of local area
                            for (byte i = 0; i < this.Kernel.GetLength(0); i++)
                            for (byte j = 0; j < this.Kernel.GetLength(1); j++)
                                image[x, y] = Filter(x, y, i, j, localArea, input, HelperFunctions.GlobalMask[x, y], E); // use the mask's position as the max or min clamp value for grayscale masks 
                            break;
                        }
                        default:
                        {
                            var localArea = new List<float>(this.Kernel.GetLength(0) * this.Kernel.GetLength(0)); // storage of local area
                            for (byte i = 0; i < this.Kernel.GetLength(0); i++)
                            for (byte j = 0; j < this.Kernel.GetLength(1); j++)
                                image[x, y] = Filter(x, y, i, j, localArea, input, 100, E); // Perform the normal erosion/dilation without a mask (the 100 argument here gets changed in Filter)
                            break;
                        }
                    }

            if (this.Times > 1)
            {
                this.Times--;
                image = Morph(image, E);
            }

            Console.WriteLine("[Morphology]        Dilation or Erosion was applied " + this.Times + " time(s) with a " + this.Kernel.GetLength(0) + "x" + this.Kernel.GetLength(1) + " kernel with " + E + " Mask.");
            HelperFunctions.GlobalMask = HelperFunctions.CropImage(HelperFunctions.GlobalMask, this.StartEnd);
            return HelperFunctions.CropImage(image, this.StartEnd);
        }

        public abstract byte Filter(int x, int y, int i, int j, List<float> localArea, byte[,] input, float grayScaleClamp, UseMask e);

        #region HelperFunction

        /// <summary>
        /// For greyscale control images, the clamp can be specified
        /// </summary>
        public static float Clamp(float value, float min, float max)
        {
            return (value < min) ? min : (value > max) ? max : value;
        }
        #endregion
    }
}