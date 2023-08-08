using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using TestWritable.structs;

namespace TestWritable.scenes
{
    internal class SceneBuilder
    {
        public static float[] Scene1()
        {
            //
            // Input spheres
            //
            var sphere1 = new SphereStruct(new Vector3(-1.5f, 0, -2),           0.5f, .1f, .97f, 1f,                255, 0, 0);
            var sphere2 = new SphereStruct(new Vector3(0, 0, -3),               0.5f, .1f, .2f, 1f,                 0, 255, 0);
            var sphere3 = new SphereStruct(new Vector3(1.5f, 0, -2),            0.5f, .1f, 0f, 1f,                0, 0, 255);
            var light_source = new SphereStruct(new Vector3(0f, -2.5f, -2),     0.5f, .1f, .45f, 1f,                255, 255, 255);

            List<float> spheres_floats = new();
            spheres_floats.AddRange(sphere1.ToFloatArr());
            spheres_floats.AddRange(sphere2.ToFloatArr());
            spheres_floats.AddRange(sphere3.ToFloatArr());
            spheres_floats.AddRange(light_source.ToFloatArr());

            if (spheres_floats.Count % sphere1.ToFloatArr().Count() != 0)
                throw new ArgumentException($"The array length must be a multiple of {sphere1.ToFloatArr().Count()}.");

            return spheres_floats.ToArray();
        }
    }
}
