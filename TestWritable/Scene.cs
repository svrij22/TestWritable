﻿using System;
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

            //Create scene
            var sphere1 = new Sphere(new Vector3(-1.5f, 0, -2), 0.5f, Color.Red);
            TracerObjects.Add(sphere1);

            var sphere2 = new Sphere(new Vector3(0, 0, -3), 0.5f, Color.Blue);
            TracerObjects.Add(sphere2);

            var sphere3 = new Sphere(new Vector3(1.5f, 0, -2), 0.5f, Color.Green);
            TracerObjects.Add(sphere3);

            TracerObject floor = new Plane(new Vector3(0, 1, 0), new Vector3(0, -1, 0), Color.Gray); // Assuming up is along y axis
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

                int taskCount = 12;
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
