using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;

namespace TestWritable.renderer
{
    internal class BitmapWriter
    {
        public static int[] Flatten2DArray(int width, int height, int[,] colorData)
        {
            // Convert 2D array to 1D because WritePixels takes a 1D array
            int[] flatArray = new int[(int)(width * height)];
            for (int i = 0; i < height; i++)
            {
                for (int j = 0; j < width; j++)
                {
                    flatArray[(int)(i * width + j)] = colorData[j, i];
                }
            }
            return flatArray;
        }
        public static void Write(WriteableBitmap writeableBitmap, int width, int height, int[] flatColorData)
        {
            try
            {
                // Reserve the back buffer for updates.
                writeableBitmap.Lock();

                // Create an Int32Rect with the size of the whole bitmap
                Int32Rect rect = new Int32Rect(0, 0, (int)width, (int)height);

                // Write the 1D array of color data to the bitmap
                writeableBitmap.WritePixels(rect, flatColorData, (int)width * 4, 0);

            }
            finally
            {
                // Release the back buffer and make it available for display.
                writeableBitmap.Unlock();
            }
        }
    }
}
