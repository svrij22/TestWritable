using ILGPU;
using ILGPU.Runtime;
using ILGPU.Runtime.CPU;
using ILGPU.Runtime.Cuda;
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
using TestWritable.engine;
using TestWritable.structs;

namespace TestWritable
{
    internal class GPUScene
    {
        private WriteableBitmap writeableBitmap;
        public List<TracerObject> TracerObjects { get; set; } = new();
        public Vector3 Origin { get; set; }
        public GPUScene(WriteableBitmap writeableBitmap, double Width, double Height)
        {
            this.writeableBitmap = writeableBitmap;

            width = Width;
            height = Height;

            Context context = Context.Create(builder => builder.AllAccelerators());

            GPUTEST();
        }

        public void GPUTEST()
        {
            Test();
        }
        static void Kernel(Index1D pixelIndex, 
                           int _width, 
                           int _height, 
                           ArrayView<float> sphereData, 
                           ArrayView<int> output)
        {
            //Pixel indices
            int img_x = pixelIndex % _width;
            int img_y = pixelIndex / _width;

            //De-code spheres
            var amountOfSpheres = SphereStruct.AmountFromFloatArr(sphereData);
            var sphere = SphereStruct.SphereFromFloatArr(sphereData, 0);

            //Setup
            Vector3 Origin = new(0, 0, 0);
            double AspectRatio = _width / (float)_height;
            float ViewportHeight = 2.0f;
            double ViewportWidth = AspectRatio * ViewportHeight;
            float FocalLength = 1.0f;
            Vector3 horizontal = new Vector3((float)ViewportWidth, 0, 0);
            Vector3 vertical = new Vector3(0, ViewportHeight, 0);
            Vector3 lowerLeftCorner = Origin - horizontal / 2 - vertical / 2 - new Vector3(0, 0, FocalLength);

            //get u and v
            float u = (float)(img_x / (_width - 1));
            float v = (float)(img_y / (_height - 1));

            // Pointing each ray towards a point on the viewport
            var direction = lowerLeftCorner + Ext.MultiplyVectorByScalar(horizontal, u) + Ext.MultiplyVectorByScalar(vertical, v) - Origin;
            var ray = new RayStruct(Origin, Vector3.Normalize(direction));

            // Get color
            int color_data = GPUTracer.Trace(ray, sphereData);

            //Set output
            output[pixelIndex] = color_data;
        }

        //Width and height
        private readonly double width;
        private readonly double height;

        /// <summary>
        /// Test method for gpu acceleration
        /// </summary>
        public void Test()
        {
            // Initialize ILGPU.
            Context context = Context.CreateDefault();
            Accelerator accelerator = context.GetPreferredDevice(preferCPU: false)
                                      .CreateAccelerator(context);

            //
            // Input spheres
            //
            var sphere1 = new SphereStruct(new Vector3(-1.5f, 0, -2),   0.5f, .1f, .97f, 1f);
            var sphere2 = new SphereStruct(new Vector3(0, 0, -3),       0.5f, .1f, .2f,  1f);
            var sphere3 = new SphereStruct(new Vector3(-1.5f, 0, -2),   0.5f, .1f, .45f, 1f);
            List<float> spheres_floats = new();
            spheres_floats.AddRange(sphere1.ToFloatArr());
            spheres_floats.AddRange(sphere2.ToFloatArr());
            spheres_floats.AddRange(sphere3.ToFloatArr());
            if (spheres_floats.Count % sphere1.ToFloatArr().Count() != 0)
                throw new ArgumentException($"The array length must be a multiple of {sphere1.ToFloatArr().Count()}.");

            MemoryBuffer1D<float, Stride1D.Dense> sphereData = accelerator.Allocate1D<float>(spheres_floats.ToArray());

            //
            // Output pixels
            //
            int amountOfPixels = (int)(width * height);
            MemoryBuffer1D<int, Stride1D.Dense> pixelsOutput = accelerator.Allocate1D<int>(amountOfPixels);

            //
            // Load / Compile
            //
            Action<Index1D, int, int, ArrayView<float>, ArrayView<int>> loadedKernel = 
                accelerator.LoadAutoGroupedStreamKernel<Index1D, int, int, ArrayView<float>, ArrayView<int>>(Kernel);

            //
            // Compute
            //
            loadedKernel((int)pixelsOutput.Length, (int)width, (int)height, sphereData.View, pixelsOutput.View);

            // wait for the accelerator to be finished with whatever it's doing
            // in this case it just waits for the kernel to finish.
            accelerator.Synchronize();

            // moved output data from the GPU to the CPU for output to console
            int[] hostOutput = pixelsOutput.GetAsArray1D();
            var t = hostOutput.Where(v => v != -8874316);
            for (int i = 0; i < 50; i++)
            {
                Debug.Write(hostOutput[i]);
            }

            accelerator.Dispose();
            context.Dispose();
        }
    }
}
