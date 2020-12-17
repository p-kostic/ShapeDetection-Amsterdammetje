using System;

namespace INFOIBV
{
    internal class Closing
    {
        public byte[,] Input;
        public float[,] Kernel;
        public int StartEnd;
        private readonly MorphologicalFilter.UseMask _e;

        public Closing(byte[,] input, float[,] kernel, MorphologicalFilter.UseMask E)
        {
            this.Input = input;
            this.Kernel = kernel;
            this.StartEnd = kernel.GetLength(0) / 2; // Adjust for all structuring element sizes
            this._e = E;
        }

        public byte[,] Close()
        {
            Console.WriteLine("[Morphology]      Initiating Closing...");
            // Dilation without a mask
            MorphologicalFilter f1 = new Dilation(this.Kernel, 1);
            var image = f1.Morph(this.Input, MorphologicalFilter.UseMask.No);

            // Erosion with a mask
            MorphologicalFilter f2 = new Erosion(this.Kernel, 1);
            image = f2.Morph(image, this._e);

            return image;
        }
    }
}