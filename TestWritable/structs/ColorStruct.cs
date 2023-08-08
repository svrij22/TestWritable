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
    }
}
