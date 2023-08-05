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
        public Vector3 Normal { get; set; }
        public float Offset { get; set; }
        public Color color { get; set; }

        public Plane(Vector3 normal, float offset, Color color)
        {
            Normal = normal;
            Offset = offset;
            this.color = color;
        }

        public override bool Hit(Ray r, float tMin, float tMax, out float t)
        {
            float denom = Vector3.Dot(Normal, r.Direction);
            if (Math.Abs(denom) > 0.0001f) // Avoid division by zero
            {
                t = (Offset - Vector3.Dot(Normal, r.Origin)) / denom;
                return t >= tMin && t <= tMax;
            }
            t = 0;
            return false;
        }
    }
}
