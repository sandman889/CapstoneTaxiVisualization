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
                //build the geography polygon from the points
                SqlGeographyBuilder geoPoly = CreatePolygonFromPoints(boundPoints);

                //the final SQL polygon element
                SqlGeography sqlPoly = geoPoly.ConstructedGeography;

                //run the stored procedure and make a list of the data
                List<QueryDto> returnVal = context.RegionQueryPoly(startDate, endDate, sqlPoly.ToString(), queryFor)
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
                SqlGeographyBuilder geoPoint = new SqlGeographyBuilder();

                //set the SRID and build the sql geography point for the centroid
                geoPoint.SetSrid(4326);
                geoPoint.BeginGeography(OpenGisGeographyType.Point);
                geoPoint.BeginFigure(centroid.Latitude, centroid.Longitude);
                geoPoint.EndFigure();
                geoPoint.EndGeography();

                SqlGeography sqlPoint = geoPoint.ConstructedGeography;


                List<QueryDto> returnVal = context.RegionQueryCircle(startDate, endDate, radius, sqlPoint.ToString(), queryFor)
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
                SqlGeographyBuilder geoPoint = new SqlGeographyBuilder();

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

                return JsonConvert.SerializeObject(new QueryDto[] {returnVal.First()});
            }
        }

        [HttpPost]
        public string GetTripIntersectionByLine(DateTime startDate, DateTime endDate, List<LatLong> linePoints)
        {
            using (TaxiDataEntities context = new TaxiDataEntities())
            {
                SqlGeographyBuilder geoLine = new SqlGeographyBuilder();

                geoLine.SetSrid(4326);
                geoLine.BeginGeography(OpenGisGeographyType.LineString);
                geoLine.BeginFigure(linePoints.First().Latitude, linePoints.First().Longitude);
                geoLine.AddLine(linePoints.Last().Latitude, linePoints.Last().Longitude);
                geoLine.EndFigure();
                geoLine.EndGeography();

                SqlGeography sqlLine = geoLine.ConstructedGeography;

                List<QueryDto> returnVal = context.LinesIntersectionQuery(startDate, endDate, sqlLine.ToString())
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
        public string GetTripsOnLine(DateTime startDate, DateTime endDate, List<LatLong> linePoints)
        {
            using (TaxiDataEntities context = new TaxiDataEntities())
            {
                SqlGeographyBuilder geoLine = new SqlGeographyBuilder();

                //build the sql geography representation of the linestring
                geoLine.SetSrid(4326);
                geoLine.BeginGeography(OpenGisGeographyType.LineString);
                geoLine.BeginFigure(linePoints.First().Latitude, linePoints.First().Longitude);
                geoLine.AddLine(linePoints.Last().Latitude, linePoints.Last().Longitude);
                geoLine.EndFigure();
                geoLine.EndGeography();

                SqlGeography sqlLine = geoLine.ConstructedGeography;

                List<QueryDto> returnVal = context.LineWithVolume(startDate, endDate, sqlLine.ToString())
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
                SqlGeographyBuilder poly1 = CreatePolygonFromPoints(regionOnePoints);
                SqlGeographyBuilder poly2 = CreatePolygonFromPoints(regionTwoPoints);

                //grab the constructed geography for each polygon
                SqlGeography polygon1 = poly1.ConstructedGeography;
                SqlGeography polygon2 = poly2.ConstructedGeography;

                List<QueryDto> returnVal = context.TwoRegionQueryPoly(startDate, endDate, polygon1.ToString(), polygon2.ToString())
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

        /// <summary>
        /// Create a SqlGeography builder for the polygon specified by the points
        /// </summary>
        /// <param name="points">Latitude longitude points for the polygon</param>
        /// <returns>SqlGeographyBuilder for the polygon</returns>
        private SqlGeographyBuilder CreatePolygonFromPoints(List<LatLong> points)
        {
            SqlGeographyBuilder polygon = new SqlGeographyBuilder();
            polygon.SetSrid(4326);

            polygon.BeginGeography(OpenGisGeographyType.Polygon);

            //set the initial point to skip, and use that to begin the figure
            LatLong initialPoint = points.First();
            polygon.BeginFigure(initialPoint.Latitude, initialPoint.Longitude);

            foreach (var point in points)
            {
                if (point != initialPoint)
                {
                    //add each point to the geography poly
                    polygon.AddLine(point.Latitude, point.Longitude);
                }
            }

            //end the configuration of the geography
            polygon.EndFigure();
            polygon.EndGeography();

            return polygon;
        }
    }
}