using ILGPU;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using static TestWritable.structs.StructExt;

namespace TestWritable.structs
{
    public struct StructWrapper
    {
        public ArrayView<float> floatData;
        private readonly int readFrom;
        public StructWrapper(ArrayView<float> floatData, int readFrom)
        {
            this.floatData = floatData;
            this.readFrom = readFrom;
        }

        /// <summary>
        /// Is glass
        /// </summary>
        public bool IsGlass()
        {
            if (floatData[readFrom] == (float)StructType.Sphere)
            {
                SphereStruct sStruct = SphereStruct.Decode(floatData, readFrom);
                return sStruct.IsGlass;
            }

            //Failure
            return false;
        }


        /// <summary>
        /// Hit switch
        /// </summary>
        public float GetLuminance()
        {
            if (floatData[readFrom] == (float)StructType.Sphere)
            {
                SphereStruct sStruct = SphereStruct.Decode(floatData, readFrom);
                return sStruct.Luminance;
            }
            if (floatData[readFrom] == (float)StructType.Plane)
            {
                PlaneStruct pStruct = PlaneStruct.Decode(floatData, readFrom);
                return pStruct.Luminance;
            }

            //Failure
            return 0;
        }
        /// <summary>
        /// Hit switch
        /// </summary>
        public float GetFresnel()
        {
            if (floatData[readFrom] == (float)StructType.Sphere)
            {
                SphereStruct sStruct = SphereStruct.Decode(floatData, readFrom);
                return sStruct.Fresnel;
            }
            if (floatData[readFrom] == (float)StructType.Plane)
            {
                PlaneStruct pStruct = PlaneStruct.Decode(floatData, readFrom);
                return pStruct.Fresnel;
            }

            //Failure
            return 0;
        }


        /// <summary>
        /// Hit switch
        /// </summary>
        public float GetReflectivity()
        {
            if (floatData[readFrom] == (float)StructType.Sphere)
            {
                SphereStruct sStruct = SphereStruct.Decode(floatData, readFrom);
                return sStruct.Reflectivity;
            }
            if (floatData[readFrom] == (float)StructType.Plane)
            {
                PlaneStruct pStruct = PlaneStruct.Decode(floatData, readFrom);
                return pStruct.Reflectivity;
            }

            //Failure
            return 0;
        }

        /// <summary>
        /// Hit switch
        /// </summary>
        public ColorStruct GetColor()
        {
            if (floatData[readFrom] == (float)StructType.Sphere)
            {
                SphereStruct sStruct = SphereStruct.Decode(floatData, readFrom);
                return sStruct.Color;
            }
            if (floatData[readFrom] == (float)StructType.Plane)
            {
                PlaneStruct pStruct = PlaneStruct.Decode(floatData, readFrom);
                return pStruct.Color;
            }

            //Failure
            return new ColorStruct();
        }

        /// <summary>
        /// Hit switch
        /// </summary>
        public Vector3 NormalAt(Vector3 point)
        {
            if (floatData[readFrom] == (float)StructType.Sphere)
            {
                SphereStruct sStruct = SphereStruct.Decode(floatData, readFrom);
                return sStruct.NormalAt(point);
            }
            if (floatData[readFrom] == (float)StructType.Plane)
            {
                PlaneStruct pStruct = PlaneStruct.Decode(floatData, readFrom);
                return pStruct.NormalAt(point);
            }

            //Failure
            return new Vector3();
        }

        /// <summary>
        /// Hit switch
        /// </summary>
        public bool Hit(RayStruct r, float tMin, float tMax, out float t)
        {
            if (floatData[readFrom] == (float)StructType.Sphere)
            {
                SphereStruct sStruct = SphereStruct.Decode(floatData, readFrom);
                return sStruct.Hit(r, tMin, tMax, out t);
            }
            if (floatData[readFrom] == (float)StructType.Plane)
            {
                PlaneStruct pStruct = PlaneStruct.Decode(floatData, readFrom);
                return pStruct.Hit(r, tMin, tMax, out t);
            }

            //Failure
            t = 0;
            return false;
        }

        internal Vector3 GetCenter()
        {
            if (floatData[readFrom] == (float)StructType.Sphere)
            {
                SphereStruct sStruct = SphereStruct.Decode(floatData, readFrom);
                return sStruct.Center;
            }
            if (floatData[readFrom] == (float)StructType.Plane)
            {
                PlaneStruct pStruct = PlaneStruct.Decode(floatData, readFrom);
                return pStruct.Center;
            }

            //Failure
            return new();
        }
    }
}
