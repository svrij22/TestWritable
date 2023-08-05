using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;

namespace TestWritable
{
    internal class Scene
    {
        private WriteableBitmap writeableBitmap;
        private readonly int width;
        private readonly int height;

        public List<TracerObject> TracerObjects { get; set; } = new();
        public Vector3 Origin { get; set; }
        public Scene(WriteableBitmap writeableBitmap, int Width, int Height)
        {
            this.writeableBitmap = writeableBitmap;

            width = Width;
            height = Height;

            //Create scene
            //var sphere1 = new Sphere(new Vector3(-1, 0, -3), 0.5f, Color.Red);
            //TracerObjects.Add(sphere1);

            var sphere2 = new Sphere(new Vector3(0, 0, -1), 0.5f, Color.Blue);
            TracerObjects.Add(sphere2);

            //var sphere3 = new Sphere(new Vector3(1, 0, -3), 0.5f, Color.Green);
            //TracerObjects.Add(sphere3);

            Origin = new Vector3(0, 0, 0);
        }

        // The DrawPixel method updates the WriteableBitmap by using
        // unsafe code to write a pixel into the back buffer.
        public void Draw()
        {
            try
            {
                // Reserve the back buffer for updates.
                writeableBitmap.Lock();

                // Settins
                var aspectRatio = width / (float)height;
                var viewportHeight = 2.0f;
                var viewportWidth = aspectRatio * viewportHeight;
                var focalLength = 1.0f;

                // Top left corner of the viewport
                var horizontal = new Vector3((float)viewportWidth, 0, 0);
                var vertical = new Vector3(0, viewportHeight, 0);
                var lowerLeftCorner = Origin - horizontal / 2 - vertical / 2 - new Vector3(0, 0, focalLength);

                // All pixels
                for (int col = 0; col < width; col++)
                {
                    for (int row = 0; row < height; row++)
                    {
                        unsafe
                        {
                            //
                            // Pointer stuff
                            //
                            // Get a pointer to the back buffer.
                            IntPtr pBackBuffer = writeableBitmap.BackBuffer;

                            // Find the address of the pixel to draw.
                            pBackBuffer += row * writeableBitmap.BackBufferStride;
                            pBackBuffer += col * 4;

                            //
                            // Ray tracing
                            // 

                            float u = (float)(col / (width - 1));
                            float v = (float)(row / (height - 1));

                            // Pointing each ray towards a point on the viewport
                            var direction = lowerLeftCorner + Ext.MultiplyVectorByScalar(horizontal, u) + Ext.MultiplyVectorByScalar(vertical, v) - Origin;
                            var ray = new Ray(Origin, Vector3.Normalize(direction));

                            // Get color
                            int color_data = Tracer.Trace(ray, TracerObjects);

                            // Assign the color data to the pixel.
                            *((int*)pBackBuffer) = color_data;
                        }

                        // Specify the area of the bitmap that changed.
                        writeableBitmap.AddDirtyRect(new Int32Rect(col, row, 1, 1));
                    }
                }
            }
            finally
            {
                // Release the back buffer and make it available for display.
                writeableBitmap.Unlock();
            }
        }
    }
}
