namespace INFOIBV
{
    internal class PartialImage
    {
        public int X;
        public int Y;
        public byte[,] Image;

        public PartialImage(int x, int y, byte[,] image) {
            this.X = x;
            this.Y = y;
            this.Image = image;
        }
    }
}
