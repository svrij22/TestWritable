using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace TestWritable
{
    public class Sphere : TracerObject
    {
        public float Radius { get; private set; }
        public Sphere(Vector3 center, float radius, Color color, double luminance = 1d)
        {
            this.Center = center;
            this.Radius = radius;
            this.Color = color;
            this.Luminance = luminance;
        }

        /// <summary>
        /// Returns the normal for any given point in space
        /// </summary>
        /// <param name="point"></param>
        /// <returns></returns>
        public override Vector3 NormalAt(Vector3 point)
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
        public override bool Hit(Ray r, float tMin, float tMax, out float t)
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
