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
            List<float> objectStructs = new List<float>();

            // Input spheres
            var spheres = new[]
            {
                new SphereStruct(new Vector3(-1.5f, 0, -2), 0.5f, .1f, 1f, 1f, 255, 0, 0),
                new SphereStruct(new Vector3(0, 0, -3), 0.5f, .1f, .2f, 1f, 0, 255, 0),
                new SphereStruct(new Vector3(1.5f, 0, -2), 0.5f, .1f, 0f, 1f, 0, 0, 255),
                new SphereStruct(new Vector3(0f, -2.5f, -2), 0.5f, 1f, 0f, 0f, 255, 255, 255)
            };
            foreach (var sphere in spheres) objectStructs.AddRange(StructExt.EncodeToBlock(sphere.Encode()));

            // Input plane
            var plane1 = new PlaneStruct(new Vector3(0, 1, 0), new Vector3(0, -1, 0), .1f, 0f, 1f, 125, 125, 125);
            objectStructs.AddRange(StructExt.EncodeToBlock(plane1.Encode()));

            // Input rectangles
            var rectangles = new[]
            {
                new RectangleStruct(new Vector3(-6, .5f, -5), new Vector3(-4, -2, -5), 12, 12, 12, .0f, .1f, .3f),
                new RectangleStruct(new Vector3(-4, .5f, -5), new Vector3(-2, -2, -5), 255, 255, 255, .0f, .1f, .3f),
                new RectangleStruct(new Vector3(-2, .5f, -5), new Vector3(0, -2, -5), 12, 12, 12, .0f, .1f, .3f),
                new RectangleStruct(new Vector3(0, .5f, -5), new Vector3(2, -2, -5), 255, 255, 255, .0f, .1f, .3f),
                new RectangleStruct(new Vector3(2, .5f, -5), new Vector3(4, -2, -5), 12, 12, 12, .0f, .1f, .3f),
                new RectangleStruct(new Vector3(4, .5f, -5), new Vector3(6, -2, -5), 255, 255, 255, .0f, .1f, .3f)
            };
            foreach (var rectangle in rectangles) objectStructs.AddRange(StructExt.EncodeToBlock(rectangle.Encode()));

            return objectStructs.ToArray();
        }
    }
}
