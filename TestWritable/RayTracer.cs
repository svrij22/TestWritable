using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using static System.Windows.Forms.DataFormats;

namespace TestWritable
{
    internal class RayTracer
    {
        public static int Trace(Ray ray, List<TracerObject> objects, int depth = 0)
        {
            const int MAX_DEPTH = 2;

            //Gets the closest object
            float closest = float.MaxValue;
            TracerObject hitObject = null;
            foreach (var obj in objects)
            {
                if (obj.Hit(ray, 0.001f, float.MaxValue, out var dist)) // tMin set to small positive number to prevent self-intersection
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
                Vector3 hitPoint = ray.PointAtParameter(closest);
                Vector3 normal = hitObject.NormalAt(hitPoint);

                // Fresnel reflection coefficient
                float ior = 1.5f; // Replace with the refractive index of the material
                float reflectionCoefficient = 0f;
                if (hitObject.Fresnel > 0)
                    reflectionCoefficient = FresnelReflection(ray.Direction, normal, ior) * hitObject.Fresnel;
                reflectionCoefficient += hitObject.Reflectivity;
                reflectionCoefficient = Math.Clamp(reflectionCoefficient, 0, 1);

                // Depth tracing for bounced rays
                if (depth < MAX_DEPTH)
                {
                    //Bounce ray
                    Ray bouncedRay = ray.Bounce(hitPoint, normal);

                    // Trace the bounced ray
                    var bounceColour = Trace(bouncedRay, objects, depth + 1);

                    // Calculate the diffuse colour
                    var diffuseColour = GetDiffuseColour(hitObject, objects, hitPoint, normal);

                    // Mix the colours
                    var mixedColour = Ext.MixColors(bounceColour, diffuseColour, reflectionCoefficient);

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
            return Ext.RGBToColorInt(12, 12, 12);
        }

        private static float FresnelReflection(Vector3 incidentDirection, Vector3 normal, float ior)
        {
            float cosI = Math.Max(-1.0f, Math.Min(1.0f, Vector3.Dot(incidentDirection, normal)));
            float etaI = 1.0f; // Air's refractive index (approximately 1.0)
            float etaT = ior; // Material's refractive index

            if (cosI > 0)
            {
                // Outside the material, flip the indices of refraction
                float temp = etaI;
                etaI = etaT;
                etaT = temp;
            }

            // Compute the sine of the transmitted angle using Snell's law
            float sinT = etaI / etaT * (float)Math.Sqrt(Math.Max(0.0f, 1.0f - cosI * cosI));

            // Check for total internal reflection
            if (sinT >= 1.0f)
            {
                return 1.0f; // Total internal reflection, all light is reflected
            }
            else
            {
                float cosT = (float)Math.Sqrt(Math.Max(0.0f, 1.0f - sinT * sinT));
                cosI = Math.Abs(cosI);
                float Rs = ((etaT * cosI) - (etaI * cosT)) / ((etaT * cosI) + (etaI * cosT));
                float Rp = ((etaI * cosI) - (etaT * cosT)) / ((etaI * cosI) + (etaT * cosT));
                return (Rs * Rs + Rp * Rp) / 2.0f; // Average reflection coefficient
            }
        }

        /// <summary>
        /// Calculates the diffuse colour, uses lambertian
        /// </summary>
        /// <param name="hitObject"></param>
        /// <param name="objects"></param>
        /// <param name="hitPoint"></param>
        /// <param name="normal"></param>
        /// <returns></returns>

        public static Random random = new Random();
        private static int GetDiffuseColour(TracerObject hitObject, List<TracerObject> objects, Vector3 hitPoint, Vector3 normal)
        {
            var diffuseLightIntensity = hitObject.Luminance; //Ambient

            foreach (var light in objects.Where(obj => obj.Luminance > .2f)) // Consider objects with Luminance > 0 as light sources
            {
                int numSoftShadowRays = 8;
                int numShadowHits = 0;
                for (int i = 0; i < numSoftShadowRays; i++)
                {
                    Vector3 randomPointOnLight = light.GetRandomPoint(); // Assuming your TracerObject can provide a random point on its surface
                    Vector3 lightDir = Vector3.Normalize(randomPointOnLight - hitPoint);
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

                    if (inShadow)
                    {
                        numShadowHits++;
                    }

                    //Skip if x times consequetively zero
                    if (numShadowHits == 0 && i == (numSoftShadowRays/2))
                        i = numSoftShadowRays;
                }

                float shadowFactor = 1.0f - (float)numShadowHits / numSoftShadowRays;
                diffuseLightIntensity += light.Luminance * shadowFactor * Math.Max(0, Vector3.Dot(Vector3.Normalize(light.Center - hitPoint), normal));
            }

            diffuseLightIntensity = Math.Clamp(diffuseLightIntensity, 0, 1);

            var red = (int)(hitObject.Color.R * diffuseLightIntensity);
            var green = (int)(hitObject.Color.G * diffuseLightIntensity);
            var blue = (int)(hitObject.Color.B * diffuseLightIntensity);
            return Ext.RGBToColorInt(red, green, blue);
        }
    }
}
