using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Analytics.Utils
{
    public static class PropertyParser
    {
        public static double ParseNumber(string number)
        {
            return double.Parse(number);
        }
    }
}
