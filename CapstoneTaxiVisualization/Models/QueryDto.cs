using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CapstoneTaxiVisualization.Models
{
    public class QueryDto
    {
        public LatLong Pickup { get; set; }
        public LatLong Dropoff { get; set; }
        public string FareTotal { get; set; }
        public string TravelTime { get; set; }
        public string NumOfPassenger { get; set; }
        public string TripDistance { get; set; }

        public QueryDto()
        {
            Pickup = new LatLong();
            Dropoff = new LatLong();
        }
    }
}