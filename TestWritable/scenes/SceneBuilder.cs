using System;
using System.Collections.Generic;
using System.Drawing;
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
            List<float> objectStructs = new List<float>();

            // Input spheres
            var spheres = new[]
            {
                new SphereStruct(new Vector3(-2.5f, 0, 0), 0.5f, .1f, .45f, 1f, Color.Orange.R, Color.Orange.G, Color.Orange.B),
                new SphereStruct(new Vector3(-1.5f, 0, -2), 0.5f, .1f, .97f, 1f, Color.WhiteSmoke.R, Color.WhiteSmoke.G, Color.WhiteSmoke.B),
                new SphereStruct(new Vector3(0, 0, -3), 0.5f, .1f, .2f, 1f, Color.Pink.R, Color.Pink.G, Color.Pink.B),
                new SphereStruct(new Vector3(1.5f, 0, -2), 0.5f, .3f, 0f, .3f, Color.Green.R, Color.Green.G, Color.Green.B),
                new SphereStruct(new Vector3(2.5f, 0, 0), 0.5f, .1f, 0f, .3f, Color.Pink.R, Color.Pink.G, Color.Pink.B, true),

                new SphereStruct(new Vector3(0, -3, -2), 0.5f, 1f, 0f, 0f, Color.White.R, Color.White.G, Color.White.B)
            };
            foreach (var sphere in spheres) objectStructs.AddRange(StructExt.EncodeToBlock(sphere.Encode()));

            // Input plane
            var plane1 = new PlaneStruct(new Vector3(0, .5f, 0), new Vector3(0, -1, 0), .15f, .04f, .3f, Color.WhiteSmoke.R, Color.WhiteSmoke.G, Color.WhiteSmoke.B);
            objectStructs.AddRange(StructExt.EncodeToBlock(plane1.Encode()));

            // Input rectangles
            var rectangles = new[]
            {
                new RectangleStruct(new Vector3(-6, .5f, -5), new Vector3(-4, -2, -5), 12, 12, 12, .0f, .1f, .3f),
                new RectangleStruct(new Vector3(-4, .5f, -5), new Vector3(-2, -2, -5), 255, 255, 255, .0f, .1f, .3f),
                new RectangleStruct(new Vector3(-2, .5f, -5), new Vector3(0, -2, -5), 12, 12, 12, .0f, .1f, .3f),
                new RectangleStruct(new Vector3(0, .5f, -5), new Vector3(2, -2, -5), 255, 255, 255, .0f, .1f, .3f),
                new RectangleStruct(new Vector3(2, .5f, -5), new Vector3(4, -2, -5), 12, 12, 12, .0f, .1f, .3f),
                new RectangleStruct(new Vector3(4, .5f, -5), new Vector3(6, -2, -5), 255, 255, 255, .0f, .1f, .3f),

                //sides
                new RectangleStruct(new Vector3(-6, .5f, -5), new Vector3(-6, -2, 0), Color.DarkRed.R, Color.DarkRed.G, Color.DarkRed.B,                .1f, 0f, .3f),
                new RectangleStruct(new Vector3(6, .5f, -5), new Vector3(6, -2, 0), Color.DeepSkyBlue.R, Color.DeepSkyBlue.G, Color.DeepSkyBlue.B,         .1f, 0f, .3f)
            };
            foreach (var rectangle in rectangles) objectStructs.AddRange(StructExt.EncodeToBlock(rectangle.Encode()));

            return objectStructs.ToArray();
        }
    }
}
