using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace TestWritable
{
    public class Plane : TracerObject
    {
        public Vector3 Normal { get; private set; }
        public Plane(Vector3 center, Vector3 normal, Color color, double luminance = 1d)
        {
            this.Center = center;
            this.Normal = Vector3.Normalize(normal);
            this.Color = color;
            this.Luminance = luminance;
        }

        public override Vector3 NormalAt(Vector3 point)
        {
            return Normal; // Plane's normal is constant everywhere
        }

        public override bool Hit(Ray r, float tMin, float tMax, out float t)
        {
            float denom = Vector3.Dot(Normal, r.Direction);
            if (MathF.Abs(denom) > 1e-6) // Ensure we're not dividing by zero (or close to it)
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
    }
}
