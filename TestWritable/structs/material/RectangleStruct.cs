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
        public Vector3 Pos1;
        public Vector3 Pos2;

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
            this.Pos1 = pos1;
            this.Pos2 = pos2;

            this.Normal = Vector3.Normalize(Vector3.Cross(pos2 - pos1, new Vector3(0, 1, 0)));

            this.Center = (pos1 + pos2) / 2;
            this.Color = ColorStruct.FromRGB(_r, _g, _b);
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
                    hitPoint.X = (float)Math.Round(hitPoint.X, 3);
                    hitPoint.Y = (float)Math.Round(hitPoint.Y, 3);
                    hitPoint.Z = (float)Math.Round(hitPoint.Z, 3);

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
