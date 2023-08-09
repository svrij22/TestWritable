using ILGPU;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using static TestWritable.structs.StructExt;

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

        private int structType;

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
            this.structType = (int)StructType.Rectangle;
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
                    hitPoint.X = MathF.Round(hitPoint.X * 1000) / 1000f;
                    hitPoint.Y = MathF.Round(hitPoint.Y * 1000) / 1000f;
                    hitPoint.Z = MathF.Round(hitPoint.Z * 1000) / 1000f;

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
                structType,

                Color.R,
                Color.G,
                Color.B,

                Luminance,
                Reflectivity,
                Fresnel,

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
                structType = (int)arr[readFrom + IndexConstants.StructType],
                Color = ColorStruct.FromRGB((int)arr[readFrom + IndexConstants.ColorR],
                                            (int)arr[readFrom + IndexConstants.ColorG],
                                            (int)arr[readFrom + IndexConstants.ColorB]),
                Luminance = arr[readFrom + IndexConstants.Luminance],
                Reflectivity = arr[readFrom + IndexConstants.Reflectivity],
                Fresnel = arr[readFrom + IndexConstants.Fresnel],
                Center = new Vector3
                {
                    X = arr[readFrom + IndexConstants.CenterX],
                    Y = arr[readFrom + IndexConstants.CenterY],
                    Z = arr[readFrom + IndexConstants.CenterZ]
                },
                Normal = new Vector3
                {
                    X = arr[readFrom + 10],
                    Y = arr[readFrom + 11],
                    Z = arr[readFrom + 12]
                },

                Min = new Vector3
                {
                    X = arr[readFrom + 13],
                    Y = arr[readFrom + 14],
                    Z = arr[readFrom + 15]
                },

                Max = new Vector3
                {
                    X = arr[readFrom + 16],
                    Y = arr[readFrom + 17],
                    Z = arr[readFrom + 18]
                },
            };

            return rectangle;
        }
    }
}
