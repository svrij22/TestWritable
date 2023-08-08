using ILGPU;
using ILGPU.Runtime;
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
using TestWritable.structs;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.ProgressBar;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TrackBar;

namespace TestWritable
{
    internal class CPURenderer
    {
        private WriteableBitmap writeableBitmap;
        public Vector3 Origin { get; set; }
        public CPURenderer(WriteableBitmap writeableBitmap, double Width, double Height)
        {
            this.writeableBitmap = writeableBitmap;

            width = Width;
            height = Height;

            Context context = Context.Create(builder => builder.AllAccelerators());

            Run();
        }

        //Width and height
        private readonly double width;
        private readonly double height;

        /// <summary>
        /// Test method for gpu acceleration
        /// </summary>
        public void Run()
        {
            // Initialize ILGPU.
            Context context = Context.CreateDefault();
            Accelerator accelerator = context.GetPreferredDevice(preferCPU: true)
                                      .CreateAccelerator(context);

            //
            // Input spheres
            //
            var sphere1 = new SphereStruct(new Vector3(-1.5f, 0, -2),   0.5f, .1f,      .97f, 1f);
            var sphere2 = new SphereStruct(new Vector3(0, 0, -3),       0.5f, .1f,      .2f, 1f);
            var sphere3 = new SphereStruct(new Vector3(1.5f, 0, -2),   0.5f, .1f,      .45f, 1f);

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
            // Compute manually
            //
            for (int i = 0; i < pixelsOutput.Length; i++)
            {
                GPURenderer.RendererKernel(i, (int)width, (int)height, ((ArrayView<float>)sphereData), (ArrayView<int>)pixelsOutput);
            }

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

            //Dispose
            accelerator.Dispose();
            context.Dispose();
        }
    }
}
