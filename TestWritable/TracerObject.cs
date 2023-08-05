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
        public Vector3 Center { get; set; }
        public Color color { get; set; }
        public abstract bool Hit(Ray r, float tMin, float tMax, out float t);
    }
}
