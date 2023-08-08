using ILGPU;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using TestWritable.structs;

namespace TestWritable.engine
{
    internal class Tracer
    {
        public static int Trace(RayStruct ray, ArrayView<float> structData, int maxDepth)
        {
            int currentDepth = 0;
            ColorStruct accumulatedColor = new ColorStruct { R = 0, G = 0, B = 0 };
            ColorStruct currentAttenuation = new ColorStruct { R = 255, G = 255, B = 255 };  // start with full color

            //While not reached max depth
            while (currentDepth < maxDepth)
            {
                //Do hit test
                float closest;
                StructWrapper hitObject;
                bool hasHit;
                HitTest(ray, structData, out closest, out hitObject, out hasHit);

                if (hasHit)
                {
                    //hitpoint and normal
                    Vector3 hitPoint = ray.Origin + ray.Direction * closest;
                    var normal = hitObject.NormalAt(hitPoint);

                    // Prepare for the next ray
                    ray = ray.Bounce(hitPoint, normal);
                    currentDepth++;

                    //reflectivity
                    var reflectivity = hitObject.GetReflectivity();

                    //Get color
                    var ownColour = hitObject.GetColor();
                    var scaledColour = ColorStruct.Scale(ownColour, 1 - reflectivity);
                    accumulatedColor = ColorStruct.Add(accumulatedColor, ColorStruct.Scale(scaledColour, currentAttenuation));

                    // Check reflectivity before continuing the loop
                    if (reflectivity < 0.01f)
                        break;

                    // Update the attenuation for the next depth (this can be adjusted for more complex materials)
                    currentAttenuation = ColorStruct.Scale(currentAttenuation, new ColorStruct { R = (int)(reflectivity * 255), 
                                                                                          G = (int)(reflectivity * 255), 
                                                                                          B = (int)(reflectivity * 255) });
                }
                else
                {
                    // Add background color scaled by the current attenuation
                    accumulatedColor = ColorStruct.Add(accumulatedColor, ColorStruct.Scale(new ColorStruct { R = 0, G = 0, B = 0 }, currentAttenuation));
                    break;
                }
            }

            return Ext.RGBToColorInt(accumulatedColor.R, accumulatedColor.G, accumulatedColor.B);
        }
        private static float SchlickFresnel(float r0, float cosTheta)
        {
            float oneMinusCosTheta = 1.0f - cosTheta;
            return r0 + (1.0f - r0) * oneMinusCosTheta * oneMinusCosTheta * oneMinusCosTheta * oneMinusCosTheta * oneMinusCosTheta;
        }

        public static void HitTest(RayStruct ray, ArrayView<float> structData, out float closest, out StructWrapper hitObject, out bool hasHit)
        {
            closest = float.MaxValue;
            hitObject = new();
            hasHit = false;

            for (int i = 0; i < StructExt.AmountOfObjects(structData); i++)
            {
                var obj = StructExt.DecodeStruct(structData, i);
                if (obj.Hit(ray, 0.001f, float.MaxValue, out var dist))
                {
                    if (dist < closest)
                    {
                        closest = dist;
                        hitObject = obj;
                        hasHit = true;
                    }
                }
            }
        }
    }
}
