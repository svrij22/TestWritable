using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace TestWritable
{
    public class Rectangle : TracerObject
    {
        public Vector3 Pos1 { get; private set; }
        public Vector3 Pos2 { get; private set; }
        public Vector3 Normal { get; private set; }

        public Rectangle(Vector3 pos1, Vector3 pos2, Color color, float luminance = 1f, float reflectivity = 0f, float fresnel = 0f)
        {
            this.Pos1 = pos1;
            this.Pos2 = pos2;
            this.Center = (pos1 + pos2) / 2;
            this.Normal = Vector3.Normalize(Vector3.Cross(pos2 - pos1, new Vector3(0, 1, 0))); // Assumes the rectangle is axis-aligned
            this.Color = color;

            this.Luminance = luminance;
            this.Reflectivity = reflectivity;
            this.Fresnel = fresnel;
        }

        public override Vector3 NormalAt(Vector3 point)
        {
            return Normal; // Rectangle's normal is constant everywhere
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
                    Vector3 hitPoint = r.PointAtParameter(t);

                    // Check if the hit point is within the rectangle boundaries
                    if (hitPoint.X >= Math.Min(Pos1.X, Pos2.X) && hitPoint.X <= Math.Max(Pos1.X, Pos2.X) &&
                        hitPoint.Y >= Math.Min(Pos1.Y, Pos2.Y) && hitPoint.Y <= Math.Max(Pos1.Y, Pos2.Y) &&
                        hitPoint.Z >= Math.Min(Pos1.Z, Pos2.Z) && hitPoint.Z <= Math.Max(Pos1.Z, Pos2.Z))
                    {
                        return true;
                    }
                }
            }

            t = 0;
            return false;
        }
    }
}
