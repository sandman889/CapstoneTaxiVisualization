using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CapstoneTaxiVisualization.Classes
{
    public static class IEnumerableExtensions
    {
        public static IEnumerable<IEnumerable<T>> TakeChunks<T>(this IEnumerable<T> source, int size)
        {
            var list = new List<T>(size);

            foreach (T item in source)
            {
                list.Add(item);
                if (list.Count == size)
                {
                    List<T> chunk = list;
                    list = new List<T>(size);
                    yield return chunk;
                }
            }

            if (list.Count > 0)
            {
                yield return list;
            }
        }
    }
}