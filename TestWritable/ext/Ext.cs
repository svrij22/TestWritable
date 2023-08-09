using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace TestWritable
{
    internal class Ext
    {
        public static int RGBToColorInt(int r, int g, int b)
        {
            // Assume full opacity for alpha
            int a = 255;

            // Shift and combine the color values to produce the final integer
            int color = (a << 24) | (r << 16) | (g << 8) | b;

            return color;
        }
        public static Vector3 MultiplyVectorByScalar(Vector3 vector, float scalar)
        {
            return new Vector3(vector.X * scalar, vector.Y * scalar, vector.Z * scalar);
        }
        public static float GetIntensity(int color)
        {
            // Extract the individual color components (R, G, B)
            int red = (color >> 16) & 0xFF;
            int green = (color >> 8) & 0xFF;
            int blue = color & 0xFF;

            // Calculate the intensity using the luminance formula
            float intensity = 0.299f * red + 0.587f * green + 0.114f * blue;

            // Normalize to the range [0, 1]
            intensity /= 255.0f;

            return intensity;
        }
        internal static int MixColors(int color1, int color2, float weight)
        {
            float inverseWeight = 1.0f - weight;

            // Extract RGB components from color1
            int r1 = (color1 >> 16) & 0xFF;
            int g1 = (color1 >> 8) & 0xFF;
            int b1 = color1 & 0xFF;

            // Extract RGB components from color2
            int r2 = (color2 >> 16) & 0xFF;
            int g2 = (color2 >> 8) & 0xFF;
            int b2 = color2 & 0xFF;

            // Calculate mixed RGB components
            int mixedR = (int)(r1 * weight + r2 * inverseWeight);
            int mixedG = (int)(g1 * weight + g2 * inverseWeight);
            int mixedB = (int)(b1 * weight + b2 * inverseWeight);

            // Construct the mixed color
            return (255 << 24) | (mixedR << 16) | (mixedG << 8) | mixedB;
        }
    }
}
