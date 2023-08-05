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
    }
}
