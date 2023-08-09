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
using TestWritable.blur;
using TestWritable.engine;
using TestWritable.renderer;
using TestWritable.scenes;
using TestWritable.structs;

namespace TestWritable
{
    internal class GPURenderer
    {
        private WriteableBitmap writeableBitmap;
        public Vector3 Origin = new(0, 0, 4);
        public GPURenderer(WriteableBitmap writeableBitmap, double Width, double Height)
        {
            this.writeableBitmap = writeableBitmap;

            width = Width;
            height = Height;

            Initialize();
            Compile();
            Compute();
        }
        public static void RendererKernel(Index1D pixelIndex,
                           Vector3 origin,
                           int _width, 
                           int _height, 
                           ArrayView<float> structData, 
                           ArrayView<double> randData,
                           ArrayView<int> output)
        {
            //Pixel indices
            int img_x = pixelIndex % _width;
            int img_y = pixelIndex / _width;

            //Setup
            double AspectRatio = _width / (float)_height;
            float ViewportHeight = 2.0f;
            double ViewportWidth = AspectRatio * ViewportHeight;
            float FocalLength = 1.0f;
            Vector3 horizontal = new Vector3((float)ViewportWidth, 0, 0);
            Vector3 vertical = new Vector3(0, ViewportHeight, 0);
            Vector3 lowerLeftCorner = origin - horizontal / 2 - vertical / 2 - new Vector3(0, 0, FocalLength);

            //get u and v
            float u = (float)(img_x / (((float)_width) - 1));
            float v = (float)(img_y / (((float)_height) - 1));

            // Pointing each ray towards a point on the viewport
            var direction = lowerLeftCorner + Ext.MultiplyVectorByScalar(horizontal, u) + Ext.MultiplyVectorByScalar(vertical, v) - origin;
            var ray = new RayStruct(origin, Vector3.Normalize(direction));

            // Get color
            int color_data = GlassTracer.Trace(ray, structData, randData, pixelIndex);

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
            //context = Context.Create().Math(MathMode.Fast).Optimize(OptimizationLevel.O2).ToContext();
            //accelerator = context.GetPreferredDevice(preferCPU: true).CreateAccelerator(context);

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

        Action<Index1D, Vector3, int, int, ArrayView<float>, ArrayView<double>, ArrayView<int>> rendererKernel;
        Action<Index1D, int, int, ArrayView<int>, ArrayView<float>> brightPixelsKernel;

        MemoryBuffer1D<int, Stride1D.Dense> pixelsOutput;
        MemoryBuffer1D<int, Stride1D.Dense> brightPixelsOutput;
        MemoryBuffer1D<float, Stride1D.Dense> structData;
        MemoryBuffer1D<double, Stride1D.Dense> randData;

        public void ResetRandomData()
        {
            //Write random doubles
            Random random = new Random();
            double[] rnd = new double[2500];
            for (int i = 0; i < 2500; i++)
                rnd[i] = random.NextDouble();
            randData = accelerator.Allocate1D<double>(rnd.ToArray());
        }
        public void Compile()
        {
            var struct_floats = SceneBuilder.Scene1();
            structData = accelerator.Allocate1D<float>(struct_floats.ToArray());

            //
            // Output pixels
            //
            int amountOfPixels = (int)(width * height);
            PixelBuffer = new int[amountOfPixels];
            pixelsOutput = accelerator.Allocate1D<int>(amountOfPixels);
            brightPixelsOutput = accelerator.Allocate1D<int>(amountOfPixels);

            //
            // Load / Compile renderer kernel
            //
            rendererKernel = accelerator.LoadAutoGroupedStreamKernel<Index1D, Vector3, int, int, ArrayView<float>, ArrayView<double>, ArrayView<int>>(RendererKernel);

            //
            // Load / Compile bright pixels kernel
            //
            brightPixelsKernel = accelerator.LoadAutoGroupedStreamKernel<Index1D, int, int, ArrayView<int>, ArrayView<float>>(Blur.GetBrightPixelsKernel);
        }

        public int[] PixelBuffer = null;

        public int FramesComputed = 1;

        public bool ResetFlag = false;
        public void ResetPixelBuffer()
        {
            FramesComputed = 0;
            ResetFlag = true;
        }

        public void CombineWithPixelBuffer(int[] hostOutput)
        {
            if (ResetFlag)
            {
                FramesComputed = 0;
                PixelBuffer = hostOutput.ToArray();
                ResetFlag = false;
                return;
            }

            for (int i = 0; i < hostOutput.Length; i++)
            {
                if (hostOutput[i] != PixelBuffer[i])
                {
                    var nWeight = 1f / FramesComputed;
                    PixelBuffer[i] = Ext.MixColors(hostOutput[i], PixelBuffer[i], nWeight);
                }
            }
        }
        public long Compute()
        {
            //
            // Compute
            //

            // Start measuring time
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            // Write random doubles
            ResetRandomData();

            // Run kernel
            rendererKernel((int)pixelsOutput.Length, Origin, (int)width, (int)height, structData.View, randData.View, pixelsOutput.View);

            // Sync
            accelerator.Synchronize();

            // Write to buffer
            int[] hostOutput = pixelsOutput.GetAsArray1D();
            CombineWithPixelBuffer(hostOutput);
            FramesComputed++;

            // Run bright pixels kernel
            //brightPixelsKernel((int)pixelsOutput.Length, (int)width, (int)height, "", brightPixelsOutput.View);

            // Sync
            accelerator.Synchronize();

            //Write
            stopwatch.Stop();
            Debug.WriteLine($"Execution Time: {stopwatch.ElapsedMilliseconds} ms");

            return stopwatch.ElapsedMilliseconds;
        }

        public void WriteToBitmap()
        {
            if (PixelBuffer == null)
                return;

            //Write to bitmap
            BitmapWriter.Write(writeableBitmap, (int)width, (int)height, PixelBuffer);
        }
    }
}
