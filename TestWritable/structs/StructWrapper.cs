using ILGPU;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using TestWritable.structs.material;
using static TestWritable.structs.StructExt;

namespace TestWritable.structs
{
    public struct StructWrapper
    {
        public ArrayView<float> floatData;
        private readonly int readFrom;
        public StructWrapper(ArrayView<float> floatData, int readFrom)
        {
            this.floatData = floatData;
            this.readFrom = readFrom;
        }
        public bool IsGlass()
        {
            if (floatData[readFrom] == (float)StructType.Sphere) return SphereStruct.Decode(floatData, readFrom).IsGlass;
            return false;
        }
        public float GetLuminance()
        {
            return floatData[readFrom + IndexConstants.Luminance];
        }
        public float GetFresnel()
        {
            return floatData[readFrom + IndexConstants.Fresnel];
        }
        public float GetReflectivity()
        {
            return floatData[readFrom + IndexConstants.Reflectivity];
        }
        public ColorStruct GetColor()
        {
            int r = (int)floatData[readFrom + IndexConstants.ColorR];
            int g = (int)floatData[readFrom + IndexConstants.ColorG];
            int b = (int)floatData[readFrom + IndexConstants.ColorB];
            return ColorStruct.FromRGB(r, g, b);
        }
        public Vector3 NormalAt(Vector3 point, RayStruct ray)
        {
            if (floatData[readFrom] == (float)StructType.Sphere) return SphereStruct.Decode(floatData, readFrom).NormalAt(point);
            if (floatData[readFrom] == (float)StructType.Plane) return PlaneStruct.Decode(floatData, readFrom).NormalAt(point);
            if (floatData[readFrom] == (float)StructType.Rectangle) return RectangleStruct.Decode(floatData, readFrom).NormalAt(point, ray);
            return new Vector3();
        }
        public bool Hit(RayStruct r, float tMin, float tMax, out float t)
        {
            if (floatData[readFrom] == (float)StructType.Sphere) return SphereStruct.Decode(floatData, readFrom).Hit(r, tMin, tMax, out t);
            if (floatData[readFrom] == (float)StructType.Plane) return PlaneStruct.Decode(floatData, readFrom).Hit(r, tMin, tMax, out t);
            if (floatData[readFrom] == (float)StructType.Rectangle) return RectangleStruct.Decode(floatData, readFrom).Hit(r, tMin, tMax, out t);
            t = 0;
            return false;
        }
        internal Vector3 GetCenter()
        {
            if (floatData[readFrom] == (float)StructType.Sphere) return SphereStruct.Decode(floatData, readFrom).Center;
            if (floatData[readFrom] == (float)StructType.Plane) return PlaneStruct.Decode(floatData, readFrom).Center;
            if (floatData[readFrom] == (float)StructType.Rectangle) return PlaneStruct.Decode(floatData, readFrom).Center;
            return new();
        }
    }
}
