using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace TestWritable.structs
{
    public struct RayStruct
    {
        public Vector3 Origin;
        public Vector3 Direction;

        public RayStruct(Vector3 origin, Vector3 direction)
        {
            Origin = origin;
            Direction = direction;
        }

        /// <summary>
        /// This method returns a Vector3 point along the ray at a distance t from the ray's origin. 
        /// It calculates this point by starting at the ray's origin and moving t units along the ray's direction.
        /// This method is useful when you want to find a specific point along the ray given a parameter t.
        /// </summary>
        /// <param name="t"></param>
        /// <returns></returns>
        public Vector3 PointAtParameter(float t) => Origin + t * Direction;

        /// <summary>
        /// Bounce(Vector3 bouncePoint, Vector3 normal): This method calculates the reflected ray when the original ray hits a surface. 
        /// It takes as parameters the bounce point, which is the point of intersection with the surface, and the normal vector at the bounce point. 
        /// It returns a new Ray with the bounce point as the origin and the reflected direction. This method is useful in simulating the behavior of
        /// light when it hits a reflective surface in ray tracing.
        /// </summary>
        /// <param name="bouncePoint"></param>
        /// <param name="normal"></param>
        /// <returns></returns>
        public RayStruct Bounce(Vector3 bouncePoint, Vector3 normal)
        {
            Vector3 reflectedDirection = Vector3.Reflect(Direction, normal);
            return new RayStruct(bouncePoint, reflectedDirection);
        }
    }
}
