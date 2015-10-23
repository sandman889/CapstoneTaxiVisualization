using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Threading;
using System.IO;
using System.Web.Mvc;
using Newtonsoft.Json;
using Microsoft.SqlServer.Types;
using CapstoneTaxiVisualization.Models;
using System.Diagnostics;
using System.Globalization;
using System.Web.Configuration;
using CapstoneTaxiVisualization.Classes;
using System.Data.Entity.Spatial;

namespace CapstoneTaxiVisualization.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public string GetPointsInPolygonRegion(DateTime startDate, DateTime endDate, List<LatLong> boundPoints, string lookFor)
        {
            using (TaxiDataEntities context = new TaxiDataEntities())
            {
                //divide the polygon in half
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
                DbGeography geog = DbGeography.FromText(sqlPoly.ToString(), 4326);

                //var returnVal = context.GetPointsFromInsideRegion(startDate, endDate, sqlPoly.ToString()).Select(x => new LatLong(Convert.ToDouble(x.pickup_latitude), Convert.ToDouble(x.pickup_longitude)));

                //var resultTest = context.Identity_Smaller.AsNoTracking().AsParallel().Where()

                var result = context.Identity_Smaller.AsNoTracking().AsParallel().Where(x => x.pickup_datetime < endDate && x.pickup_datetime > startDate && x.pickup_geolocation.Intersects(geog));

                var returnVal = result.Select(x => new QueryDto
                {
                    Pickup = new LatLong
                    {
                        Latitude = (double)x.pickup_latitude,
                        Longitude = (double)x.pickup_longitude
                    },
                    Dropoff = new LatLong
                    {
                        Latitude = (double)x.dropoff_latitude,
                        Longitude = (double)x.dropoff_longitude
                    },
                    FareTotal = x.total_amount,
                    TravelTime = x.trip_time_in_secs,
                    NumOfPassenger = x.passenger_count
                }).ToList();

               // for(var i = 0; i < returnVal.GetPageCount()) {

                //}

                return JsonConvert.SerializeObject(returnVal);
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
    }
}