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
using CapstoneTaxiVisualization.Classes;

namespace CapstoneTaxiVisualization.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public string GetPointsInPolygonRegion(DateTime startDate, DateTime endDate, List<LatLong> boundPoints)
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

                var returnVal = context.GetPointsFromInsideRegion(startDate, endDate, sqlPoly.ToString()).Select(x => new LatLong(Convert.ToDouble(x.pickup_latitude), Convert.ToDouble(x.pickup_longitude))).ToList();

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