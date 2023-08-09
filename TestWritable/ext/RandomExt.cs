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
        public static double GetRandom(ArrayView<double> random, int index, out int nindex)
        {
            double rand = random[index];
            if (index == 0)
            {
                nindex = ((int)(random.Length * rand));
                return rand;
            }
            nindex = index + 1;
            return rand;
        }
    }
}
