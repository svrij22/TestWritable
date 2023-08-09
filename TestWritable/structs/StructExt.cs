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

        public const int BlockSize = 32;
        public static float[] EncodeToBlock(float[] arr)
        {
            float[] block = new float[BlockSize];
            for (int i = 0; i < arr.Length; i++)
            {
                block[i] = arr[i];
            }
            return block;
        }

        // Count the number of objects (determined by the number of -123123123 values)
        public static int AmountOfObjects(ArrayView<float> arr)
        {
            return (int)(arr.Length / 32);
        }

        // Decode a specific struct using the object's index
        public static StructWrapper DecodeStruct(ArrayView<float> arr, int objectIndex)
        {
            return new StructWrapper(arr, objectIndex * 32);
        }
    }
}
