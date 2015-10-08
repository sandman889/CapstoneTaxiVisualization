using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using System.Web.Configuration;
using System.Threading;
using CapstoneTaxiVisualization.Models;

namespace CapstoneTaxiVisualization
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
            InitializeThreadPool(Int32.Parse(WebConfigurationManager.AppSettings["ThreadPoolSize"]));
        }

        //initialize the threadpool the size specified and allow that many active threads
        protected void InitializeThreadPool(int size)
        {
            ThreadPool.SetMaxThreads(size, size);
        }
    }
}
