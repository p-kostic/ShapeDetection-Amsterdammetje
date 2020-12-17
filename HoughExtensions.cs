using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace INFOIBV
{
    public struct Line
    {
        /// <summary>
        /// Struct used to represent lines converted to y = ax + b notation
        /// </summary>
        public double A;
        public double B;
        public Color C;

        public Line(double a, double b, Color c)
        {
            this.A = a;
            this.B = b;
            this.C = c;
        }

        /// <summary>
        /// Function used to check if lines are equal to each other, employs rounding
        /// </summary>
        public static bool Equals(Line c1, Line c2)
        {
            return (int)c1.A == (int)c2.A && (int)c1.B == (int)c2.B;
        }

        /// <summary>
        /// Function used to check if lines are not equal to each other, employs rounding
        /// </summary>
        public static bool Unequals(Line c1, Line c2)
        {
            return (int)c1.A != (int)c2.A || (int)c1.B != (int)c2.B;
        }
    }

    public struct Pair
    {
        /// <summary>
        /// Struct used to represent pairs of lines along with information specific to the two
        /// </summary>
        public Tuple<double, double> Intersection;
        public Line L1;
        public readonly Line l2;
        public Pair(Tuple<double, double> intersection, Line l1, Line l2)
        {
            this.Intersection = intersection;
            this.L1 = l1;
            this.l2 = l2;
        }

        /// <summary>
        /// Function used to check if pairs are not equal to each other
        /// </summary>
        public static bool Equals(Pair p1, Pair p2)
        {
            return Line.Equals(p1.L1, p2.L1) && Line.Equals(p1.l2, p2.l2);
        }
    }

    public static class HoughExtensions
    {
        /// <summary>
        /// Converts all polar lines to cartesian lines
        /// </summary>
        public static List<Line> GetAllCartesianLines(List<FoundLines> polarLines, byte[,] outputImage)
        {
            return polarLines.Select(t => MakeCartesianLine(t, outputImage)).ToList();
        }

        /// <summary>
        /// Calculates the intersection between two cartesian lines
        /// </summary>
        public static Tuple<double, double> CalculateIntersection(Line l1c, Line l2c)
        {
            // a x + b = a x + b
            var x = (l2c.B - l1c.B) / (l1c.A - l2c.A);
            var y = l1c.A * x + l1c.B;

            Console.WriteLine("x = " + x + ", y = " + y);
            return new Tuple<double, double>(x, y);
        }

        /// <summary>
        /// Convert a line in polar coordinates to cartesian notation
        /// </summary>
        public static Line MakeCartesianLine(FoundLines l, byte[,] outputImage)
        {
            double height = outputImage.GetLength(1);
            double width = outputImage.GetLength(0);
            // During processing h_h is doubled so that -ve r values 
            var houghHeight = (Math.Sqrt(2) * Math.Max(height, width)) / 2;
            // Find edge points and vote in array 
            var centerX = width / 2;
            var centerY = height / 2;
            const int y1 = 0;
            var y2 = height;

            var x1 = (l.Rho - houghHeight - (y1 - centerY) * Math.Sin(l.Theta)) / Math.Cos(l.Theta) + centerX;
            var x2 = (l.Rho - houghHeight - (y2 - centerY) * Math.Sin(l.Theta)) / Math.Cos(l.Theta) + centerX;

            var a = (y2 - y1) / (x2 - x1);
            var b = y1 - x1 * a;

            return new Line(a, b, Color.Lime);
        }

        /// <summary>
        /// Pairs up lines while adding a bit of padding for later use
        /// </summary>
        public static List<Pair> GetRoughPairs(List<Line> lines, int height)
        {
            var potentialPairs = new List<Pair>();
            var r = new Random();
            for (var i = 0; i < lines.Count; i++)
                for (var j = 0; j < lines.Count; j++)
                {
                    // Make sure we are not checking the line against itself
                    if ((int)lines[i].A == (int)lines[j].A && (int)lines[i].B == (int)lines[j].B)
                        continue;

                    var intersection = CalculateIntersection(lines[i], lines[j]);

                    // We're not interested in non-negative intersections in the y direction
                    if (intersection.Item2 > 0)
                        continue;

                    var r1 = r.Next(250);
                    var r2 = r.Next(250);
                    var r3 = r.Next(250);
                    var randomColor = Color.FromArgb(r1, r2, r3);
                    var l1 = lines[i].A < 0 ? new Line(lines[i].A, lines[i].B + 100, randomColor) : new Line(lines[i].A, lines[i].B - 100, randomColor);
                    var l2 = lines[j].A < 0 ? new Line(lines[j].A, lines[j].B - 100, randomColor) : new Line(lines[j].A, lines[j].B + 100, randomColor);
                    potentialPairs.Add(new Pair(intersection, l1, l2));
                }

            var filtered = potentialPairs.GroupBy(x => Math.Round(x.Intersection.Item2)).Select(y => y.First()).ToList();
            var sorted = filtered.OrderBy(x => x.Intersection.Item2).Reverse().ToList();
            sorted = UniqueFilter(sorted, height);

            var result = new List<Pair>();
            for (var i = 0; i < result.Count; i++)
            {
                result.Add(sorted[i]);
                Console.WriteLine("Pair: y = " + sorted[i].L1.A + "x + " + sorted[i].L1.B + " with y = " + sorted[i].l2.A + "x + " + sorted[i].l2.B + "With intersection (" + sorted[i].Intersection.Item1 + " , " + sorted[i].Intersection.Item2 + ")");
            }

            return sorted;
        }

        /// <summary>
        /// Pairs up lines that are likely part of the same pole
        /// </summary>
        public static List<Pair> GetPairs(List<Line> lines, int height)
        {
            var potentialPairs = new List<Pair>();
            var r = new Random();
            for (var i = 0; i < lines.Count; i++)
                for (var j = 0; j < lines.Count; j++)
                {
                    // Make sure we are not checking the line against itself
                    if ((int)lines[i].A == (int)lines[j].A && (int)lines[i].B == (int)lines[j].B)
                        continue;

                    var intersection = CalculateIntersection(lines[i], lines[j]);

                    // We're not interested in non-negative intersections in the y direction
                    if (intersection.Item2 > 0)
                        continue;

                    var r1 = r.Next(250);
                    var r2 = r.Next(250);
                    var r3 = r.Next(250);
                    var randomColor = Color.FromArgb(r1, r2, r3);
                    var l1 = new Line(lines[i].A, lines[i].B, randomColor);
                    var l2 = new Line(lines[j].A, lines[j].B, randomColor);

                    potentialPairs.Add(new Pair(intersection, l1, l2));
                }

            var filtered = potentialPairs.GroupBy(x => Math.Round(x.Intersection.Item2)).Select(y => y.First()).ToList(); // Get rid of multiples of the same pairs
            var sorted = filtered.OrderBy(x => x.Intersection.Item2).Reverse().ToList();
            sorted = UniqueFilter(sorted, height);

            var result = new List<Pair>();
            for (var i = 0; i < result.Count; i++)
            {
                result.Add(sorted[i]);
                Console.WriteLine("Pair: y = " + sorted[i].L1.A + "x + " + sorted[i].L1.B + " with y = " + sorted[i].l2.A + "x + " + sorted[i].l2.B + "With intersection (" + sorted[i].Intersection.Item1 + " , " + sorted[i].Intersection.Item2 + ")");
            }

            return sorted;
        }

        /// <summary>
        /// When two pairs share one line, remove the pair that is less likely to represent the other side of the pole.
        /// This is done by comparing the pairs with each other and keeping the one with less difference in the x-direction
        /// between the points at y = 0.
        /// </summary>
        private static List<Pair> UniqueFilter(IList<Pair> pairs, int height)
        {
            var illegals = new List<Pair>();
            for (var i = 0; i < pairs.Count; i++)
            {
                var diff1 = Math.Abs(GetX(pairs[i].L1) - GetX(pairs[i].l2));
                for (var j = 0; j < pairs.Count; j++)
                {
                    if (i == j)
                        continue;

                    var diff2 = Math.Abs(GetX(pairs[j].L1) - GetX(pairs[j].l2));

                    if (!Line.Equals(pairs[i].L1, pairs[j].L1) && !Line.Equals(pairs[i].L1, pairs[j].l2) && !Line.Equals(pairs[i].l2, pairs[j].L1) && !Line.Equals(pairs[i].l2, pairs[j].l2))
                        continue;

                    illegals.Add(diff1 < diff2 ? pairs[j] : pairs[i]);
                }
            }

            return pairs.Where((t1, i) => !illegals.Where((t, j) => i != j).Any(t => Pair.Equals(t1, t))).ToList();
        }

        /// <summary>
        /// Gets the x value of a line at y = 0
        /// </summary>
        private static double GetX(Line l)
        {
            var x0 = -((l.B - 0) / l.A);
            return x0;
        }

        /// <summary>
        /// Removes any pixel that isn't inside any of the pairs in the provided list.
        /// </summary>
        public static byte[,] CleanOutsidePairs(byte[,] image, List<Pair> pairs)
        {
            var result = new byte[image.GetLength(0), image.GetLength(1)];
            for (var x = 0; x < image.GetLength(0); x++)
            {
                for (var y = 0; y < image.GetLength(1); y++)
                {
                    var inBetween = false;
                    for (var i = 0; i < pairs.Count; i++)
                    {
                        var xl0 = -((pairs[i].L1.B - y) / pairs[i].L1.A);
                        var xl1 = -((pairs[i].l2.B - y) / pairs[i].l2.A);
                        if (x <= xl0 && x >= xl1)
                            inBetween = true;
                    }
                    if (inBetween)
                        result[x, y] = image[x, y];
                }
            }
            return result;
        }
    }
}
