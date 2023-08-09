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
            var sphere1 = new SphereStruct(new Vector3(-1.5f, 0, -2),           0.5f,   .1f, 1f, 1f,                255, 0, 0);
            var sphere2 = new SphereStruct(new Vector3(0, 0, -3),               0.5f,   .1f, .2f, 1f,                 0, 255, 0);
            var sphere3 = new SphereStruct(new Vector3(1.5f, 0, -2),            0.5f,   .1f, 0f, 1f,                0, 0, 255);
            var light_source = new SphereStruct(new Vector3(0f, -2.5f, -2),     0.5f,   1f, 0f, 0f,                255, 255, 255);

            var plane1 = new PlaneStruct(new Vector3(0, 1, 0), new Vector3(0, -1, 0),   .1f, 0f, 1f,                125, 125, 125);

            List<float> spheres_floats = new();
            spheres_floats.AddRange(sphere1.Encode());
            spheres_floats.AddRange(sphere2.Encode());
            spheres_floats.AddRange(sphere3.Encode());
            spheres_floats.AddRange(plane1.Encode());
            spheres_floats.AddRange(light_source.Encode());

            return spheres_floats.ToArray();
        }
    }
}
