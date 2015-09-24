using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Newtonsoft.Json;
using Microsoft.SqlServer.Types;
using CapstoneTaxiVisualization.Models;
using System.Diagnostics;

namespace CapstoneTaxiVisualization.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            Stopwatch time = new Stopwatch();

            //POLYGON((-73.993 40.75, -73.993 40.752, -73.995 40.752, -73.995 40.75, -73.993 40.75))
            List<LatLong> points = new List<LatLong>();

            points.AddRange(new LatLong[] {
                new LatLong(40.75, -73.993), 
                new LatLong(40.752, -73.993), 
                new LatLong(40.752, -73.995), 
                new LatLong(40.75, -73.995),
                new LatLong(40.75, -73.993)
            });

            time.Start();
            //test call for stored procedure
            GetPointsInPolygonRegion(new DateTime(2013, 11, 2), new DateTime(2013, 11, 9), points);
            time.Stop();

            var test = time.ElapsedMilliseconds;
            return View();
        }

        public string GetNearestNeighbor(LatLong point)
        {
            //run the sql query to get the nearest neighbor within a certain distance, increase the distance by powers of two until one is found
            var test = "";

            return JsonConvert.SerializeObject(test);
        }

        [HttpPost]
        public string GetPointsInPolygonRegion(DateTime startDate, DateTime endDate, List<LatLong> boundPoints)
        {
            using (TaxiDataEntities context = new TaxiDataEntities())
            {
                var geoPoly = new SqlGeographyBuilder();

                //set the SRID and chose the sql geography datatype
                geoPoly.SetSrid(4326);
                geoPoly.BeginGeography(OpenGisGeographyType.Polygon);

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

                SqlGeography sqlPoly = geoPoly.ConstructedGeography;

                return JsonConvert.SerializeObject(context.GetPointsFromInsideRegion(startDate, endDate, sqlPoly.ToString()));
            }
        }
    }
}