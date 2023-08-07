using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Media3D;
using static System.Windows.Forms.DataFormats;
using static TestWritable.TracerObject;

namespace TestWritable
{
    internal class RayTracer
    {
        public static int Trace(Ray ray, List<TracerObject> objects, int depth = 0)
        {
            const int MAX_DEPTH = 4;

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
                // Depth tracing for bounced rays
                if (depth < MAX_DEPTH)
                {
                    // Base properties
                    Vector3 hitPoint = ray.PointAtParameter(closest);
                    Vector3 normal = hitObject.NormalAt(hitPoint, ray);

                    //index of refraction
                    float ior = 1.5f; // Replace with the refractive index of the material

                    // For glass materials
                    if (hitObject.Material == MaterialType.Glass)
                    {

                        Vector3 outwardNormal;
                        Vector3 reflected = Vector3.Reflect(ray.Direction, hitObject.NormalAt(hitPoint, ray));
                        float ni_over_nt;
                        float reflectProb;
                        Vector3 refracted;
                        if (Vector3.Dot(ray.Direction, normal) > 0)
                        {
                            outwardNormal = -normal;
                            ni_over_nt = ior;  // ior is the refractive index for the material
                            reflectProb = FresnelReflection(ray.Direction, normal, ior);
                        }
                        else
                        {
                            outwardNormal = normal;
                            ni_over_nt = 1.0f / ior;
                            reflectProb = FresnelReflection(ray.Direction, -normal, ior);
                        }
                        static float Lerp(float a, float b, float t)
                        {
                            return (1 - t) * a + t * b;
                        }
                        reflectProb = Lerp(reflectProb, 0.5f, 0.1f);

                        Refract(ray.Direction, outwardNormal, ni_over_nt, out refracted);
                        Ray refractedRay = new Ray(hitPoint, refracted);
                        var refractCol = Trace(refractedRay, objects, depth + 1);

                        Ray reflectedRay = new Ray(hitPoint, reflected);
                        var reflectCol = Trace(reflectedRay, objects, depth + 1);

                        return Ext.MixColors(refractCol, reflectCol, 1-reflectProb);
                    }

                    // Fresnel reflection coefficient
                    float reflectionCoefficient = 0f;
                    if (hitObject.Fresnel > 0)
                        reflectionCoefficient = FresnelReflection(ray.Direction, normal, ior) * hitObject.Fresnel;
                    reflectionCoefficient += hitObject.Reflectivity;
                    reflectionCoefficient = Math.Clamp(reflectionCoefficient, 0, 1);

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
            return Ext.RGBToColorInt(120, 150, 180);
        }

        /// <summary>
        /// Fresnel reflection
        /// </summary>
        /// <param name="incidentDirection"></param>
        /// <param name="normal"></param>
        /// <param name="ior"></param>
        /// <returns></returns>
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
        /// function that computes the refracted ray given an incident ray and a normal. This function should also handle the total internal reflection.
        /// </summary>
        /// <param name="v"></param>
        /// <param name="n"></param>
        /// <param name="ni_over_nt"></param>
        /// <param name="refracted"></param>
        /// <returns></returns>
        private static bool Refract(Vector3 v, Vector3 n, float ni_over_nt, out Vector3 refracted)
        {
            Vector3 uv = Vector3.Normalize(v);
            float dt = Vector3.Dot(uv, n);
            float discriminant = 1.0f - ni_over_nt * ni_over_nt * (1 - dt * dt);
            if (discriminant > 0)
            {
                refracted = ni_over_nt * (uv - n * dt) - n * MathF.Sqrt(discriminant);
                return true;
            }
            refracted = default(Vector3);
            return false;
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
            
            //Base light intensity is ambient light
            var diffuseLightIntensity = hitObject.Luminance; //Ambient

            //For each luminant object
            var luminant_objects = objects.Where(obj => obj.Luminance > .2f);
            foreach (var light in luminant_objects) // Consider objects with Luminance > 0 as light sources
            {
                
                //Cast 8 rays
                int numSoftShadowRays = 8;
                int numShadowHits = 0;
                for (int i = 0; i < numSoftShadowRays; i++)
                {
                    //Point to random object on light
                    Vector3 randomPointOnLight = light.GetRandomPoint(); // Assuming your TracerObject can provide a random point on its surface
                    Vector3 lightDir = Vector3.Normalize(randomPointOnLight - hitPoint);
                    Ray shadowRay = new Ray(hitPoint, lightDir);

                    //Check if ray is in shadow
                    bool inShadow = false;
                    foreach (var o in objects)
                    {
                        if (o != hitObject && 
                            o != light 
                            && o.Material != MaterialType.Glass && o.Hit(shadowRay, 0.001f, 1.0f, out _))
                        {
                            inShadow = true;
                            break;
                        }
                    }

                    //Shadow hits
                    if (inShadow)
                        numShadowHits++;

                    //Skip if x times consequetively zero
                    if (numShadowHits == 0 && i == (numSoftShadowRays/2))
                        i = numSoftShadowRays;
                }

                //Calculate shadow factor
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
