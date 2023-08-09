using ILGPU;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestWritable.structs
{
    public class StructExt
    {
        public enum StructType
        {
            Sphere,
            Plane,
            Rectangle
        }

        // Count the number of objects (determined by the number of -123123123 values)
        public static int AmountOfObjects(ArrayView<float> arr)
        {
            int count = 0;
            for (int i = 0; i < arr.Length; i++)
            {
                if (arr[i] == -123123123)
                {
                    count++;
                }
            }
            return count;
        }

        // Decode a specific struct using the object's index
        public static StructWrapper DecodeStruct(ArrayView<float> arr, int objectIndex)
        {
            int currentObject = 0;
            int readFrom = 0;

            for (int i = 0; i < arr.Length; i++)
            {
                if (arr[i] == -123123123)
                {
                    if (currentObject == objectIndex)
                    {
                        return new StructWrapper(arr, readFrom);
                    }

                    currentObject++;
                    readFrom = i + 1;
                }
            }

            return new StructWrapper();
        }
    }
}
