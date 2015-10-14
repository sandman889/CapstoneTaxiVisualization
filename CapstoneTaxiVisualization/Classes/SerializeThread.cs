using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Newtonsoft.Json;
using System.Threading;
using CapstoneTaxiVisualization.Controllers;

namespace CapstoneTaxiVisualization.Classes
{
    public class SerializeThread
    {
        public object dataToSerialize { get; set; }
        public string jsonResult { get; set; }

        public void SerializeObject(Object state)
        {
            Interlocked.Increment(ref Utilities.jobCount);
            jsonResult = JsonConvert.SerializeObject(dataToSerialize);
            Interlocked.Decrement(ref Utilities.jobCount);
        }
    }
}