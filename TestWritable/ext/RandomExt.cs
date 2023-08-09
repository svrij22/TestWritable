using ILGPU;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestWritable.ext
{
    internal class RandomExt
    {
        public static double GetRandom(ArrayView<double> random, int index)
        {
            index = (int)(index % random.Length);
            double rand = random[index];
            return rand;
        }
    }
}
