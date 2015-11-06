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
                        "~/Scripts/jquery/jquery-{version}.js"
            ));

            bundles.Add(new ScriptBundle("~/bundles/jqueryval").Include(
                        "~/Scripts/jquery/jquery.validate*"));

            bundles.Add(new ScriptBundle("~/bundles/visualization").Include(
                    "~/Scripts/map/d3.min.js",
                    "~/Scripts/map/leaflet-src.js",
                    "~/Scripts/map/leaflet.js",
                    "~/Scripts/map/mapbox.js",
                    "~/Scripts/map/drawMap.js",
                    "~/Scripts/map/geojson-rewind.js",
                    "~/Scripts/TaxiVizUtil.js",
                    "~/Scripts/InterfaceInitialization.js",
                    "~/Scripts/kendo.all.min.js",
                    "~/Scripts/map/NewYorkBoroughs.js",
                    "~/Scripts/d3.parcoords.js",
                    "~/Scripts/DemoChartData.js"
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
                      "~/Content/themes/base/all.css",
                      "~/Content/pure-min.css",
                      "~/Content/d3.parcoords.css"
                      ));

            bundles.Add(new StyleBundle("~/Content/kendocss").Include(
                "~/Content/kendo/kendo.common.min.css",              
                "~/Content/kendo/kendo.metroblack.min.css",
                "~/Content/kendo/kendo.metroblack.mobile.min.css"
                ));
        }
    }
}
