using ILGPU;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using static TestWritable.structs.StructExt;

namespace TestWritable.structs
{
    public struct PlaneStruct
    {
        public Vector3 Center;
        public Vector3 Normal;

        public float Luminance;
        public float Reflectivity;
        public float Fresnel;

        public ColorStruct Color;
        public int structType;

        public PlaneStruct(Vector3 center, 
                           Vector3 normal,
                           float luminance = 1f, 
                           float reflectivity = 0f, 
                           float fresnel = 0f,
                           int _r = 255,
                           int _g = 255,
                           int _b = 255)
        {
            this.Center = center;
            this.Normal = Vector3.Normalize(normal); // Ensure the normal is always normalized

            this.Luminance = luminance;
            this.Reflectivity = reflectivity;
            this.Fresnel = fresnel;

            this.Color = ColorStruct.FromRGB(_r, _g, _b);

            this.structType = (int)StructType.Plane;
        }

        /// <summary>
        /// Normal at method
        /// </summary>
        public Vector3 NormalAt(Vector3 point) 
        {
            return Normal;
        }

        /// <summary>
        /// Hit method
        /// </summary>
        public bool Hit(RayStruct r, float tMin, float tMax, out float t)
        {
            float denom = Vector3.Dot(Normal, r.Direction);
            if (System.MathF.Abs(denom) > 1e-6) // Ensure we're not dividing by zero (or close to it)
            {
                Vector3 oc = Center - r.Origin;
                t = Vector3.Dot(oc, Normal) / denom;

                if (t < tMax && t > tMin)
                {
                    return true;
                }
            }

            t = 0;
            return false;
        }

        /// <summary>
        /// Convert to FloatArray
        /// </summary>
        /// <returns></returns>
        public float[] Encode()
        {
            return new float[]
            {
                structType,
                Center.X,
                Center.Y,
                Center.Z,
                Normal.X,
                Normal.Y,
                Normal.Z,
                Color.R,
                Color.G,
                Color.B,
                Luminance,
                Reflectivity,
                Fresnel,
                -123123123 // use -123123123 as end key
            };
        }


        /// <summary>
        /// Convert back to plane struct
        /// </summary>
        /// <param name="arr"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public static PlaneStruct Decode(ArrayView<float> arr, int readFrom)
        {
            var plane = new PlaneStruct
            {
                structType = (int)arr[readFrom], // Read structType from the specified index
                Center = new Vector3
                {
                    X = arr[readFrom + 1],
                    Y = arr[readFrom + 2],
                    Z = arr[readFrom + 3]
                },
                Normal = new Vector3
                {
                    X = arr[readFrom + 4],
                    Y = arr[readFrom + 5],
                    Z = arr[readFrom + 6]
                },
                Color = ColorStruct.FromRGB((int)arr[readFrom + 7], (int)arr[readFrom + 8], (int)arr[readFrom + 9]),
                Luminance = arr[readFrom + 10],
                Reflectivity = arr[readFrom + 11],
                Fresnel = arr[readFrom + 12]
            };

            return plane;
        }
    }
}
