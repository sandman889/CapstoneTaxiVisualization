using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Newtonsoft.Json;
using Microsoft.SqlServer.Types;
using CapstoneTaxiVisualization.Models;
using System.Diagnostics;
using System.Globalization;

namespace CapstoneTaxiVisualization.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            //POLYGON((-73.993 40.75, -73.993 40.752, -73.995 40.752, -73.995 40.75, -73.993 40.75))
            List<LatLong> points = new List<LatLong>();

            points.AddRange(new LatLong[] {
                new LatLong(40.75, -73.993), 
                new LatLong(40.752, -73.993), 
                new LatLong(40.752, -73.995), 
                new LatLong(40.75, -73.995),
                new LatLong(40.75, -73.993)
            });

            LatLong point = new LatLong(40.757575, -73.999999);

            //test calls for stored procedure
            //var example = GetPointsInPolygonRegion(new DateTime(2013, 11, 2), new DateTime(2013, 11, 9), points);
           // var example2 = GetNearestNeighbor(new DateTime(2013, 11, 2), new DateTime(2013, 11, 9), point);

            return View();
        }

        [HttpGet]
        public string GetNearestNeighbor(DateTime startDate, DateTime endDate, LatLong point)
        {
            using (TaxiDataEntities context = new TaxiDataEntities())
            {
                var attempt = 0;
                string jsonResult = String.Empty;

                //create the geography builder, set the SRID, and then begin the Point figure
                var geoPoint = new SqlGeographyBuilder();
                geoPoint.SetSrid(4326);
                geoPoint.BeginGeography(OpenGisGeographyType.Point);

                //place the point within the figure, and close everything out 
                geoPoint.BeginFigure(point.Latitude, point.Longitude);
                geoPoint.EndFigure();
                geoPoint.EndGeography();

                //get the final constructed geometry
                SqlGeography sqlPoint = geoPoint.ConstructedGeography;

                var testing = context.NearestPointQuery(startDate, endDate, 50, sqlPoint.ToString());

                return JsonConvert.SerializeObject(context.NearestPointQuery(startDate, endDate, 50, sqlPoint.ToString()));
            }
        }

        [HttpPost]
        public Object GetPointsInPolygonRegion(string startDateString, string endDateString, List<LatLong> boundPoints)
        {
            using (TaxiDataEntities context = new TaxiDataEntities())
            {
                /*DateTime? startDate = DateTime.ParseExact(startDateString.Substring(0, 24),
                                                    "ddd MMM d yyyy HH:mm:ss",
                                                    CultureInfo.InvariantCulture);

                DateTime? endDate = DateTime.ParseExact(endDateString.Substring(0, 24),
                                                    "ddd MMM d yyyy HH:mm:ss",
                                                    CultureInfo.InvariantCulture);*/

                DateTime? startDate = new DateTime(2013, 11, 02);
                DateTime? endDate = new DateTime(2013, 11, 03);

                var geoPoly = new SqlGeographyBuilder();

                //set the SRID and chose the sql geography datatype
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

                return context.GetPointsFromInsideRegion(startDate, endDate, sqlPoly.ToString());
            }
        }
    }
}