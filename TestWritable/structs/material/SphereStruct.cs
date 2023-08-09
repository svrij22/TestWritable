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
    public static class IndexConstants
    {
        public const int StructType = 0;

        public const int ColorR = 1;
        public const int ColorG = 2;
        public const int ColorB = 3;

        public const int Luminance = 4;
        public const int Reflectivity = 5;
        public const int Fresnel = 6;

        public const int CenterX = 7;
        public const int CenterY = 8;
        public const int CenterZ = 9;
        public const int Radius = 10;

        public const int IsGlass = 11;
    }
    public struct SphereStruct
    {

        public float Radius;
        public Vector3 Center;

        public float Luminance;
        public float Reflectivity;
        public float Fresnel;

        public ColorStruct Color;

        public bool IsGlass;

        public int structType;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="center"></param>
        /// <param name="radius"></param>
        /// <param name="color"></param>
        /// <param name="luminance"></param>
        /// <param name="reflectivity"></param>
        /// <param name="fresnel"></param>
        public SphereStruct(Vector3 center, 
                            float radius, 
                            float luminance = 1f, 
                            float reflectivity = 0f, 
                            float fresnel = 0f,
                            int _r = 255,
                            int _g = 255,
                            int _b = 255,
                            bool isGlass = false)
        {
            this.Radius = radius;
            this.Center = center;

            this.Luminance = luminance;
            this.Reflectivity = reflectivity;
            this.Fresnel = fresnel;

            this.Color = ColorStruct.FromRGB(_r, _g, _b);

            this.IsGlass = isGlass;
            this.structType = (int)StructType.Sphere;
        }

        /// <summary>
        /// Convert to FloatArray
        /// </summary>
        /// <returns></returns>
        /// 
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
                Radius,
                IsGlass ? 1 : 0,
            };
        }

        /// <summary>
        /// Convert back to sphere structs
        /// </summary>
        /// <param name="arr"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public static SphereStruct Decode(ArrayView<float> arr, int readFrom)
        {
            var sphere = new SphereStruct
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
                Radius = arr[readFrom + 10],
                IsGlass = arr[readFrom + 11] == 1 ? true : false,
            };

            return sphere;
        }

        /// <summary>
        /// Returns the normal for any given point in space
        /// </summary>
        /// <param name="point"></param>
        /// <returns></returns>
        /// <param name="r"></param>
        public Vector3 NormalAt(Vector3 point)
        {
            Vector3 normalVector = Vector3.Subtract(point, Center);
            normalVector = Vector3.Normalize(normalVector);
            return normalVector;
        }

        /// <summary>
        /// The Hit method in the Sphere class is used to check if a ray intersects the sphere between tMin and tMax. 
        /// It calculates the quadratic equation's coefficients to represent intersection points. Using the quadratic formula,
        /// it finds the discriminant and determines the number of solutions. If the discriminant is > 0, there are two intersections. 
        /// For each, it verifies if the t value lies within tMin and tMax. If so, t is set to this value and true is returned. 
        /// Otherwise, t is set to 0 and false is returned, indicating no intersection.
        /// </summary>
        /// <param name="r"></param>
        /// <param name="tMin"></param>
        /// <param name="tMax"></param>
        /// <param name="t"></param>
        /// <returns></returns>
        public bool Hit(RayStruct r, float tMin, float tMax, out float t)
        {
            var oc = r.Origin - Center;
            var a = Vector3.Dot(r.Direction, r.Direction);
            var b = 2.0f * Vector3.Dot(oc, r.Direction);
            var c = Vector3.Dot(oc, oc) - Radius * Radius;
            var discriminant = b * b - 4 * a * c;

            if (discriminant > 0)
            {
                var temp = (-b - MathF.Sqrt(discriminant)) / (2.0f * a);
                if (temp < tMax && temp > tMin)
                {
                    t = temp;
                    return true;
                }

                temp = (-b + MathF.Sqrt(discriminant)) / (2.0f * a);
                if (temp < tMax && temp > tMin)
                {
                    t = temp;
                    return true;
                }
            }

            t = 0;
            return false;
        }

        /// <summary>
        /// Random points
        /// </summary>
        public Vector3 GetRandomPoint(double rand1, double rand2)
        {
            return Center + Vector3.Normalize(new Vector3((float)rand1 - .5f,
                                                (float)rand2 - .5f,
                                                (float)rand1 - .5f));
        }
    }
}
