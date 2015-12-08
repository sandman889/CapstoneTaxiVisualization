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
                SqlGeography sqlPoly = CreatePolygonFromPoints(boundPoints);

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
                //build the centroid out of the latlong object given
                SqlGeography sqlPoint = CreatePoint(centroid);

                //kick off the stored procedure and return the data
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
                //create the point to look for given by the latlong object
                SqlGeography sqlPoint = CreatePoint(point);

                //initialize return list and the initial distance to search in
                List<QueryDto> returnVal = new List<QueryDto>();
                int initialDistance = 10;

                //starting from the initialDistance search for a trip and expand the search distance until 
                //a point is found. The max is 100000 meters which is around 62 miles which is sufficient for this project
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
                //build the line from the points given
                SqlGeography sqlLine = CreateLineFromPoints(linePoints);

                //kick off the stored procedure
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
                //build the line from the points given
                SqlGeography sqlLine = CreateLineFromPoints(linePoints);

                //kick off the stored procedure
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
                SqlGeography polygon1 = CreatePolygonFromPoints(regionOnePoints);
                SqlGeography polygon2 = CreatePolygonFromPoints(regionTwoPoints);

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

        [HttpPost]
        public string DualRegionQueryCircle(DateTime startDate, DateTime endDate, LatLong pointOne, double radiusOne, LatLong pointTwo, double radiusTwo)
        {
            using (TaxiDataEntities context = new TaxiDataEntities())
            {
                SqlGeography centroidOne = CreatePoint(pointOne);
                SqlGeography centroidTwo = CreatePoint(pointTwo);

                List<QueryDto> returnVal = context.TwoRegionQueryCircle(startDate, endDate, radiusOne, centroidOne.ToString(), radiusTwo, centroidTwo.ToString())
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
        private SqlGeography CreatePolygonFromPoints(List<LatLong> points)
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

            return polygon.ConstructedGeography;
        }

        /// <summary>
        /// Create a SqlGeography instance for the point specified by the LatLong object
        /// </summary>
        /// <param name="point">LatLong object representing the point</param>
        /// <returns>SqlGeography point</returns>
        private SqlGeography CreatePoint(LatLong point)
        {
            SqlGeographyBuilder geoPoint = new SqlGeographyBuilder();

            //set the SRID and build the sql geography point for the centroid
            geoPoint.SetSrid(4326);
            geoPoint.BeginGeography(OpenGisGeographyType.Point);
            geoPoint.BeginFigure(point.Latitude, point.Longitude);
            geoPoint.EndFigure();
            geoPoint.EndGeography();

            return geoPoint.ConstructedGeography;
        }

        /// <summary>
        /// Create the SqlGeography line string from the points given
        /// </summary>
        /// <param name="linePoints">List of LatLong that represents the start and end of the line</param>
        /// <returns>SqlGeography line string</returns>
        private SqlGeography CreateLineFromPoints(List<LatLong> linePoints)
        {
            SqlGeographyBuilder geoLine = new SqlGeographyBuilder();

            //build the sql geography representation of the linestring
            geoLine.SetSrid(4326);
            geoLine.BeginGeography(OpenGisGeographyType.LineString);
            geoLine.BeginFigure(linePoints.First().Latitude, linePoints.First().Longitude);
            geoLine.AddLine(linePoints.Last().Latitude, linePoints.Last().Longitude);
            geoLine.EndFigure();
            geoLine.EndGeography();

            return geoLine.ConstructedGeography;
        }
    }
}