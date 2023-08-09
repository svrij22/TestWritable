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
        /// <summary>
        /// Eerst alles opdelen in methoden
        /// Daarna de hele trace doen
        /// Dan backtrack kleuren mengen
        /// </summary>
        /// <param name="ray"></param>
        /// <param name="objects"></param>
        /// <param name="depth"></param>
        /// <returns></returns>
        /// 
        public static void Hit(Ray ray, List<TracerObject> objects, out TracerObject hitObject, out float dist)
        {
            //Gets the closest object
            dist = float.MaxValue;
            hitObject = null;
            foreach (var obj in objects)
            {
                if (obj.Hit(ray, 0.001f, float.MaxValue, out var d)) // tMin set to small positive number to prevent self-intersection
                {
                    if (d < dist)
                    {
                        dist = d;
                        hitObject = obj;
                    }
                }
            }
        }
        public static float ReflectionCoEfficient(Ray ray, TracerObject hitObject, Vector3 normal, float ior = 1.5f)
        {
            // Fresnel reflection coefficient
            float reflectionCoefficient = 0f;
            if (hitObject.Fresnel > 0)
                reflectionCoefficient = FresnelReflection(ray.Direction, normal, ior) * hitObject.Fresnel;
            reflectionCoefficient += hitObject.Reflectivity;
            reflectionCoefficient = Math.Clamp(reflectionCoefficient, 0, 1);
            return reflectionCoefficient;
        }
        public static int Trace(Ray ray, List<TracerObject> objects)
        {
            const int MAX_DEPTH = 4;

            int[] colorArray = new int[10];
            float[] mixtureAmount = new float[10];

            for(int depth = 0; depth < MAX_DEPTH; depth++)
            {
                //Do hit
                float dist;
                TracerObject hitObject;
                Hit(ray, objects, out hitObject, out dist);

                // If it hits an object, return the object's color
                if (hitObject != null)
                {
                    // Depth tracing for bounced rays
                    // Base properties
                    Vector3 hitPoint = ray.PointAtParameter(dist);
                    Vector3 normal = hitObject.NormalAt(hitPoint, ray);

                    //index of refraction
                    float ior = 1.5f; // Replace with the refractive index of the material
                    float reflectionCoefficient = ReflectionCoEfficient(ray, hitObject, normal, ior);

                    // Calculate the diffuse colour
                    int diffuseColour = GetDiffuseColour(hitObject, objects, hitPoint, normal);
                    colorArray[depth] = diffuseColour;  //Write down current diffuse colour
                    mixtureAmount[depth] = reflectionCoefficient;

                    if (depth < MAX_DEPTH)
                    {
                        Ray nextRay = ray.Bounce(hitPoint, normal);
                        ray = nextRay;
                    }
                    else
                    {
                        // Get color when reaching max depth
                        var red = (int)(hitObject.Color.R);
                        var green = (int)(hitObject.Color.G);
                        var blue = (int)(hitObject.Color.B);
                        var directColor = Ext.RGBToColorInt(red, green, blue);
                        colorArray[depth + 1] = directColor;
                    }
                }
                else
                {
                    // Return black if no hit
                    colorArray[depth] = Ext.RGBToColorInt(120, 150, 180);
                    mixtureAmount[depth] = 0;
                    break;
                }
            }

            // Start with the last color
            int resultColor = colorArray[colorArray.Count() - 1];

            // Work our way to the start
            for (int i = colorArray.Count() - 2; i >= 0; i--)
            {
                resultColor = Ext.MixColors(resultColor, colorArray[i], mixtureAmount[i]);
            }

            //Mix
            //int mixedColour = Ext.MixColors(bounceColour, diffuseColour, reflectionCoefficient);
            return resultColor;
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
                diffuseLightIntensity += light.Luminance * Math.Max(0, Vector3.Dot(Vector3.Normalize(light.Center - hitPoint), normal));
            }

            diffuseLightIntensity = Math.Clamp(diffuseLightIntensity, 0, 1);

            var red = (int)(hitObject.Color.R * diffuseLightIntensity);
            var green = (int)(hitObject.Color.G * diffuseLightIntensity);
            var blue = (int)(hitObject.Color.B * diffuseLightIntensity);
            return Ext.RGBToColorInt(red, green, blue);
        }
    }
}
