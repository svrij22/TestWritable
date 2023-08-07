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
        public Vector3 Min { get; private set; }
        public Vector3 Max { get; private set; }

        public Rectangle(Vector3 pos1, Vector3 pos2, Color color, float luminance = 1f, float reflectivity = 0f, float fresnel = 0f)
        {
            this.Pos1 = pos1;
            this.Pos2 = pos2;

            // Compute normal. Since we don't have a specific orientation, let's take a general approach:
            Vector3 v1 = new Vector3(Pos2.X - Pos1.X, 0, 0); 
            Vector3 v2 = new Vector3(0, Pos2.Y - Pos1.Y, 0);

            this.Normal = Vector3.Normalize(Vector3.Cross(pos2 - pos1, new Vector3(0, 1, 0))); // Assumes the rectangle is axis-aligned

            this.Center = (pos1 + pos2) / 2;
            this.Color = color;
            this.Luminance = luminance;
            this.Reflectivity = reflectivity;
            this.Fresnel = fresnel;

            Min = new Vector3(
                Math.Min(Pos1.X, Pos2.X),
                Math.Min(Pos1.Y, Pos2.Y),
                Math.Min(Pos1.Z, Pos2.Z)
            );
            Max = new Vector3(
                Math.Max(Pos1.X, Pos2.X),
                Math.Max(Pos1.Y, Pos2.Y),
                Math.Max(Pos1.Z, Pos2.Z)
            );
        }

        public override Vector3 NormalAt(Vector3 point, Ray r)
        {
            if (Vector3.Dot(Normal, r.Direction) < 0)
            {
                // Ray is hitting the front side of the rectangle
                return Normal;
            }
            else
            {
                // Ray is hitting the back side of the rectangle
                return -Normal;
            }
        }

        public override bool Hit(Ray r, float tMin, float tMax, out float t)
        {
            float denom = Vector3.Dot(Normal, r.Direction);
            if (MathF.Abs(denom) > 1e-6)
            {
                Vector3 oc = Center - r.Origin;
                t = Vector3.Dot(oc, Normal) / denom;

                if (t < tMax && t > tMin)
                {
                    Vector3 hitPoint = r.PointAtParameter(t);
                    hitPoint.X = (float)Math.Round(hitPoint.X, 3);
                    hitPoint.Y = (float)Math.Round(hitPoint.Y, 3);
                    hitPoint.Z = (float)Math.Round(hitPoint.Z, 3);

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
    }
}
