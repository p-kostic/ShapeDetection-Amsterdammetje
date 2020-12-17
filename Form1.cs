using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace INFOIBV
{
    public partial class INFOIBV : Form
    {
        private Bitmap _inputImage;
        private Bitmap _inputImage2;
        private Bitmap _outputImage;

        public INFOIBV()
        {
            InitializeComponent();
        }

        private void LoadImageButton_Click(object sender, EventArgs e)
        {
            if (this.openImageDialog.ShowDialog() != DialogResult.OK) 
                return;
            var file = this.openImageDialog.FileName;
            this.imageFileName.Text = file;
            this._inputImage?.Dispose();
            this._inputImage = new Bitmap(file);

            if (this._inputImage.Size.Height <= 0 || this._inputImage.Size.Width <= 0 || this._inputImage.Size.Height > 512 || this._inputImage.Size.Width > 512)
                MessageBox.Show("Error in image dimensions (have to be > 0 and <= 512)");
            else
            {
                this.pictureBox1.Image = this._inputImage;
                Console.WriteLine("[Image selection]   Input image loaded with a size of " + this._inputImage.Size.Width + "x" + this._inputImage.Size.Height);
            }
        }

        private void applyButton_Click(object sender, EventArgs e)
        {
            if (this._inputImage == null) 
                return;
            this._outputImage?.Dispose();
            this._outputImage = new Bitmap(this._inputImage.Size.Width, this._inputImage.Size.Height);
            var image = new Color[this._inputImage.Size.Width, this._inputImage.Size.Height];
            var image2 = new Color[this._inputImage2.Size.Width, this._inputImage2.Size.Height];

            // Setup progress bar
            this.progressBar.Visible = true;
            this.progressBar.Minimum = 1;
            this.progressBar.Maximum = this._inputImage.Size.Width * this._inputImage.Size.Height;
            this.progressBar.Value = 1;
            this.progressBar.Step = 1;

            // Copy input Bitmap to array            
            for (var x = 0; x < this._inputImage.Size.Width; x++)
                for (var y = 0; y < this._inputImage.Size.Height; y++)
                    image[x, y] = this._inputImage.GetPixel(x, y);                           // Set pixel color in array at (x,y)

            // Copy input Bitmap to array for the control image
            for (var x = 0; x < this._inputImage2.Size.Width; x++)
                for (var y = 0; y < this._inputImage2.Size.Height; y++)
                    image2[x, y] = this._inputImage2.GetPixel(x, y);                         // Set pixel color in array at (x,y)

            //==================================================================================
            //================================= PIPELINE =======================================
            //==================================================================================
            //RESET VARS FROM PREVIOUS IMAGE:
            FloodFill.ObjectCount = 0;
            FloodFill.Objects = new List<PictureObject>();

            //===================== Preprocessing: Filtering, Segmentation =====================
            var greyscale = HelperFunctions.Greyscalize(image);
            HelperFunctions.GlobalMask = HelperFunctions.Greyscalize(image2);

            //===================== Edge Detection =============================================
            var detectSides = CannyEdge.CannyEdgeDetector(greyscale, 1, GlobalThreshold.OtsuThreshold(greyscale)); 

            //===================== Find sides of road poles using Hough transform =============
            var lines = HoughTransform.FoundLines(detectSides, 0, 0);                            
            var cartLines = HoughExtensions.GetAllCartesianLines(lines, detectSides);
            var pairs = HoughExtensions.GetPairs(cartLines, detectSides.GetLength(1));
            var roughPairs = HoughExtensions.GetRoughPairs(cartLines, detectSides.GetLength(1));

            //===================== Find lower and upper boundaries of poles ===================
            var workingImage = BilateralFilter.BilateralFilter2D(greyscale);
            workingImage = EdgeDetection.DetectEdges(HelperFunctions.Threshold(workingImage, GlobalThreshold.OtsuThreshold(workingImage)));
            detectSides = HoughExtensions.CleanOutsidePairs(workingImage, roughPairs);
            detectSides = FloodFill.MarkObjects(detectSides);
            detectSides = FloodFill.filterObjects(detectSides);                       

            //==================================================================================
            //==================================================================================
            //==================================================================================

            var nDV = ValueCounter(detectSides);
            this.label2.Text = "Number of distinct values: " + nDV;
            Console.ForegroundColor = ConsoleColor.Magenta;
            Console.WriteLine("[Done]              Final image with size " + detectSides.GetLength(0) + "x" + detectSides.GetLength(1) + " and " + nDV + " distinct Values");
            Console.ResetColor();

            var newImage = Colorize(detectSides);
            DrawBoundingBox(newImage);

            var outputResized = new Bitmap(this._outputImage, detectSides.GetLength(0), detectSides.GetLength(1));

            // Copy array to output Bitmap
            for (var x = 0; x < newImage.GetLength(0); x++)
                for (var y = 0; y < newImage.GetLength(1); y++)
                    outputResized.SetPixel(x, y, newImage[x, y]);                      // Set the pixel color at coordinate (x,y)

            var g = Graphics.FromImage(outputResized);

            DrawPairs(pairs, outputResized); // Draw pairs generated by hough transform
            FloodFill.DrawFinal(this._inputImage, pairs);

            pictureBox2.Image = this._inputImage; // Display output image - replace InputImage with OutputResized to see hough pairs and object map with bounding boxes
            progressBar.Visible = true;           // Hide progress bar
        }

        private static int ValueCounter(byte[,] input)
        {
            return input.Cast<byte>().Distinct().Count();
        }

        private void saveButton_Click(object sender, EventArgs e)
        {
            if (this._outputImage == null)
                return;
            if (this.saveImageDialog.ShowDialog() == DialogResult.OK)
                this._outputImage.Save(this.saveImageDialog.FileName);
        }

        private void button1_Click_1(object sender, EventArgs e)
        {
            if (this.openImageDialog.ShowDialog() != DialogResult.OK)
                return;

            var file = this.openImageDialog.FileName; // Get the file name
            this.imageFileName.Text = file;                // Show file name
            this._inputImage2?.Dispose();                   // Reset image
            this._inputImage2 = new Bitmap(file);           // Create new Bitmap from file
            if (this._inputImage2.Size.Height <= 0 || this._inputImage2.Size.Width <= 0 || this._inputImage2.Size.Height > 512 ||
                this._inputImage2.Size.Width > 512)         // Dimension check
                MessageBox.Show("Error in image dimensions (have to be > 0 and <= 512)");
            else
            {
                this.pictureBox3.Image = this._inputImage2; // Display input image
                Console.WriteLine("[Image selection]   Mask/2nd image loaded with a size of " + this._inputImage2.Size.Width + "x" + this._inputImage2.Size.Height);
            }
        }

        private static void DrawBoundingBox(Color[,] image)
        {
            foreach (var t in FloodFill.Objects)
            {
                // Horizontal Top
                for (var j = t.XMin; j < t.XMax; j++)
                    image[j, t.YMin] = Color.FromArgb(0, 255, 0);
                // Horizontal Bottom
                for (var j = t.XMin; j < t.XMax; j++)
                    image[j, t.YMax] = Color.FromArgb(0, 255, 0);
                // Vertical Left
                for (var j = t.YMin; j < t.YMax; j++)
                    image[t.XMin, j] = Color.FromArgb(0, 255, 0);

                // Vertical Right
                for (var j = t.YMin; j < t.YMax; j++)
                    image[t.XMax, j] = Color.FromArgb(0, 255, 0);
            }
        }

        private static Color[,] Colorize(byte[,] image)
        {
            var finalImage = new Color[image.GetLength(0), image.GetLength(1)];
            for (var x = 0; x < image.GetLength(0); x++)
                for (var y = 0; y < image.GetLength(1); y++)
                    finalImage[x, y] = Color.FromArgb(image[x, y], image[x, y], image[x, y]);
            return finalImage;
        }

        private List<FoundLines> DrawLines(List<FoundLines> lines, Graphics g, Bitmap outputImage)
        {
            var result = new List<FoundLines>();
            var height = outputImage.Height;
            var width = outputImage.Width;

            // During processing h_h is doubled so that -ve r values 
            var houghHeight = (int)(Math.Sqrt(2) * Math.Max(height, width)) / 2;

            // Find edge points and vote in array 
            double centerX = width / 2;
            double centerY = height / 2;

            foreach (var line in lines)
            {
                for (var y = 0; y < height; y++)
                {
                    var x = (int)((line.Rho - houghHeight - (y - centerY) * Math.Sin(line.Theta)) / Math.Cos(line.Theta) + centerX);
                    if (x >= width || x < 0)
                        continue;
                    outputImage.SetPixel(x, y, Color.Lime);
                    result.Add(line);
                }
            }

            return result;
        }

        private static void DrawPairs(IEnumerable<Pair> pairs, Bitmap outputImage)
        {
            foreach (var p in pairs)
            {
                DrawLine(p.L1, outputImage);
                DrawLine(p.l2, outputImage);
            }
        }

        private static void DrawLine(Line l, Bitmap outputImage)
        {
            var height = outputImage.Height;
            var width = outputImage.Width;

            for (var y = 0; y < height; y++)
            {
                var x = -((l.B - y) / l.A);
                if (x > 0 && x < width)
                    outputImage.SetPixel((int)x, y, l.C);
            }
        }
    }
}
