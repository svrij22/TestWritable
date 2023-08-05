using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace TestWritable
{
    public class Ray
    {
        public Vector3 Origin { get; private set; }
        public Vector3 Direction { get; private set; }

        public Ray(Vector3 origin, Vector3 direction)
        {
            Origin = origin;
            Direction = direction;
        }

        public Vector3 PointAtParameter(float t) => Origin + t * Direction;
    }
}
