using ILGPU;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TestWritable.structs;

namespace TestWritable.engine
{
    internal class Tracer
    {
        public static int Trace(RayStruct ray, ArrayView<float> sphereData)
        {
            //Gets the closest object
            float closest = float.MaxValue;
            SphereStruct hitObject = new(new(), 0, 0, 0, 0, 0, 0, 0);
            bool hasHit = false;

            //Loop spheres
            for (int i = 0; i < SphereStruct.AmountFromFloatArr(sphereData); i++)
            {
                var obj = SphereStruct.SphereFromFloatArr(sphereData, i);
                if (obj.Hit(ray, 0.001f, float.MaxValue, out var dist)) // tMin set to small positive number to prevent self-intersection
                {
                    if (dist < closest)
                    {
                        closest = dist;
                        hitObject = obj;
                        hasHit = true;
                    }
                }
            }

            // If it hits an object, return the object's color
            if (hasHit)
            {
                // Get color when reaching max depth
                int red = hitObject.R;
                int green = hitObject.G;
                int blue = hitObject.B;

                //Calc direct color
                var directColor = Ext.RGBToColorInt(red, green, blue);

                // Return direct color
                return directColor;
            }

            // Return black if no hit
            return Ext.RGBToColorInt(0, 0, 0);
        }
    }
}
