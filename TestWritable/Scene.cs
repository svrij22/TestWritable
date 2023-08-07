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

            //SPHERE 1
            var sphere1 = new Sphere(new Vector3(-1.5f, 0, -2), 0.5f, Color.Red,                            .1f, 1f, 1f);
            TracerObjects.Add(sphere1);

            //SPHERE 2
            var sphere2 = new Sphere(new Vector3(0, 0, -3), 0.5f, Color.Pink,                               .1f, .2f, 1f);
            TracerObjects.Add(sphere2);

            //SPHERE 3
            var sphere3 = new Sphere(new Vector3(1.5f, 0, -2), 0.5f, Color.Green,                           .1f, 0f, .3f);
            TracerObjects.Add(sphere3);

            //SPHERE BEHIND
            var sphere2_behind = new Sphere(new Vector3(2f, 0, -5), 0.5f, Color.Orange,                     .1f, .55f, 1f);
            TracerObjects.Add(sphere2_behind);


            //SIDE
            var side1 = new Rectangle(new Vector3(-6, 0, -5), new Vector3(-6, -2, 0), Color.DarkRed, .3f, 0);
            TracerObjects.Add(side1);

            //SIDE 2
            var side2 = new Rectangle(new Vector3(6, 0, -5), new Vector3(6, -2, 0), Color.CadetBlue, .3f, 0);
            TracerObjects.Add(side2);

            //BLUE 1
            var rectangle11 = new Rectangle(new Vector3(-6, 0, -5), new Vector3(-4, -2, -5), Color.White, .1f, .2f);
            TracerObjects.Add(rectangle11);

            //RED 1
            var rectangle3 = new Rectangle(new Vector3(-4, 0, -5), new Vector3(-2, -2, -5), Color.Black,    .1f, .2f);
            TracerObjects.Add(rectangle3);

            //BLUE 1
            var rectangle4 = new Rectangle(new Vector3(-2, 0, -5), new Vector3(0, -2, -5), Color.White,     .1f, .2f);
            TracerObjects.Add(rectangle4);

            //RED 1
            var rectangle = new Rectangle(new Vector3(0, 0, -5), new Vector3(2, -2, -5), Color.Black,       .1f, .2f);
            TracerObjects.Add(rectangle);

            //BLUE 1
            var rectangle2 = new Rectangle(new Vector3(2, 0, -5), new Vector3(4, -2, -5), Color.White,      .1f, .2f);
            TracerObjects.Add(rectangle2);

            //RED 1
            var rectangle22 = new Rectangle(new Vector3(4, 0, -5), new Vector3(6, -2, -5), Color.Black, .1f, .2f);
            TracerObjects.Add(rectangle22);

            //LIGHT SOURCE
            var light_source = new Sphere(new Vector3(0, -3, -2), 0.9f, Color.White, 1);
            TracerObjects.Add(light_source);

            TracerObject floor = new Plane(new Vector3(0, .5f, 0), new Vector3(0, -1, 0), Color.WhiteSmoke, luminance: .2f, reflectivity: .04f, fresnel: .2f); // Assuming up is along y axis
            TracerObjects.Add(floor);

            Origin = new Vector3(0, 0, 0);
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

                // Reserve the back buffer for updates.
                writeableBitmap.Lock();

                // Convert 2D array to 1D because WritePixels takes a 1D array
                int[] flatArray = new int[(int)(width * height)];
                for (int i = 0; i < height; i++)
                {
                    for (int j = 0; j < width; j++)
                    {
                        flatArray[(int)(i * width + j)] = colorData[j, i];
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
    }
}
