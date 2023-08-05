using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace TestWritable
{
    internal class Tracer
    {
        public static Vector3 LightPosition = new Vector3(0, -4, 0); // Position of the light source

        public const int MaxDepth = 1; // Maximum recursion depth for reflections

        public static int Trace(Ray r, List<TracerObject> objects, int depth = 0)
        {
            if (depth >= MaxDepth)
            {
                return Ext.RGBToColorInt(0, 0, 0); // Return black if maximum recursion depth is reached
            }

            float closest = float.MaxValue;
            TracerObject hitObject = null;

            foreach (var obj in objects)
            {
                if (obj.Hit(r, float.MinValue, float.MaxValue, out var t))
                {
                    if (t < closest)
                    {
                        closest = t;
                        hitObject = obj;
                    }
                }
            }

            if (hitObject != null)
            {
                // Calculate lighting
                var hitPoint = r.PointAtParameter(closest);
                var N = Vector3.Normalize(hitPoint - hitObject.Center);

                var LightDirection = Vector3.Normalize(LightPosition - hitPoint);
                var intensity = Vector3.Dot(LightDirection, N);
                intensity = Math.Clamp(intensity, 0, 1);
                intensity = (float)Math.Pow(intensity, .5);
                intensity = Math.Clamp(intensity, 0, 1);

                var red = (int)(hitObject.color.R * intensity);
                var green = (int)(hitObject.color.G * intensity);
                var blue = (int)(hitObject.color.B * intensity);

                var directColor = Ext.RGBToColorInt(red, green, blue);

                // Calculate reflection
                var reflectionDirection = Vector3.Normalize(-r.Direction - 2 * Vector3.Dot(-r.Direction, N) * N);
                var offsetHitPoint = hitPoint + N * 0.001f; // Add small offset to avoid self intersection
                var reflectionRay = new Ray(offsetHitPoint, reflectionDirection);
                var reflectionColor = Trace(reflectionRay, objects, depth + 1);

                // Mix the direct color and the reflection color
                var mixedColor = Ext.MixColors(directColor, reflectionColor, 0.5f); // Mix 50% of each color

                return mixedColor;
            }

            return Ext.RGBToColorInt(0, 0, 0);
        }
    }
}
