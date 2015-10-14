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
                        "~/Scripts/jquery/jquery-{version}.js",
                        "~/Scripts/jquery-ui-1.11.4.min.js"));

            bundles.Add(new ScriptBundle("~/bundles/jqueryval").Include(
                        "~/Scripts/jquery/jquery.validate*"));

            bundles.Add(new ScriptBundle("~/bundles/bootstrap").Include(
                      "~/Scripts/bootstrap/bootstrap.js"));

            bundles.Add(new ScriptBundle("~/bundles/visualization").Include(
                    "~/Scripts/map/d3.min.js",
                    "~/Scripts/map/leaflet-src.js",
                    "~/Scripts/map/leaflet.js",
                    "~/Scripts/map/mapbox.js",
                    "~/Scripts/map/drawMap.js"
                ));

            bundles.Add(new ScriptBundle("~/bundles/leaflet-draw")
                    .IncludeDirectory("~/Scripts/map/leaflet-draw/src", "*.js", false)
                    .IncludeDirectory("~/Scripts/map/leaflet-draw/dist", "*.js", false)
                    .IncludeDirectory("~/Scripts/map/leaflet-draw/src/draw", "*.js", false)
                    .IncludeDirectory("~/Scripts/map/leaflet-draw/src/draw/handler", "*.js", false)
                    .IncludeDirectory("~/Scripts/map/leaflet-draw/src/edit", "*.js", false)
                    .IncludeDirectory("~/Scripts/map/leaflet-draw/src/edit/handler", "*.js", false)
                    .IncludeDirectory("~/Scripts/map/leaflet-draw/src/ext", "*.js", false)
                    );

            bundles.Add(new StyleBundle("~/Content/css").Include(
                      "~/Content/site.css",
                      "~/Content/leaflet.css",
                      "~/Content/mapbox.css",
                      "~/Content/leaflet.draw.css",
                      "~/Content/themes/base/all.css"));
        }
    }
}
