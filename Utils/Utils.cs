using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace LikeABrawler2
{
    public static class Utils
    {
        public static float Next(this Random rnd, float min, float max)
        {
            double val = (rnd.NextDouble() * (max - min) + min); 
            return (float)val;
        }

        public static IntPtr ToIntPtr(this object target)
        {
            IntPtr allocedObj = Marshal.AllocHGlobal(Marshal.SizeOf(target));
            Marshal.StructureToPtr(target, allocedObj, false);

            return allocedObj;
        }
    }
}
