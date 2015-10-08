using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CapstoneTaxiVisualization.Models
{
    public class RegionQueryModel
    {
        public DateTime startDate { get; set; }
        public DateTime endDate { get; set; }
        public List<LatLong> boundPoints { get; set; }
        public string jsonResult { get; set; }

        public RegionQueryModel()
        {
            boundPoints = new List<LatLong>();
            jsonResult = String.Empty;
        }
    }
}