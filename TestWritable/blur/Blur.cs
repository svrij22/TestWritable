using ILGPU;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestWritable.blur
{
    internal class Blur
    {

        public int[,] BoxBlur(int[,] colorData, int width, int height)
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

        public int[,] GaussianBlur(int[,] colorData, int width, int height)
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

        public int[,] CombineImages(int[,] original, int[,] blurred, int width, int height)
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


        /// <summary>
        /// Bright pixels kernel
        /// </summary>
        /// <param name="pixelIndex"></param>
        /// <param name="_width"></param>
        /// <param name="_height"></param>
        /// <param name="input"></param>
        /// <param name="output"></param>
        public static void GetBrightPixelsKernel(Index1D pixelIndex,
                                                 int _width,
                                                 int _height,
                                                 ArrayView<int> input,
                                                 ArrayView<float> output) {

            //Pixel indices
            float intensity = Ext.GetIntensity(input[pixelIndex]); // Define this function to extract intensity from your color.
            output[pixelIndex] = intensity;
        }
        public float[,] GetBrightPixels(int[,] colorData, int width, int height)
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
        public int[,] ExtractAndBlurBrightPixels(float[,] isBright, int[,] colorData, float threshold, int width, int height)
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
            int[,] blurredBrightPixels = GaussianBlur(brightPixels, width, height);
            return blurredBrightPixels;
        }
    }
}
