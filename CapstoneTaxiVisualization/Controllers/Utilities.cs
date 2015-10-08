using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using CapstoneTaxiVisualization.Models;
using Microsoft.SqlServer.Types;

namespace CapstoneTaxiVisualization.Controllers
{
    public static class Utilities
    {
        public static int jobCount = 0;

        /// <summary>
        /// Gets the Centroid of the polygon given by the boundPoints
        /// </summary>
        /// <param name="boundPoints">List of Latitudes and Longitudes that represent the polygon</param>
        /// <returns>LatLong object representing the centroid of the polygon</returns>
        public static LatLong GetCentroid(List<LatLong> boundPoints)
        {
            var geoPoly = new SqlGeographyBuilder();

            geoPoly.SetSrid(4326);
            geoPoly.BeginGeography(OpenGisGeographyType.Polygon);

            //set the initial point to skip, and use that to begin the figure
            var initialPoint = boundPoints.First();
            geoPoly.BeginFigure(initialPoint.Latitude, initialPoint.Longitude);

            foreach (var point in boundPoints)
            {
                if (point != initialPoint)
                {
                    //add each point to the geography poly
                    geoPoly.AddLine(point.Latitude, point.Longitude);
                }
            }

            //end the configuration of the geography
            geoPoly.EndFigure();
            geoPoly.EndGeography();

            //the final SQL polygon element
            SqlGeography sqlPoly = geoPoly.ConstructedGeography;
            var center = sqlPoly.MakeValid().EnvelopeCenter();
    
            return new LatLong(center.Lat.Value, center.Long.Value);
        }

        public static string BuildJsonString(List<string> json)
        {
            string jsonToReturn = String.Empty;
            List<string> cleanedJson = new List<string>();

            foreach (var str in json)
            {
                //remove the first and last character of the array which are the [] characters
                var temp = str.Remove(0, 1);
                temp = temp.Remove(temp.Length - 1, 1);
                cleanedJson.Add(temp);
            }

            //begin the array
            jsonToReturn += "[";

            foreach (var str in cleanedJson)
            {
                jsonToReturn += str + ',';
            }

            //remove the last comma, and close the array
            jsonToReturn = jsonToReturn.Remove(jsonToReturn.Length - 1);
            jsonToReturn += "]";

            return jsonToReturn;
        }
    }
}