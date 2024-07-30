using System;
using System.Collections.Generic;
using System.Linq;
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
    }
}
