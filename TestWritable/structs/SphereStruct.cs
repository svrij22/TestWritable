using ILGPU;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace TestWritable.structs
{
    public struct SphereStruct
    {
        public float Luminance;
        public float Reflectivity;
        public float Fresnel;

        public float Radius;
        public Vector3 Center;

        public int R;
        public int G;
        public int B;

        public bool IsGlass;

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

            this.R = _r;
            this.G = _g;
            this.B = _b;

            this.IsGlass = isGlass;
        }

        /// <summary>
        /// Convert to FloatArray
        /// </summary>
        /// <returns></returns>
        public float[] ToFloatArr()
        {
            return new float[]
            {
                Luminance,
                Reflectivity,
                Fresnel,
                Radius,
                Center.X,
                Center.Y,
                Center.Z,
                R,
                G,
                B,
                IsGlass ? 1 : 0,
            };
        }
        public static int AmountFromFloatArr(ArrayView<float> arr)
        {
            return (int)(arr.Length / 11);
        }

        /// <summary>
        /// Convert back to sphere structs
        /// </summary>
        /// <param name="arr"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public static SphereStruct SphereFromFloatArr(ArrayView<float> arr, int index)
        {
            var sphere = new SphereStruct
            {
                Luminance = arr[index * 10],
                Reflectivity = arr[index * 10 + 1],
                Fresnel = arr[index * 10 + 2],
                Radius = arr[index * 10 + 3],
                Center = new Vector3
                {
                    X = arr[index * 10 + 4],
                    Y = arr[index * 10 + 5],
                    Z = arr[index * 10 + 6]
                },
                R = (int)arr[index * 10 + 7],
                G = (int)arr[index * 10 + 8],
                B = (int)arr[index * 10 + 9],
                IsGlass = arr[index * 10 + 10] == 1 ? true : false,
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
    }
}
