using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace TestWritable
{
    internal class RayTracer
    {
        public static int Trace(Ray r, List<TracerObject> objects, int depth = 0)
        {
            const int MAX_DEPTH = 4;

            //Gets the closest object
            float closest = float.MaxValue;
            TracerObject hitObject = null;
            foreach (var obj in objects)
            {
                if (obj.Hit(r, 0.001f, float.MaxValue, out var dist)) // tMin set to small positive number to prevent self-intersection
                {
                    if (dist < closest)
                    {
                        closest = dist;
                        hitObject = obj;
                    }
                }
            }

            // If it hits an object, return the object's color
            if (hitObject != null)
            {
                // Lambertian lighting
                Vector3 hitPoint = r.PointAtParameter(closest);
                Vector3 normal = hitObject.NormalAt(hitPoint);

                // Depth tracing for bounced rays
                if (depth < MAX_DEPTH)
                {
                    //Bounce ray
                    Ray bouncedRay = r.Bounce(hitPoint, normal);

                    // Trace the bounced ray
                    var bounceColour = Trace(bouncedRay, objects, depth + 1);

                    // Calculate the diffuse colour
                    var diffuseColour = GetDiffuseColour(hitObject, objects, hitPoint, normal);

                    // Mix the colours
                    var mixedColour = Ext.MixColors(bounceColour, diffuseColour, hitObject.Reflectivity);

                    // Return mixed colour
                    return mixedColour;
                }
                else
                {
                    // Get color when reaching max depth
                    var red = (int)(hitObject.Color.R);
                    var green = (int)(hitObject.Color.G);
                    var blue = (int)(hitObject.Color.B);
                    var directColor = Ext.RGBToColorInt(red, green, blue);

                    // Return direct color
                    return directColor;
                }
            }

            // Return black if no hit
            return Ext.RGBToColorInt(0, 0, 0);
        }
        private static int GetDiffuseColour(TracerObject hitObject, List<TracerObject> objects, Vector3 hitPoint, Vector3 normal)
        {
            var diffuseLightIntensity = hitObject.Luminance; //Ambient

            foreach (var light in objects.Where(obj => obj.Luminance > 0)) // Consider objects with Luminance > 0 as light sources
            {
                //Get light dir
                Vector3 lightDir = Vector3.Normalize(light.Center - hitPoint);

                // Check for shadows
                // Check if this light source is blocked by any other object
                Ray shadowRay = new Ray(hitPoint, lightDir);
                bool inShadow = false;
                foreach (var o in objects)
                {
                    if (o != hitObject && o != light && o.Hit(shadowRay, 0.001f, 1.0f, out _))
                    {
                        inShadow = true;
                        break;
                    }
                }

                if (!inShadow)
                {
                    // Add contribution of this light source to the diffuse light intensity
                    diffuseLightIntensity += light.Luminance * Math.Max(0, Vector3.Dot(lightDir, normal));
                }
            }

            // Clamp diffuseLightIntensity
            diffuseLightIntensity = Math.Clamp(diffuseLightIntensity, 0, 1);

            // Scale the object's colour by the total diffuse light intensity
            var red = (int)(hitObject.Color.R * diffuseLightIntensity);
            var green = (int)(hitObject.Color.G * diffuseLightIntensity);
            var blue = (int)(hitObject.Color.B * diffuseLightIntensity);
            return Ext.RGBToColorInt(red, green, blue);
        }
    }
}
