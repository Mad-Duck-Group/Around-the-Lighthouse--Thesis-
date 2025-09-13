using System;
using System.Numerics;
using System.Linq;
using System.Runtime.CompilerServices;

namespace Madduck.Scripts.Utils.Others
{
    public static class EnumUtils
    {
        /// <summary>
        /// Get the maximum value of an enum type.
        /// </summary>
        /// <param name="src"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static T Max<T>(this T src) where T : Enum
        {
            return Enum.GetValues(typeof(T)).Cast<T>().Max();
        }

        /// <summary>
        /// Get the minimum value of an enum type.
        /// </summary>
        /// <param name="src"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static T Min<T>(this T src) where T : Enum
        {
            return Enum.GetValues(typeof(T)).Cast<T>().Min();
        }
    }
}