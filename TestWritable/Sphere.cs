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
        public Sphere(Vector3 center, float radius, Color color)
        {
            this.Center = center;
            this.Radius = radius;
            this.color = color;
        }

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
