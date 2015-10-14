using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Threading;
using CapstoneTaxiVisualization.Models;
using Microsoft.SqlServer.Types;
using System.Web.Configuration;
using CapstoneTaxiVisualization.Classes;

namespace CapstoneTaxiVisualization.Controllers
{
    public static class Utilities
    {
        public static int jobCount = 0;

        #region Public Methods
        public static string SerializeJsonThread(IEnumerable<object> data)
        {
            List<SerializeThread> procThread = new List<SerializeThread>();

            //divide the enumerable object into chunks
            foreach (var chunk in data.TakeChunks(Convert.ToInt32(WebConfigurationManager.AppSettings["ThreadPoolSize"])))
            {
                SerializeThread temp = new SerializeThread();
                temp.dataToSerialize = chunk;

                procThread.Add(temp);
            }

            foreach (var proc in procThread)
            {
                ThreadPool.QueueUserWorkItem(new WaitCallback(proc.SerializeObject));
            }

            //block until the threads are initialized
            while (jobCount == 0) { /*blocking*/ }
            //block until the threads have finished
            while (jobCount != 0) { /*blocking*/ }

            return BuildJsonString(procThread.Select(x => x.jsonResult).ToList());
        }
        #endregion

        #region Private Methods
        private static string BuildJsonString(List<string> json)
        {
            string jsonToReturn = String.Empty;
            List<string> cleanedJson = new List<string>();

            foreach (var str in json)
            {
                //remove the first and last character of the array which are the [] characters
                var temp = str.Remove(0, 1);
                temp = temp.Remove(temp.Length - 1, 1);
                cleanedJson.Add(temp);
            }

            //begin the array
            jsonToReturn += "[";

            foreach (var str in cleanedJson)
            {
                jsonToReturn += str + ',';
            }

            //remove the last comma, and close the array
            jsonToReturn = jsonToReturn.Remove(jsonToReturn.Length - 1);
            jsonToReturn += "]";

            return jsonToReturn;
        }
        #endregion
    }
}