using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WavConfigCore.Tools
{
    public static class Extensions
    {

        // Return a shallow clone of a list.
        public static List<T> ShallowClone<T>(this List<T> items)
        {
            return new List<T>(items);
        }

        // Return a shallow clone of an array.
        public static T[] ShallowClone<T>(this T[] items)
        {
            return (T[])items.Clone();
        }
    }
}
