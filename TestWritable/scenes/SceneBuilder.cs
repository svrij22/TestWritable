using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using TestWritable.structs;
using TestWritable.structs.material;

namespace TestWritable.scenes
{
    internal class SceneBuilder
    {
        public static float[] Scene1()
        {
            List<float> objectStructs = new();

            //
            // Input spheres
            //
            var sphere1 = new SphereStruct(new Vector3(-1.5f, 0, -2),           0.5f,   .1f, 1f, 1f,                255, 0, 0);
            var sphere2 = new SphereStruct(new Vector3(0, 0, -3),               0.5f,   .1f, .2f, 1f,                 0, 255, 0);
            var sphere3 = new SphereStruct(new Vector3(1.5f, 0, -2),            0.5f,   .1f, 0f, 1f,                0, 0, 255);
            objectStructs.AddRange(sphere1.Encode());
            objectStructs.AddRange(sphere2.Encode());
            objectStructs.AddRange(sphere3.Encode());

            var light_source = new SphereStruct(new Vector3(0f, -2.5f, -2),     0.5f,   1f, 0f, 0f,                255, 255, 255);
            objectStructs.AddRange(light_source.Encode());

            var plane1 = new PlaneStruct(new Vector3(0, 1, 0), new Vector3(0, -1, 0), .1f, 0f, 1f, 125, 125, 125);
            objectStructs.AddRange(plane1.Encode());
            //
            // rects back
            //

            var rectangle11 = new RectangleStruct(new Vector3(-6, .5f, -5), new Vector3(-4, -2, -5),    12, 12, 12,         .0f, .1f, .3f);
            var rectangle3 = new RectangleStruct(new Vector3(-4, .5f, -5),  new Vector3(-2, -2, -5),     255, 255, 255,        .0f, .1f, .3f);
            var rectangle4 = new RectangleStruct(new Vector3(-2, .5f, -5),  new Vector3(0, -2, -5),      12, 12, 12,         .0f, .1f, .3f);
            var rectangle = new RectangleStruct(new Vector3(0, .5f, -5),    new Vector3(2, -2, -5),        255, 255, 255,        .0f, .1f, .3f);
            var rectangle2 = new RectangleStruct(new Vector3(2, .5f, -5),   new Vector3(4, -2, -5),       12, 12, 12,         .0f, .1f, .3f);
            var rectangle22 = new RectangleStruct(new Vector3(4, .5f, -5),  new Vector3(6, -2, -5),      255, 255, 255,        .0f, .1f, .3f);

            objectStructs.AddRange(rectangle11.Encode());
            objectStructs.AddRange(rectangle3.Encode());
            objectStructs.AddRange(rectangle4.Encode());
            objectStructs.AddRange(rectangle.Encode());
            objectStructs.AddRange(rectangle2.Encode());
            objectStructs.AddRange(rectangle22.Encode());

            return objectStructs.ToArray();
        }
    }
}
