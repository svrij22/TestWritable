using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestWritable.structs
{
    public struct ColorStruct
    {
        public int R { get;  set; }
        public int G { get;  set; }
        public int B { get;  set; }
        public static ColorStruct FromRGB(int r, int g, int b)
        {
            return new ColorStruct()
            {
                R = r,
                G = g,
                B = b
            };
        }
        public static ColorStruct Mix(ColorStruct c1, ColorStruct c2, float amount)
        {
            float inverseAmount = 1.0f - amount;

            int r = Clamp((int)((c1.R * inverseAmount + c2.R * amount) * 255));
            int g = Clamp((int)((c1.G * inverseAmount + c2.G * amount) * 255));
            int b = Clamp((int)((c1.B * inverseAmount + c2.B * amount) * 255));

            return FromRGB(r, g, b);
        }

        public static ColorStruct Scale(ColorStruct c1, float amount)
        {
            int r = Clamp((int)(c1.R * amount));
            int g = Clamp((int)(c1.G * amount));
            int b = Clamp((int)(c1.B * amount));

            return FromRGB(r, g, b);
        }
        public static ColorStruct Scale(ColorStruct c, ColorStruct scale)
        {
            return new ColorStruct
            {
                R = c.R * scale.R / 255,
                G = c.G * scale.G / 255,
                B = c.B * scale.B / 255
            };
        }

        // Updated color addition and scaling functions
        public static ColorStruct Add(ColorStruct c1, ColorStruct c2)
        {
            return new ColorStruct
            {
                R = Math.Min(c1.R + c2.R, 255),
                G = Math.Min(c1.G + c2.G, 255),
                B = Math.Min(c1.B + c2.B, 255)
            };
        }

        private static int Clamp(int value)
        {
            if (value < 0) return 0;
            if (value > 255) return 255;
            return value;
        }
    }
}

