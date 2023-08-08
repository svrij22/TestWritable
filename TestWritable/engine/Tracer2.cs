using ILGPU;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using TestWritable.structs;

namespace TestWritable.engine
{
    internal class Tracer2
    {
        public static int Trace(RayStruct ray, ArrayView<float> sphereData, int depth = 0)
        {
            const int MAX_DEPTH = 1;

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
                // Depth tracing for bounced rays
                if (depth < MAX_DEPTH)
                {
                    // Base properties
                    Vector3 hitPoint = ray.PointAtParameter(closest);
                    Vector3 normal = hitObject.NormalAt(hitPoint);

                    //index of refraction
                    float ior = 1.5f; // Replace with the refractive index of the material

                    // For glass materials
                    if (hitObject.IsGlass)
                    {

                        Vector3 outwardNormal;
                        Vector3 reflected = Vector3.Reflect(ray.Direction, hitObject.NormalAt(hitPoint));
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
                        RayStruct refractedRay = new RayStruct(hitPoint, refracted);
                        var refractCol = Trace(refractedRay, sphereData, depth + 1);

                        RayStruct reflectedRay = new RayStruct(hitPoint, reflected);
                        var reflectCol = Trace(reflectedRay, sphereData, depth + 1);

                        return Ext.MixColors(refractCol, reflectCol, 1 - reflectProb);
                    }

                    // Fresnel reflection coefficient
                    float reflectionCoefficient = 0f;
                    if (hitObject.Fresnel > 0)
                        reflectionCoefficient = FresnelReflection(ray.Direction, normal, ior) * hitObject.Fresnel;
                    reflectionCoefficient += hitObject.Reflectivity;
                    reflectionCoefficient = IntrinsicMath.Clamp(reflectionCoefficient, 0f, 1f);

                    //Bounce ray
                    RayStruct bouncedRay = ray.Bounce(hitPoint, normal);

                    // Trace the bounced ray
                    var bounceColour = Trace(bouncedRay, sphereData, depth + 1);

                    // Calculate the diffuse colour
                    var diffuseColour = GetDiffuseColour(hitObject, sphereData, hitPoint, normal);

                    // Mix the colours
                    var mixedColour = Ext.MixColors(bounceColour, diffuseColour, reflectionCoefficient);

                    // Return mixed colour
                    return mixedColour;
                }
                else
                {
                    // Get color when reaching max depth
                    var red = (int)(hitObject.R);
                    var green = (int)(hitObject.G);
                    var blue = (int)(hitObject.B);
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
        private static int GetDiffuseColour(SphereStruct hitObject, ArrayView<float> sphereData, Vector3 hitPoint, Vector3 normal)
        {

            //Base light intensity is ambient light
            var diffuseLightIntensity = hitObject.Luminance; //Ambient

            //For each luminant object
            for (int i = 0; i < SphereStruct.AmountFromFloatArr(sphereData); i++)
            {
                //is luminant
                var luminant_object = SphereStruct.SphereFromFloatArr(sphereData, i);
                if (luminant_object.Luminance < .2f)
                    continue;

                //Cast 8 rays
                int numSoftShadowRays = 5;
                int numShadowHits = 0;
                for (int i2 = 0; i2 < numSoftShadowRays; i2++)
                {
                    //Point to random object on light
                    Vector3 randomPointOnLight = luminant_object.GetRandomPoint(); // Assuming your TracerObject can provide a random point on its surface
                    Vector3 lightDir = Vector3.Normalize(randomPointOnLight - hitPoint);
                    RayStruct shadowRay = new RayStruct(hitPoint, lightDir);

                    //Check if ray is in shadow
                    bool inShadow = false;
                    for (int i3 = 0; i3 < SphereStruct.AmountFromFloatArr(sphereData); i3++)
                    {
                        //Get check object
                        var o = SphereStruct.SphereFromFloatArr(sphereData, i2);

                        //Check if is not base object or luminant object
                        if (o.Center != hitObject.Center &&
                            o.Center != luminant_object.Center
                            && o.IsGlass && o.Hit(shadowRay, 0.001f, 1.0f, out _))
                        {
                            inShadow = true;
                            break;
                        }
                    }

                    //Shadow hits
                    if (inShadow)
                        numShadowHits++;

                    //Skip if x times consequetively zero
                    if (numShadowHits == 0 && i2 == (numSoftShadowRays / 2))
                        i2 = numSoftShadowRays;
                }

                //Calculate shadow factor
                float shadowFactor = 1.0f - (float)numShadowHits / numSoftShadowRays;
                diffuseLightIntensity += luminant_object.Luminance * shadowFactor * Math.Max(0, Vector3.Dot(Vector3.Normalize(luminant_object.Center - hitPoint), normal));
            }
            diffuseLightIntensity = IntrinsicMath.Clamp(diffuseLightIntensity, 0.0f, 1.0f);

            var red = (int)(hitObject.R * diffuseLightIntensity);
            var green = (int)(hitObject.G * diffuseLightIntensity);
            var blue = (int)(hitObject.B * diffuseLightIntensity);
            return Ext.RGBToColorInt(red, green, blue);
        }
    }
}
