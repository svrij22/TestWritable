﻿using System;
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
        public Vector3 Center { get; set; }
        public Color Color { get; set; }

        public abstract Vector3 NormalAt(Vector3 point);
        public abstract bool Hit(Ray r, float tMin, float tMax, out float dist);
    }
}
