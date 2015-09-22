using System.Web;
using System.Web.Optimization;

namespace CapstoneTaxiVisualization
{
    public class BundleConfig
    {
        // For more information on bundling, visit http://go.microsoft.com/fwlink/?LinkId=301862
        public static void RegisterBundles(BundleCollection bundles)
        {
            bundles.Add(new ScriptBundle("~/bundles/jquery").Include(
                        "~/Scripts/jquery/jquery-{version}.js"));

            bundles.Add(new ScriptBundle("~/bundles/jqueryval").Include(
                        "~/Scripts/jquery/jquery.validate*"));

            bundles.Add(new ScriptBundle("~/bundles/bootstrap").Include(
                      "~/Scripts/bootstrap/bootstrap.js"));

            bundles.Add(new ScriptBundle("~/bundles/visualization").Include(
                    "~/Scripts/map/d3.min.js",
                    "~/Scripts/map/leaflet.js",
                    "~/Scripts/map/mapbox.js",
                    "~/Scripts/map/drawMap.js"
                ));

            bundles.Add(new StyleBundle("~/Content/css").Include(
                      "~/Content/site.css",
                      "~/Content/leaflet.css",
                      "~/Content/mapbox.css"));
        }
    }
}
