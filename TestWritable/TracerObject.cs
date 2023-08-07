using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace TestWritable
{
    public abstract class TracerObject
    {
        public float Luminance { get; set; } = 1f;
        public float Reflectivity { get; set; } = 0f;
        public float Fresnel { get; set; } = 0f;
        public float Radius { get; set; }
        public Vector3 Center { get; set; }
        public Color Color { get; set; }

        public abstract Vector3 NormalAt(Vector3 point);
        public abstract bool Hit(Ray r, float tMin, float tMax, out float dist);

        public static Random random = new Random();
        public Vector3 GetRandomPoint()
        {
            double rand1 = random.NextDouble();
            double rand2 = random.NextDouble();
            if (rand1 == 0)
                random = new Random();

            float theta = (float)(rand1 * 2d * Math.PI);    // Random value between [0, 2π]
            float phi = (float)(Math.Acos(2 * rand2 - 1d)); // Random value between [0, π]

            float x = (float)(Radius * Math.Sin(phi) * Math.Cos(theta));
            float y = (float)(Radius * Math.Sin(phi) * Math.Sin(theta));
            float z = (float)(Radius * Math.Cos(phi));

            return new Vector3(Center.X + x, Center.Y + y, Center.Z + z);
        }
    }
}
