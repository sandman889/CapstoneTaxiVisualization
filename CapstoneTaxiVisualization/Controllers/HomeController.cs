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

        [HttpPost]
        public string GetPointsInCircleRegion(DateTime startDate, DateTime endDate, double radius, LatLong centroid, int queryFor)
        {
            using (TaxiDataEntities context = new TaxiDataEntities())
            {
                var geoPoint = new SqlGeographyBuilder();

                //set the SRID and build the sql geography point
                geoPoint.SetSrid(4326);
                geoPoint.BeginGeography(OpenGisGeographyType.Point);
                geoPoint.BeginFigure(centroid.Latitude, centroid.Longitude);
                geoPoint.EndFigure();
                geoPoint.EndGeography();

                SqlGeography sqlPoint = geoPoint.ConstructedGeography;


                var returnVal = context.RegionQueryCircle(startDate, endDate, radius, sqlPoint.ToString(), queryFor)
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

        [HttpPost]
        public string GetNearestPoint(DateTime startDate, DateTime endDate, LatLong point, int queryFor)
        {
            using (TaxiDataEntities context = new TaxiDataEntities())
            {
                var geoPoint = new SqlGeographyBuilder();

                //set the SRID and build the sql geography point
                geoPoint.SetSrid(4326);
                geoPoint.BeginGeography(OpenGisGeographyType.Point);
                geoPoint.BeginFigure(point.Latitude, point.Longitude);
                geoPoint.EndFigure();
                geoPoint.EndGeography();

                SqlGeography sqlPoint = geoPoint.ConstructedGeography;

                List<QueryDto> returnVal = new List<QueryDto>();
                int initialDistance = 10;

                while (returnVal.Count() == 0 && initialDistance < 100000)
                {
                    returnVal = context.NearestPointQuery(startDate, endDate, initialDistance, sqlPoint.ToString(), queryFor)
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

                    initialDistance *= 2;
                }

                return JsonConvert.SerializeObject(returnVal);
            }
        }

        [HttpPost]
        public string GetTripIntersectionByLine(DateTime startDate, DateTime endDate, List<LatLong> linePoints)
        {
            using (TaxiDataEntities context = new TaxiDataEntities())
            {
                var geoLine = new SqlGeographyBuilder();

                geoLine.SetSrid(4326);
                geoLine.BeginGeography(OpenGisGeographyType.LineString);
                geoLine.BeginFigure(linePoints.First().Latitude, linePoints.First().Longitude);
                geoLine.AddLine(linePoints.Last().Latitude, linePoints.Last().Longitude);
                geoLine.EndFigure();
                geoLine.EndGeography();

                SqlGeography sqlLine = geoLine.ConstructedGeography;

                var returnVal = context.LinesIntersectionQuery(startDate, endDate, sqlLine.ToString())
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

        [HttpPost]
        public string DualRegionQuery(DateTime startDate, DateTime endDate, List<LatLong> regionOnePoints, List<LatLong> regionTwoPoints)
        {
            using (TaxiDataEntities context = new TaxiDataEntities())
            {
                //build both of the polygons to send to the server
                var poly1 = new SqlGeographyBuilder();
                var poly2 = new SqlGeographyBuilder();

                poly1.SetSrid(4326);
                poly2.SetSrid(4326);

                poly1.BeginGeography(OpenGisGeographyType.Polygon);
                poly2.BeginGeography(OpenGisGeographyType.Polygon);

                //polygon 1
                //poly1.BeginFigure()
            }
            return JsonConvert.SerializeObject("");
        }

      /*  private SqlGeographyBuilder CreatePolygonFromPoints(List<LatLong> points)
        {
            SqlGeographyBuilder polygon = new SqlGeographyBuilder();
            polygon.SetSrid(4326);

            polygon.BeginGeography(OpenGisGeographyType.Polygon);


        }*/
    }
}