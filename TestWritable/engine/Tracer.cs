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

            Vector3 lightPosition = new Vector3(0, -3, 0); // Some arbitrary direction for the light
            Vector3.Normalize(lightPosition);

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
                    Vector3 hitPoint = ray.Origin + ray.Direction * closest;
                    var normal = hitObject.NormalAt(hitPoint);

                    // Calculate direction from hitPoint to light source
                    Vector3 toLight = lightPosition - hitPoint;
                    Vector3.Normalize(toLight);

                    // Calculate the Lambertian reflection factor
                    float lambert = Math.Max(Vector3.Dot(normal, toLight), 0);

                    ray = ray.Bounce(hitPoint, normal);
                    currentDepth++;

                    var reflectivity = hitObject.GetReflectivity();

                    var ownColour = hitObject.GetColor();
                    var lambertColor = ColorStruct.Scale(ownColour, lambert); // This scales the object's color by the Lambertian factor
                    accumulatedColor = ColorStruct.Add(accumulatedColor, ColorStruct.Scale(scaledColour, currentAttenuation));

                    if (reflectivity < 0.01f)
                        break;

                    currentAttenuation = ColorStruct.Scale(currentAttenuation, new ColorStruct
                    {
                        R = (int)(reflectivity * 255),
                        G = (int)(reflectivity * 255),
                        B = (int)(reflectivity * 255)
                    });
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
