﻿using ILGPU;
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
        public static int TraceMain(RayStruct ray, ArrayView<float> structData, int maxDepth)
        {
            int depth = 0;
            int resultColor = 0;

            while (depth < maxDepth)
            {
                resultColor = Trace(ray, structData, out RayStruct nextRay);
                ray = nextRay;
                depth++;
            }

            return resultColor;
        }
        public static int Trace(RayStruct ray, ArrayView<float> structData, out RayStruct nextRay)
        {
            //Do hit test
            float closest;
            StructWrapper hitObject;
            bool hasHit;
            HitTest(ray, structData, out closest, out hitObject, out hasHit);

            // If it hits an object, return the object's color
            if (hasHit)
            {
                // Base properties
                Vector3 hitPoint = ray.PointAtParameter(closest);
                Vector3 normal = hitObject.NormalAt(hitPoint);

                //index of refraction
                float ior = 1.5f; // Replace with the refractive index of the material

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
                diffuseLightIntensity += light.Luminance * Math.Max(0, Vector3.Dot(Vector3.Normalize(light.Center - hitPoint), normal));
            }

            diffuseLightIntensity = Math.Clamp(diffuseLightIntensity, 0, 1);

            var red = (int)(hitObject.Color.R * diffuseLightIntensity);
            var green = (int)(hitObject.Color.G * diffuseLightIntensity);
            var blue = (int)(hitObject.Color.B * diffuseLightIntensity);
            return Ext.RGBToColorInt(red, green, blue);
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
