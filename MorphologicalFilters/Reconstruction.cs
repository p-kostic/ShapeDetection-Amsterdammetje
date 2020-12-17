
namespace INFOIBV
{
    internal class Reconstruction
    {
        public byte[,] Input;
        public float[,] Dkernel;
        public float[,] Ekernel;
        public int DstartEnd;
        private readonly MorphologicalFilter.UseMask _e;

        public Reconstruction(byte[,] input, float[,] Dkernel, float[,] Ekernel, MorphologicalFilter.UseMask E)
        {
            this.Input = input;
            this.Dkernel = Dkernel;
            this.Ekernel = Ekernel;
            this.DstartEnd = Dkernel.GetLength(0) / 2; // Adjust for all structuring element sizes
            this._e = E;
        }

        public byte[,] Reconstruct()
        {
            // Erosion without a mask
            MorphologicalFilter f1 = new Erosion(this.Ekernel, 1);
            var image = f1.Morph(this.Input, MorphologicalFilter.UseMask.No);

            byte[,] lastImage = new byte[this.Input.GetLength(0), this.Input.GetLength(1)];
            HelperFunctions.GlobalMask = this.Input;
            while (!IsEqual(image, lastImage))
            {
                lastImage = image;
                // Dilation with a mask
                MorphologicalFilter f2 = new Dilation(this.Dkernel, 1);
                image = f2.Morph(image, this._e);
            }

            return lastImage;
        }

        private bool IsEqual(byte[,] a, byte[,] b)
        {
            for (var x = 0; x < a.GetLength(0); x++)
                for (var y = 0; y < a.GetLength(1); y++)
                    if (a[x, y] != b[x + this.DstartEnd, y + this.DstartEnd])
                        return false;
            return true;
        }
    }
}