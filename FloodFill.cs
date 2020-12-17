using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;


namespace INFOIBV
{
    /// <summary>
    /// Keeps track of information regarding a labeled object
    /// </summary>
    public class PictureObject
    {
        public int ObjectNumber;
        public int XMin;
        public int XMax;
        public int YMin;
        public int YMax;

        public Tuple<int, int> TopLeft;
        public Tuple<int, int> TopRight;
        public Tuple<int, int> BottomLeft;
        public Tuple<int, int> BottomRight;

        public PictureObject(int objectNumber)
        {
            this.ObjectNumber = objectNumber;
        }
    }


    internal static class FloodFill
    {
        public static byte ObjectCount;
        private static byte[,] _objectMap;
        public static List<PictureObject> Objects = new List<PictureObject>();

        /// <summary>
        /// Marks objects with labels using flood fill techniques
        /// </summary>
        public static byte[,] MarkObjects(byte[,] image)
        {
            _objectMap = new byte[image.GetLength(0), image.GetLength(1)];

            for (var x = 0; x < image.GetLength(0); x++)
                for (var y = 0; y < image.GetLength(1); y++)
                    if (image[x, y] > 0 && _objectMap[x, y] == 0)
                    {
                        ObjectCount++;
                        var p = new PictureObject(ObjectCount) { XMin = 512, XMax = 0, YMin = 512, YMax = 0 };
                        Objects.Add(p);
                        CheckNeighboringPixels(image, x, y);
                    }

            Console.WriteLine("[Feature Selection] Flood Fill found " + ObjectCount + " objects to draw.");

            CalculateCorners(_objectMap);

            return _objectMap;
        }

        /// <summary>
        /// Iterative  flood fill
        /// </summary>
        private static void CheckNeighboringPixels(byte[,] image, int x, int y)
        {
            while (true)
            {
                if (image[x, y] > 0 && _objectMap[x, y] == 0)
                {
                    _objectMap[x, y] = ObjectCount;

                    // Position with smallest x and y for a single object
                    if (Objects[ObjectCount - 1].XMin > x) 
                        Objects[ObjectCount - 1].XMin = x;
                    if (Objects[ObjectCount - 1].YMin > y) 
                        Objects[ObjectCount - 1].YMin = y;

                    // Position with biggest x and y for a single object
                    if (Objects[ObjectCount - 1].XMax < x) 
                        Objects[ObjectCount - 1].XMax = x;
                    if (Objects[ObjectCount - 1].YMax < y) 
                        Objects[ObjectCount - 1].YMax = y;

                    if (x + 1 < image.GetLength(0)) 
                        CheckNeighboringPixels(image, x + 1, y);
                    if (x - 1 > 0) 
                        CheckNeighboringPixels(image, x - 1, y);
                    if (y + 1 < image.GetLength(1))
                        CheckNeighboringPixels(image, x, y + 1);
                    if (y - 1 > 0)
                    {
                        y -= 1;
                        continue;
                    }
                }
                break;
            }
        }

        /// <summary>
        /// Save the bounding box of an object 
        /// </summary>
        public static void CalculateCorners(byte[,] image)
        {
            foreach (var pictureObject in Objects)
            {
                pictureObject.TopLeft = new Tuple<int, int>(pictureObject.XMin, pictureObject.YMin);
                pictureObject.TopRight = new Tuple<int, int>(pictureObject.XMax, pictureObject.YMin);
                pictureObject.BottomLeft = new Tuple<int, int>(pictureObject.XMin, pictureObject.YMax);
                pictureObject.BottomRight = new Tuple<int, int>(pictureObject.XMax, pictureObject.YMax);
            }
        }

        /// <summary>
        /// Remove objects that are not tall enough
        /// </summary>
        public static byte[,] filterObjects(byte[,] image)
        {
            var result = new byte[image.GetLength(0), image.GetLength(1)];

            for (var x = 0; x < image.GetLength(0); x++)
                for (var y = 0; y < image.GetLength(1); y++)
                {
                    if (image[x, y] <= 0) 
                        continue;

                    var height = Math.Abs(Objects[image[x, y] - 1].TopLeft.Item2 - Objects[image[x, y] - 1].BottomLeft.Item2);
                    var width = Math.Abs(Objects[image[x, y] - 1].TopLeft.Item1 - Objects[image[x, y] - 1].BottomLeft.Item1);

                    if (height > 50 && width / height < 0.5)
                        result[x, y] = 255;
                }

            var objects = new List<PictureObject>();
            foreach (var o in Objects.Where(o => Math.Abs(o.TopLeft.Item2 - o.BottomLeft.Item2) > 50))
            {
                objects.Add(o);
                ObjectCount--;
            }
            Objects = objects;

            return result;
        }

        /// <summary>
        /// Draws the final shapes over the original input image.
        /// </summary>
        public static void DrawFinal(Bitmap image, List<Pair> pairs)
        {
            for (var x = 0; x < image.Width; x++)
            {
                for (var y = 0; y < image.Height; y++)
                {
                    var betweenAPair = false;
                    for (var i = 0; i < pairs.Count; i++)
                    {
                        var xl0 = -((pairs[i].L1.B - y) / pairs[i].L1.A);
                        var xl1 = -((pairs[i].l2.B - y) / pairs[i].l2.A);
                        if (x <= xl0 && x >= xl1)
                            betweenAPair = true;
                    }

                    var insideAbb = false;
                    foreach (var unused in Objects.Where(o =>  y > o.TopLeft.Item2 && y < o.BottomLeft.Item2 && x > o.TopLeft.Item1 && x < o.TopRight.Item1))
                        insideAbb = true;

                    if (!betweenAPair || !insideAbb)
                        continue;

                    var highlight =
                        Color.FromArgb((int) HelperFunctions.ImageClamp((int) (image.GetPixel(x, y).R + 100)), image.GetPixel(x, y).G, image.GetPixel(x, y).B);
                    image.SetPixel(x, y, highlight);
                }
            }
        }
    }
}
