using ILGPU;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace TestWritable.structs.material
{
    public struct RectangleStruct // If TracerObject is a class, remove the inheritance here.
    {
        public Vector3 Center;
        public Vector3 Normal;

        public Vector3 Min;
        public Vector3 Max;

        public ColorStruct Color;

        public float Luminance;
        public float Reflectivity;
        public float Fresnel;

        public RectangleStruct(Vector3 pos1, 
                               Vector3 pos2, 
                               int _r = 255,
                               int _g = 255,
                               int _b = 255,
                               float luminance = 1f, 
                               float reflectivity = 0f, 
                               float fresnel = 0f)
        {
            this.Normal = Vector3.Normalize(Vector3.Cross(pos2 - pos1, new Vector3(0, 1, 0)));

            this.Center = (pos1 + pos2) / 2;
            this.Color = ColorStruct.FromRGB(_r, _g, _b);
            this.Luminance = luminance;
            this.Reflectivity = reflectivity;
            this.Fresnel = fresnel;

            Min = new Vector3(
                Math.Min(pos1.X, pos2.X),
                Math.Min(pos1.Y, pos2.Y),
                Math.Min(pos1.Z, pos2.Z)
            );
            Max = new Vector3(
                Math.Max(pos1.X, pos2.X),
                Math.Max(pos1.Y, pos2.Y),
                Math.Max(pos1.Z, pos2.Z)
            );
        }

        // Assuming TracerObject is not an interface, remove 'override' and adjust method signatures if necessary
        public Vector3 NormalAt(Vector3 point, RayStruct r)
        {
            if (Vector3.Dot(Normal, r.Direction) < 0)
            {
                return Normal;
            }
            else
            {
                return -Normal;
            }
        }

        public bool Hit(RayStruct r, float tMin, float tMax, out float t)
        {
            float denom = Vector3.Dot(Normal, r.Direction);
            if (MathF.Abs(denom) > 1e-6)
            {
                Vector3 oc = Center - r.Origin;
                t = Vector3.Dot(oc, Normal) / denom;

                if (t < tMax && t > tMin)
                {
                    Vector3 hitPoint = r.PointAtParameter(t);
                    hitPoint.X = (float)Math.Round(hitPoint.X * 1000) / 1000;
                    hitPoint.Y = (float)Math.Round(hitPoint.Y * 1000) / 1000;
                    hitPoint.Z = (float)Math.Round(hitPoint.Z * 1000) / 1000;

                    // Use precomputed Min and Max for hit detection
                    if (hitPoint.X >= Min.X && hitPoint.X <= Max.X &&
                        hitPoint.Y >= Min.Y && hitPoint.Y <= Max.Y &&
                        hitPoint.Z >= Min.Z && hitPoint.Z <= Max.Z)
                    {
                        return true;
                    }
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
            Center.X,
            Center.Y,
            Center.Z,

            Normal.X,
            Normal.Y,
            Normal.Z,

            Min.X,
            Min.Y,
            Min.Z,

            Max.X,
            Max.Y,
            Max.Z,

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
        /// Convert back to RectangleStruct
        /// </summary>
        /// <param name="arr"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public static RectangleStruct Decode(ArrayView<float> arr, int readFrom)
        {
            var rectangle = new RectangleStruct
            {
                Center = new Vector3
                {
                    X = arr[readFrom + 0],
                    Y = arr[readFrom + 1],
                    Z = arr[readFrom + 2]
                },

                Normal = new Vector3
                {
                    X = arr[readFrom + 3],
                    Y = arr[readFrom + 4],
                    Z = arr[readFrom + 5]
                },

                Min = new Vector3
                {
                    X = arr[readFrom + 6],
                    Y = arr[readFrom + 7],
                    Z = arr[readFrom + 8]
                },

                Max = new Vector3
                {
                    X = arr[readFrom + 9],
                    Y = arr[readFrom + 10],
                    Z = arr[readFrom + 11]
                },

                Color = ColorStruct.FromRGB((int)arr[readFrom + 12], (int)arr[readFrom + 13], (int)arr[readFrom + 14]),

                Luminance = arr[readFrom + 15],
                Reflectivity = arr[readFrom + 16],
                Fresnel = arr[readFrom + 17]
            };

            return rectangle;
        }
    }
}
