using System.Web;
using System.Web.Optimization;

namespace ITMS
{
    public class BundleConfig
    {
        // For more information on bundling, visit https://go.microsoft.com/fwlink/?LinkId=301862
        public static void RegisterBundles(BundleCollection bundles)
        {
            bundles.Add(new ScriptBundle("~/bundles/jquery").Include(
                        "~/Scripts/jquery-{version}.js"));

            bundles.Add(new ScriptBundle("~/bundles/jqueryval").Include(
                        "~/Scripts/jquery.validate*"));

            // Use the development version of Modernizr to develop with and learn from. Then, when you're
            // ready for production, use the build tool at https://modernizr.com to pick only the tests you need.
            bundles.Add(new ScriptBundle("~/bundles/modernizr").Include(
                        "~/Scripts/modernizr-*"));

            bundles.Add(new ScriptBundle("~/bundles/bootstrap", "//cdn.datatables.net/1.10.19/js/jquery.dataTables.min.js").Include(
                      "~/Scripts/bootstrap.js"));

            


            bundles.Add(new StyleBundle("~/Content/css", "//cdn.datatables.net/1.10.19/css/jquery.dataTables.min.css").Include(
                      "~/Content/assets/vendor/bootstrap/css/bootstrap.min.css",
                      "~/Content/assets/vendor/fonts/circular-std/style.css",
                      "~/Content/assets/libs/css/style.css",
                      "~/Content/assets/vendor/fonts/fontawesome/css/fontawesome-all.css",
                      "~/Content/assets/vendor/charts/chartist-bundle/chartist.css",
                      "~/Content/assets/vendor/charts/morris-bundle/morris.css",
                      "~/Content/assets/vendor/fonts/material-design-iconic-font/css/materialdesignicons.min.css",
                      "~/Content/assets/vendor/charts/c3charts/c3.css",
                      "~/Content/assets/vendor/fonts/flag-icon-css/flag-icon.min.css"
                      ));
        }
    }
}
