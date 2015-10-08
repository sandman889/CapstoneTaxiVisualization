using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Threading;
using System.Web.Mvc;
using Newtonsoft.Json;
using Microsoft.SqlServer.Types;
using CapstoneTaxiVisualization.Models;
using System.Diagnostics;
using System.Globalization;
using System.Web.Configuration;

namespace CapstoneTaxiVisualization.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            Stopwatch threaded = new Stopwatch();
            Stopwatch nonThread = new Stopwatch();
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
           /* threaded.Start();
            var exampleThread = GetPointsInPolygonRegionThreaded(points, new DateTime(2013, 11, 2), new DateTime(2013, 11, 7));
            threaded.Stop();

            nonThread.Start();
            var example = GetPointsInPolygonRegion(new DateTime(2013, 11, 2), new DateTime(2013, 11, 7), points);
            nonThread.Stop();
            //var example2 = GetNearestNeighbor(new DateTime(2013, 11, 2), new DateTime(2013, 11, 9), point);

            var test = example == exampleThread;
            var threadTime = threaded.ElapsedMilliseconds;
            var singleTime = nonThread.ElapsedMilliseconds;*/
            return View();
        }

        public string GetPointsInPolygonRegion(DateTime startDate, DateTime endDate, List<LatLong> boundPoints)
        {
            using (TaxiDataEntities context = new TaxiDataEntities())
            {
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

                return JsonConvert.SerializeObject(context.GetPointsFromInsideRegion(startDate, endDate, sqlPoly.ToString()));
            }
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
        public string GetPointsInPolygonRegionThreaded(List<LatLong> boundPoints, DateTime startDate, DateTime endDate)
        {
            //obtain the centroid of the polygon to divide it properly
            LatLong centroid = Utilities.GetCentroid(boundPoints);

            //set up the list of stored procedure objects to run in the threads
            List<StoredProcedures> procThreads = new List<StoredProcedures>();
            for (int i = 0; i < boundPoints.Count() - 1; ++i)
            {
                var temp = new StoredProcedures();
                temp.boundPoints.AddRange(new LatLong[] {new LatLong(centroid.Latitude, centroid.Longitude), 
                                                         new LatLong(boundPoints.ElementAt(i).Latitude, boundPoints.ElementAt(i).Longitude),
                                                         new LatLong(boundPoints.ElementAt(i + 1).Latitude, boundPoints.ElementAt(i + 1).Longitude),
                                                         new LatLong(centroid.Latitude, centroid.Longitude)
                                                        });
                temp.startDate = startDate;
                temp.endDate = endDate;

                procThreads.Add(temp);
            }

            //queue up all of the threads to run
            foreach (var proc in procThreads)
            {
                ThreadPool.QueueUserWorkItem(proc.GetPointsInPolygonRegion);
            }
            
            //block the main thread until the jobs have started
            while (Utilities.jobCount == 0) { /*blocking*/ }
            //now block until the jobs have finished
            while (Utilities.jobCount != 0) { /*blocking*/}

            return Utilities.BuildJsonString(procThreads.Select(x => x.jsonResult).ToList());
        }
    }

    public class StoredProcedures
    {
        public DateTime startDate;
        public DateTime endDate;
        public List<LatLong> boundPoints = new List<LatLong>();
        public string jsonResult = String.Empty;

        public void GetPointsInPolygonRegion(Object state)
        {
            Interlocked.Increment(ref Utilities.jobCount);
            using (TaxiDataEntities context = new TaxiDataEntities())
            {
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

                jsonResult = JsonConvert.SerializeObject(context.GetPointsFromInsideRegion(startDate, endDate, sqlPoly.ToString()));
            }
            Interlocked.Decrement(ref Utilities.jobCount);
        }
    }
}