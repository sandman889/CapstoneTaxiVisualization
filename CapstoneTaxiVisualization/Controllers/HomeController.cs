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
        public string GetPointsInPolygonRegion(DateTime startDate, DateTime endDate, List<LatLong> boundPoints, int queryFor)
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
                //DbGeography geog = DbGeography.FromText(sqlPoly.ToString(), 4326);

                var returnVal = context.RegionQueryPoly(startDate, endDate, sqlPoly.ToString(), queryFor)
                    .Select(x => new QueryDto
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
                        NumOfPassenger = x.passenger_count,
                        TripDistance = x.trip_distance
                    }).ToList();

                return JsonConvert.SerializeObject(returnVal);
            }
        }
    }
}