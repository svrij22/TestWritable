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

            while (currentDepth < maxDepth)
            {
                float closest = float.MaxValue;
                StructWrapper hitObject = new();
                bool hasHit = false;

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
                    accumulatedColor = AddColors(accumulatedColor, ScaleColor(hitObject.GetColor(), currentAttenuation));

                    // Check reflectivity before continuing the loop
                    if (reflectivity < 0.01f)
                        break;

                    // Update the attenuation for the next depth (this can be adjusted for more complex materials)
                    currentAttenuation = ScaleColor(currentAttenuation, new ColorStruct { R = (int)(reflectivity * 255), 
                                                                                          G = (int)(reflectivity * 255), 
                                                                                          B = (int)(reflectivity * 255) });
                }
                else
                {
                    // Add background color scaled by the current attenuation
                    accumulatedColor = AddColors(accumulatedColor, ScaleColor(new ColorStruct { R = 0, G = 0, B = 0 }, currentAttenuation));
                    break; // No need to continue if there's no hit
                }
            }

            return Ext.RGBToColorInt(accumulatedColor.R, accumulatedColor.G, accumulatedColor.B);
        }

        // Updated color addition and scaling functions
        private static ColorStruct AddColors(ColorStruct c1, ColorStruct c2)
        {
            return new ColorStruct
            {
                R = Math.Min(c1.R + c2.R, 255),
                G = Math.Min(c1.G + c2.G, 255),
                B = Math.Min(c1.B + c2.B, 255)
            };
        }

        // This is a helper function to scale a color by another color
        private static ColorStruct ScaleColor(ColorStruct c, ColorStruct scale)
        {
            return new ColorStruct
            {
                R = c.R * scale.R / 255,
                G = c.G * scale.G / 255,
                B = c.B * scale.B / 255
            };
        }
    }
}
