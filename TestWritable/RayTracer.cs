using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace TestWritable
{
    internal class RayTracer
    {
        public static int Trace(Ray r, List<TracerObject> objects)
        {

            //Gets the closest object
            float closest = float.MaxValue;
            TracerObject hitObject = null;
            foreach (var obj in objects)
            {
                if (obj.Hit(r, 0, float.MaxValue, out var dist))
                {
                    if (dist < closest)
                    {
                        closest = dist;
                        hitObject = obj;
                    }
                }
            }

            //If it hits and object, return the object's colour
            if (hitObject != null)
            {
                //Get color
                var red = (int)(hitObject.Color.R);
                var green = (int)(hitObject.Color.G);
                var blue = (int)(hitObject.Color.B);
                var directColor = Ext.RGBToColorInt(red, green, blue);
                
                return directColor;
            }

            return Ext.RGBToColorInt(0, 0, 0);
        }
    }
}
