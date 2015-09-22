using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Newtonsoft.Json;
using Microsoft.SqlServer.Types;
using CapstoneTaxiVisualization.Models;

namespace CapstoneTaxiVisualization.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }

        public string GetNearestNeighbor(LatLong point)
        {
            //run the sql query to get the nearest neighbor within a certain distance, increase the distance by powers of two until one is found
            var test = "";

            return JsonConvert.SerializeObject(test);
        }

        [HttpPost]
        public string GetPointsInPolygonRegion(List<LatLong> boundPoints)
        {
            var geoPoly = new SqlGeographyBuilder();

            //set the SRID and chose the sql geography datatype
            geoPoly.SetSrid(4326);
            geoPoly.BeginGeography(OpenGisGeographyType.Polygon);

            foreach (var point in boundPoints)
            {
                //add each point to the geography poly
                geoPoly.AddLine(point.Latitude, point.Longitude);
            }

            //add the first point again because it will complete the polygon
            geoPoly.AddLine(boundPoints.First().Latitude, boundPoints.First().Longitude);

            //end the configuration of the geography
            geoPoly.EndFigure();
            geoPoly.EndGeography();

            SqlGeography sqlPoly = geoPoly.ConstructedGeography;

            //todo, not this, actually call a stored procedure
            return sqlPoly.ToString();
        }
    }
}