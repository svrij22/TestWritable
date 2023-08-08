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
using TestWritable.renderer;
using TestWritable.scenes;
using TestWritable.structs;

namespace TestWritable
{
    internal class GPURenderer
    {
        private WriteableBitmap writeableBitmap;

        public Vector3 Origin { get; set; }
        public GPURenderer(WriteableBitmap writeableBitmap, double Width, double Height)
        {
            this.writeableBitmap = writeableBitmap;

            width = Width;
            height = Height;

            Context context = Context.Create(builder => builder.AllAccelerators());

            Initialize();
            Run();
        }

        public static void RendererKernel(Index1D pixelIndex, 
                           int _width, 
                           int _height, 
                           ArrayView<float> sphereData, 
                           ArrayView<int> output)
        {
            //Pixel indices
            int img_x = pixelIndex % _width;
            int img_y = pixelIndex / _width;

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
            float u = (float)(img_x / (((float)_width) - 1));
            float v = (float)(img_y / (((float)_height) - 1));

            // Pointing each ray towards a point on the viewport
            var direction = lowerLeftCorner + Ext.MultiplyVectorByScalar(horizontal, u) + Ext.MultiplyVectorByScalar(vertical, v) - Origin;
            var ray = new RayStruct(Origin, Vector3.Normalize(direction));

            // Get color
            int color_data = Tracer.Trace(ray, sphereData);

            //Set output
            output[pixelIndex] = color_data;
        }

        //Width and height
        private readonly double width;
        private readonly double height;

        /// <summary>
        /// Test method for gpu acceleration
        /// </summary>
        /// 
        private Context context;
        private Accelerator accelerator;
        public void Initialize()
        {
            // Initialize ILGPU.
            context = Context.CreateDefault();
            accelerator = context.GetPreferredDevice(preferCPU: false)
                                      .CreateAccelerator(context);
        }

        public void Dispose()
        {
            //Dispose
            accelerator.Dispose();
            context.Dispose();
        }
        public void Run()
        {
            var scene1_floats = SceneBuilder.Scene1();
            MemoryBuffer1D<float, Stride1D.Dense> sphereData = accelerator.Allocate1D<float>(scene1_floats.ToArray());

            //
            // Output pixels
            //
            int amountOfPixels = (int)(width * height);
            MemoryBuffer1D<int, Stride1D.Dense> pixelsOutput = accelerator.Allocate1D<int>(amountOfPixels);

            //
            // Load / Compile
            //
            Action<Index1D, int, int, ArrayView<float>, ArrayView<int>> loadedKernel = 
                accelerator.LoadAutoGroupedStreamKernel<Index1D, int, int, ArrayView<float>, ArrayView<int>>(RendererKernel);

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

            //Write to bitmap
            BitmapWriter.Write(writeableBitmap, (int)width, (int)height, hostOutput);
        }
    }
}
