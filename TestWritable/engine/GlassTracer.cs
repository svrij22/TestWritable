using ILGPU;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using TestWritable.ext;
using TestWritable.structs;

namespace TestWritable.engine
{
    public class GlassTracer
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
        public static float ReflectionCoEfficient(RayStruct ray, StructWrapper hitObject, Vector3 normal, float ior = 1.5f)
        {
            // Fresnel reflection coefficient
            float reflectionCoefficient = 0f;
            if (hitObject.GetFresnel() > 0)
                reflectionCoefficient = FresnelReflection(ray.Direction, normal, ior) * hitObject.GetFresnel();
            reflectionCoefficient += hitObject.GetReflectivity();
            reflectionCoefficient = IntrinsicMath.Clamp(reflectionCoefficient, 0, 1);
            return reflectionCoefficient;
        }
        public static int Trace(RayStruct ray, ArrayView<float> structData, ArrayView<double> randData, int pixelIndex)
        {
            const int MAX_DEPTH = 5;

            int[] colorArray = new int[10];
            float[] mixtureAmount = new float[10];

            for (int depth = 0; depth < MAX_DEPTH; depth++)
            {
                //Do hit test
                float dist;
                StructWrapper hitObject;
                bool hasHit;
                HitTest(ray, structData, out dist, out hitObject, out hasHit);

                // If it hits an object, return the object's color
                if (hasHit)
                {
                    // Depth tracing for bounced rays
                    // Base properties
                    Vector3 hitPoint = ray.PointAtParameter(dist);
                    Vector3 normal = hitObject.NormalAt(hitPoint, ray);

                    //index of refraction
                    float ior = 1.5f; // Replace with the refractive index of the material
                    float reflectionCoefficient = ReflectionCoEfficient(ray, hitObject, normal, ior);


                    // For glass materials
                    if (hitObject.IsGlass())
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

                        //Calculate refraction
                        Refract(ray.Direction, outwardNormal, ni_over_nt, out refracted);
                        RayStruct refractedRay = new RayStruct(hitPoint, refracted);
                        ray = refractedRay;
                        colorArray[depth] = 0xFFFFFF;
                        mixtureAmount[depth] = 1;
                        continue;
                    }

                    // Calculate the diffuse colour
                    int diffuseColour = GetDiffuseColour(hitObject, structData, randData, pixelIndex, hitPoint, normal);

                    //Set col
                    colorArray[depth] = diffuseColour;  //Write down current diffuse colour
                    mixtureAmount[depth] = reflectionCoefficient;

                    if (depth < MAX_DEPTH)
                    {
                        RayStruct nextRay = ray.Bounce(hitPoint, normal);
                        ray = nextRay;
                    }
                    else
                    {
                        // Get color when reaching max depth
                        var col = hitObject.GetColor();
                        var red = (int)(col.R);
                        var green = (int)(col.G);
                        var blue = (int)(col.B);
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
            int resultColor = colorArray[colorArray.Length - 1];

            // Work our way to the start
            for (int i = colorArray.Length - 2; i >= 0; i--)
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
            float sinT = etaI / etaT * MathF.Sqrt(Math.Max(0.0f, 1.0f - cosI * cosI));

            // Check for total internal reflection
            if (sinT >= 1.0f)
            {
                return 1.0f; // Total internal reflection, all light is reflected
            }
            else
            {
                float cosT = MathF.Sqrt(Math.Max(0.0f, 1.0f - sinT * sinT));
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
        private static int GetDiffuseColour(StructWrapper hitObject, ArrayView<float> structData, ArrayView<double> randData, int doublePointer, Vector3 hitPoint, Vector3 normal)
        {
            //Base light intensity is ambient light
            var diffuseLightIntensity = hitObject.GetLuminance(); //Ambient
            for (int i = 0; i < StructExt.AmountOfObjects(structData); i++)
            {
                //Base lumin
                var light_obj = StructExt.DecodeStruct(structData, i);
                var lumin = light_obj.GetLuminance();
                if (lumin > .2f)
                {
                    //Cast 8 rays
                    int numSoftShadowRays = 1;
                    int numShadowHits = 0;
                    for (int i2 = 0; i2 < numSoftShadowRays; i2++)
                    {
                        //Generate random 
                        //double rand1 = RandomExt.GetRandom(randData, (doublePointer * 32));
                        //doublePointer++;

                        //Generate random 
                        //double rand2 = RandomExt.GetRandom(randData, (doublePointer * 32));
                        //doublePointer++;

                        //Point to random object on light
                        //Vector3 randomPointOnLight = light_obj.GetRandomPoint(rand1, rand2); // Assuming your TracerObject can provide a random point on its surface
                        Vector3 randomPointOnLight = light_obj.GetCenter(); // Assuming your TracerObject can provide a random point on its surface
                        Vector3 lightDir = Vector3.Normalize(randomPointOnLight - hitPoint);
                        RayStruct shadowRay = new RayStruct(hitPoint, lightDir);

                        //Check if ray is in shadow
                        bool inShadow = false;
                        for (int i3 = 0; i3 < StructExt.AmountOfObjects(structData); i3++)
                        {
                            //Base lumin
                            var blocking_obj = StructExt.DecodeStruct(structData, i3);
                            if (blocking_obj.readFrom != light_obj.readFrom &&
                                blocking_obj.readFrom != hitObject.readFrom
                                && !blocking_obj.IsGlass() 
                                && blocking_obj.Hit(shadowRay, 0.001f, 5.0f, out _))
                            {
                                inShadow = true;
                                break;
                            }
                        }

                        //Shadow hits
                        if (inShadow)
                            numShadowHits++;

                        //Skip if x times consequetively zero
                        if (numShadowHits == 0 && i == (numSoftShadowRays / 2))
                            i = numSoftShadowRays;
                    }

                    //Calculate shadow factor
                    float shadowFactor = 1.0f - (float)numShadowHits / numSoftShadowRays;
                    diffuseLightIntensity += light_obj.GetLuminance() * shadowFactor * Math.Max(0, Vector3.Dot(Vector3.Normalize(light_obj.GetCenter() - hitPoint), normal));
                }
            }

            diffuseLightIntensity = IntrinsicMath.Clamp(diffuseLightIntensity, 0, 1);

            var col = hitObject.GetColor();
            var red = (int)(col.R * diffuseLightIntensity);
            var green = (int)(col.G * diffuseLightIntensity);
            var blue = (int)(col.B * diffuseLightIntensity);

            return Ext.RGBToColorInt(red, green, blue);
        }
    }
}
