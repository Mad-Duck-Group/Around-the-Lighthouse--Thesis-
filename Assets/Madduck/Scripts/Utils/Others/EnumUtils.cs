using System;
using System.Numerics;
using System.Linq;
using System.Runtime.CompilerServices;

namespace Madduck.Scripts.Utils.Others
{
    public static class EnumUtils
    {
        public static T Max<T>(this T src) where T : Enum
        {
            return Enum.GetValues(typeof(T)).Cast<T>().Max();
        }

        public static T Min<T>(this T src) where T : Enum
        {
            return Enum.GetValues(typeof(T)).Cast<T>().Min();
        }
    }
}