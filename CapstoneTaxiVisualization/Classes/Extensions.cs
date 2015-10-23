using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.IO;
using Newtonsoft.Json;
using CapstoneTaxiVisualization.Models;

namespace CapstoneTaxiVisualization.Classes
{
    public static class Extensions
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

        public static string ToJsonString(this PagedList<QueryDto> rhs)
        {
         /*   StringWriter sw = new StringWriter();
            JsonTextWriter writer = new JsonTextWriter(sw);

            //[
            writer.WriteStartArray();

            foreach (var list in rhs)
            {
                //{
                writer.WriteStartObject();

                writer.WritePropertyName("Pickup");
                //{
                writer.WriteStartObject();

                writer.WritePropertyName("Latitude");
                writer.WriteValue(elem.Pickup.Latitude);

                writer.WritePropertyName("Longitude");
                writer.WriteValue(elem.Pickup.Longitude);

                writer.WriteEndObject();
                //}

                writer.WriteStartObject();
                writer.WritePropertyName("Dropoff");

                writer.WritePropertyName("Dropoff");

                writer.WriteEndObject();
            }

            writer.WriteEndArray();

            return sw.ToString();*/
            return "";
        }

        public static PagedList<T> ToPagedList<T>(this IQueryable<T> source, int page, int pageSize)
        {
            return new PagedList<T>(source, page, pageSize);
        }
    }
}