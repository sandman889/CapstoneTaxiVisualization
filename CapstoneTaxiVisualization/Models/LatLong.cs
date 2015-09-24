using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CapstoneTaxiVisualization.Models
{
    public class LatLong
    {
        public double Latitude { get; set; }
        public double Longitude { get; set; }

        public LatLong(double lat, double lng)
        {
            Latitude = lat;
            Longitude = lng;
        }
    }
}