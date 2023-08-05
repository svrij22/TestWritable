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
        public static Vector3 LightDirection = new Vector3(0, 1, 0); // Light coming from the top
        public static int Trace(Ray r, List<TracerObject> objects)
        {
            foreach (var obj in objects)
            {
                if (obj.Hit(r, float.MinValue, float.MaxValue, out var t))
                {
                    // Calculate lighting
                    var hitPoint = r.PointAtParameter(t);
                    var N = Vector3.Normalize(hitPoint - obj.Center);
                    var intensity = Vector3.Dot(-LightDirection, N); // Light intensity based on angle with light source
                    intensity = Math.Clamp(intensity, 0, 1); // Clamp the intensity value between 0 and 1

                    var red = (int)(obj.color.R * intensity); // Compute red color component based on light intensity
                    var green = (int)(obj.color.G * intensity); // Compute red color component based on light intensity
                    var blue = (int)(obj.color.B * intensity); // Compute red color component based on light intensity

                    return Ext.RGBToColorInt(red, green, blue);
                }
            }
            return Ext.RGBToColorInt(0, 0, 0);
        }
    }
}
