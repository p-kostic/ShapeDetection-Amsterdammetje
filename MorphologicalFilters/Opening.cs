using System;

namespace INFOIBV
{
    internal class Opening
    {
        public byte[,] Input;
        public float[,] Kernel;
        public int StartEnd;
        private readonly MorphologicalFilter.UseMask _e;

        public Opening(byte[,] input, float[,] kernel, MorphologicalFilter.UseMask E)
        {
            this.Input = input;
            this.Kernel = kernel;
            this.StartEnd = kernel.GetLength(0) / 2; // Adjust for all structuring element sizes
            this._e = E;
        }

        public byte[,] Open(float grayScaleMaximum = 255)
        {
            if (grayScaleMaximum <= 0)
                throw new ArgumentOutOfRangeException(nameof(grayScaleMaximum));
            Console.WriteLine("[Morphology]      Initiating Opening...");

            // Erosion without a mask
            MorphologicalFilter f1 = new Erosion(this.Kernel, 1);
            var image = f1.Morph(this.Input, MorphologicalFilter.UseMask.No);

            // Dilation with a mask
            MorphologicalFilter f2 = new Dilation(this.Kernel, 1);
            image = f2.Morph(image, this._e);

            return image;
        }
    }
}