using System;
using System.Collections.Generic;
using System.Linq;

namespace INFOIBV
{
    internal class Dilation : MorphologicalFilter
    {
        public Dilation(float[,] kernel, int times) : base(kernel, times)
        {

        }

        public override byte Filter(int x, int y, int i, int j, List<float> localArea, byte[,] input, float grayScaleClamp, UseMask e)
        {
            if (e != UseMask.Greyscale)
                grayScaleClamp = 255;

            localArea.Add(input[x - this.StartEnd + i, y - this.StartEnd + j] + this.Kernel[i, j]);
            return (byte)Math.Round(Clamp(localArea.Max(), 0f, grayScaleClamp));
        }
    }
}