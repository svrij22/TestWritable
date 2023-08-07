using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.ProgressBar;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TrackBar;

namespace TestWritable
{
    internal class Scene
    {
        private WriteableBitmap writeableBitmap;
        private readonly double width;
        private readonly double height;

        public List<TracerObject> TracerObjects { get; set; } = new();
        public Vector3 Origin { get; set; }
        public Scene(WriteableBitmap writeableBitmap, double Width, double Height)
        {
            this.writeableBitmap = writeableBitmap;

            width = Width;
            height = Height;

            LoadScene1();
        }

        public void LoadScene1()
        {

            //SPHERE 0
            var sphere2_behind = new Sphere(new Vector3(-2.5f, 0, 0), 0.5f, Color.Orange, .1f, .45f, 1f);
            TracerObjects.Add(sphere2_behind);

            //SPHERE 1
            var sphere1 = new Sphere(new Vector3(-1.5f, 0, -2), 0.5f, Color.WhiteSmoke, .1f, .97f, 1f);
            TracerObjects.Add(sphere1);

            //SPHERE 2
            var sphere2 = new Sphere(new Vector3(0, 0, -3), 0.5f, Color.Pink, .1f, .2f, 1f);
            TracerObjects.Add(sphere2);

            //SPHERE 4
            var sphere3 = new Sphere(new Vector3(1.5f, 0, -2), 0.5f, Color.Green, .3f, 0f, .3f);
            TracerObjects.Add(sphere3);

            //SPHERE 5
            var sphere4 = new Sphere(new Vector3(2.5f, 0, 0), 0.5f, Color.Pink, .1f, 0f, .3f);
            sphere4.Material = TracerObject.MaterialType.Glass;
            TracerObjects.Add(sphere4);


            //SIDE
            var side1 = new Rectangle(new Vector3(-6, .5f, -5), new Vector3(-6, -2, 0), Color.DarkRed, .1f, 0f, .3f);
            TracerObjects.Add(side1);

            //SIDE
            var side2 = new Rectangle(new Vector3(6, .5f, -5), new Vector3(6, -2, 0), Color.DeepSkyBlue, .1f, 0f, .3f);
            TracerObjects.Add(side2);


            //BLUE 1
            var rectangle11 = new Rectangle(new Vector3(-6, .5f, -5), new Vector3(-4, -2, -5), Color.WhiteSmoke,   .0f, .1f, .3f);
            TracerObjects.Add(rectangle11);

            //RED 1
            var rectangle3 = new Rectangle(new Vector3(-4, .5f, -5), new Vector3(-2, -2, -5), Color.Black,    .0f, .1f, .3f);
            TracerObjects.Add(rectangle3);

            //BLUE 1
            var rectangle4 = new Rectangle(new Vector3(-2, .5f, -5), new Vector3(0, -2, -5), Color.WhiteSmoke,     .0f, .1f, .3f);
            TracerObjects.Add(rectangle4);

            //RED 1
            var rectangle = new Rectangle(new Vector3(0, .5f, -5), new Vector3(2, -2, -5), Color.Black,       .0f, .1f, .3f);
            TracerObjects.Add(rectangle);

            //BLUE 1
            var rectangle2 = new Rectangle(new Vector3(2, .5f, -5), new Vector3(4, -2, -5), Color.WhiteSmoke,      .0f, .1f, .3f);
            TracerObjects.Add(rectangle2);

            //RED 1
            var rectangle22 = new Rectangle(new Vector3(4, .5f, -5), new Vector3(6, -2, -5), Color.Black,     .0f, .1f, .3f);
            TracerObjects.Add(rectangle22);


            //LIGHT SOURCE
            var light_source = new Sphere(new Vector3(0, -3, -2), 1f, Color.White, 1);
            TracerObjects.Add(light_source);

            TracerObject floor = new Plane(new Vector3(0, .5f, 0), new Vector3(0, -1, 0), Color.WhiteSmoke, luminance: .15f, reflectivity: .04f, fresnel: .3f); // Assuming up is along y axis
            TracerObjects.Add(floor);

            Origin = new Vector3(0, 0, 4);
        }

        /// <summary>
        /// View settings
        /// </summary>
        public double AspectRatio => width / (float)height;
        public float ViewportHeight = 2.0f;
        public double ViewportWidth => AspectRatio * ViewportHeight;
        public float FocalLength = 1.0f;
        public Vector3 horizontal => new Vector3((float)ViewportWidth, 0, 0);
        public Vector3 vertical => new Vector3(0, ViewportHeight, 0);
        public Vector3 lowerLeftCorner => Origin - horizontal / 2 - vertical / 2 - new Vector3(0, 0, FocalLength);

        /// <summary>
        /// Trace single point
        /// </summary>
        /// <param name="column"></param>
        /// <param name="row"></param>
        internal void TracePoint(int column, int row)
        {
            //get u and v
            float u = (float)(column / (width - 1));
            float v = (float)(row / (height - 1));

            // Pointing each ray towards a point on the viewport
            var direction = lowerLeftCorner + Ext.MultiplyVectorByScalar(horizontal, u) + Ext.MultiplyVectorByScalar(vertical, v) - Origin;
            var ray = new Ray(Origin, Vector3.Normalize(direction));

            // Get color
            int color_data = RayTracer.Trace(ray, TracerObjects);
        }

        // The DrawPixel method updates the WriteableBitmap by using
        // unsafe code to write a pixel into the back buffer.
        public void Draw()
        {
            try
            {

                // Create a 2D array to hold color data
                int[,] colorData = DoRaytrace();
                float[,] brightPixels = GetBrightPixels(colorData);
                int[,] blurredBrightPixels = ExtractAndBlurBrightPixels(brightPixels, colorData, .96f);
                int[,] combined = CombineImages(blurredBrightPixels, colorData);

                // Reserve the back buffer for updates.
                writeableBitmap.Lock();

                // Convert 2D array to 1D because WritePixels takes a 1D array
                int[] flatArray = new int[(int)(width * height)];
                for (int i = 0; i < height; i++)
                {
                    for (int j = 0; j < width; j++)
                    {
                        flatArray[(int)(i * width + j)] = combined[j, i];
                    }
                }

                // Create an Int32Rect with the size of the whole bitmap
                Int32Rect rect = new Int32Rect(0, 0, (int)width, (int)height);

                // Write the 1D array of color data to the bitmap
                writeableBitmap.WritePixels(rect, flatArray, (int)width * 4, 0);

            }
            finally
            {
                // Release the back buffer and make it available for display.
                writeableBitmap.Unlock();
            }
        }

        /// <summary>
        /// Raytraces the scene
        /// </summary>
        /// <returns></returns>
        public int[,] DoRaytrace()
        {
            int[,] colorData = new int[(int)width, (int)height];
            List<Task> tasks = new List<Task>();

            int taskCount = 24;
            int rowsPerTask = (int)(height / taskCount);

            for (int taskNumber = 0; taskNumber < taskCount; taskNumber++)
            {
                // Determine the start and end rows for this task
                int startRow = taskNumber * rowsPerTask;
                int endRow = (int)(taskNumber == taskCount - 1
                    ? height :
                    startRow + rowsPerTask); // Make sure the last task handles any remaining rows

                tasks.Add(Task.Run(() =>
                {
                    for (int col = 0; col < width; col++)
                    {
                        for (int row = startRow; row < endRow; row++)
                        {
                            //get u and v
                            float u = (float)(col / (width - 1));
                            float v = (float)(row / (height - 1));

                            // Pointing each ray towards a point on the viewport
                            var direction = lowerLeftCorner + Ext.MultiplyVectorByScalar(horizontal, u) + Ext.MultiplyVectorByScalar(vertical, v) - Origin;
                            var ray = new Ray(Origin, Vector3.Normalize(direction));

                            // Get color
                            int color_data = RayTracer.Trace(ray, TracerObjects);

                            // set col in arr
                            colorData[col, row] = color_data;
                        }
                    }
                }));
            }

            // Wait for all tasks to complete
            Task.WaitAll(tasks.ToArray());

            return colorData;
        }

        public float[,] GetBrightPixels(int[,] colorData)
        {
            // Corona effect (bloom)
            float[,] isBright = new float[(int)width, (int)height];
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    float intensity = Ext.GetIntensity(colorData[x, y]); // Define this function to extract intensity from your color.
                    isBright[x, y] = intensity;
                }
            }

            return isBright;
        }
        public int[,] ExtractAndBlurBrightPixels(float[,] isBright, int[,] colorData, float threshold)
        {
            int[,] brightPixels = new int[(int)width, (int)height];

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    if (isBright[x, y] > threshold)
                    {
                        brightPixels[x, y] = colorData[x, y];
                    }
                    else
                    {
                        brightPixels[x, y] = 0; // make it black
                    }
                }
            }

            // Now you apply your blur on the brightPixels, the result is the blurred bright regions
            int[,] blurredBrightPixels = GaussianBlur(brightPixels);

            return blurredBrightPixels;
        }

        public int[,] CombineImages(int[,] original, int[,] blurred)
        {
            int[,] combined = new int[(int)width, (int)height];

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    // Here, you're essentially adding the RGB values of the two images
                    // Extract RGB components from original
                    int r1 = (original[x, y] >> 16) & 0xFF;
                    int g1 = (original[x, y] >> 8) & 0xFF;
                    int b1 = original[x, y] & 0xFF;

                    // Extract RGB components from blurred
                    int r2 = (blurred[x, y] >> 16) & 0xFF;
                    int g2 = (blurred[x, y] >> 8) & 0xFF;
                    int b2 = blurred[x, y] & 0xFF;

                    // Add them up, ensuring they don't exceed 255
                    int finalR = Math.Min(r1 + r2, 255);
                    int finalG = Math.Min(g1 + g2, 255);
                    int finalB = Math.Min(b1 + b2, 255);

                    combined[x, y] = Ext.RGBToColorInt(finalR, finalG, finalB);
                }
            }

            return combined;
        }

        public int[,] BoxBlur(int[,] colorData)
        {
            int blurSize = 9;  // Adjust this based on your requirements.
            int[,] blurredData = new int[(int)width, (int)height];

            int taskCount = 24;
            int rowsPerTask = (int)(height / taskCount);

            List<Task> tasks = new List<Task>();
            for (int taskNumber = 0; taskNumber < taskCount; taskNumber++)
            {
                int startRow = taskNumber * rowsPerTask;
                int endRow = (int)(taskNumber == taskCount - 1
                    ? height :
                    startRow + rowsPerTask); // Make sure the last task handles any remaining rows

                tasks.Add(Task.Run(() =>
                {
                    for (int x = 0; x < width; x++)
                    {
                        for (int y = startRow; y < endRow; y++)
                        {
                            int sumR = 0;
                            int sumG = 0;
                            int sumB = 0;
                            int count = 0;

                            for (int dx = -blurSize; dx <= blurSize; dx++)
                            {
                                for (int dy = -blurSize; dy <= blurSize; dy++)
                                {
                                    int nx = x + dx;
                                    int ny = y + dy;
                                    if (nx >= 0 && nx < width && ny >= 0 && ny < height)
                                    {
                                        int pixel = colorData[nx, ny];
                                        sumR += (pixel >> 16) & 0xFF;
                                        sumG += (pixel >> 8) & 0xFF;
                                        sumB += pixel & 0xFF;
                                        count++;
                                    }
                                }
                            }

                            int avgR = sumR / count;
                            int avgG = sumG / count;
                            int avgB = sumB / count;

                            blurredData[x, y] = Ext.RGBToColorInt(avgR, avgG, avgB);
                        }
                    }
                }));
            }
            Task.WaitAll(tasks.ToArray());

            return blurredData;
        }

        public int[,] GaussianBlur(int[,] colorData)
        {
            // Example 3x3 kernel, you might want to use a bigger one or compute it dynamically
            float[,] kernel = GenerateGaussianKernel(15, 8);

            int kernelSize = 15; // Adjust this if you use a larger kernel
            int[,] blurredData = new int[(int)width, (int)height];

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    float sumR = 0;
                    float sumG = 0;
                    float sumB = 0;

                    for (int dx = -kernelSize / 2; dx <= kernelSize / 2; dx++)
                    {
                        for (int dy = -kernelSize / 2; dy <= kernelSize / 2; dy++)
                        {
                            int nx = x + dx;
                            int ny = y + dy;

                            if (nx >= 0 && nx < width && ny >= 0 && ny < height)
                            {
                                float weight = kernel[dx + kernelSize / 2, dy + kernelSize / 2];
                                int pixel = colorData[nx, ny];
                                sumR += ((pixel >> 16) & 0xFF) * weight;
                                sumG += ((pixel >> 8) & 0xFF) * weight;
                                sumB += (pixel & 0xFF) * weight;
                            }
                        }
                    }

                    int finalR = (int)Math.Round(sumR);
                    int finalG = (int)Math.Round(sumG);
                    int finalB = (int)Math.Round(sumB);
                    blurredData[x, y] = Ext.RGBToColorInt(finalR, finalG, finalB);
                }
            }

            return blurredData;
        }

        public float[,] GenerateGaussianKernel(int size = 9, double sigma = 1.0)
        {
            if (size % 2 == 0)
            {
                throw new ArgumentException("Size should be odd for a symmetric kernel.", nameof(size));
            }

            float[,] kernel = new float[size, size];
            double sum = 0.0;

            // Fill the kernel with values from the Gaussian function
            for (int x = 0; x < size; x++)
            {
                for (int y = 0; y < size; y++)
                {
                    double xDist = x - size / 2;
                    double yDist = y - size / 2;
                    double value = (1.0 / (2.0 * Math.PI * sigma * sigma)) *
                                   Math.Exp(-(xDist * xDist + yDist * yDist) / (2.0 * sigma * sigma));
                    kernel[x, y] = (float)value;
                    sum += value;
                }
            }

            // Normalize the kernel
            for (int x = 0; x < size; x++)
            {
                for (int y = 0; y < size; y++)
                {
                    kernel[x, y] /= (float)sum;
                }
            }

            return kernel;
        }
    }
}
