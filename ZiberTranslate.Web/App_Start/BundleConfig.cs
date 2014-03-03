using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Optimization;

namespace ZiberTranslate.Web.App_Start
{
    public class BundleConfig
    {
        public static void RegisterBundles(BundleCollection bundles)
        {
            bundles.Add(new ScriptBundle("~/bundles/app.js")
                .Include("~/Scripts/modules.js")
                .IncludeDirectory("~/Scripts/controllers", "*.js")
                .IncludeDirectory("~/Scripts/services", "*.js")
                .IncludeDirectory("~/Scripts/directives", "*.js")
                .Include("~/Scripts/app.js"));

            bundles.Add(new ScriptBundle("~/bundles/libs.js")
                            .Include("~/Scripts/libs/angular.js")
                            .Include("~/Scripts/libs/angular-route.js")
                            .Include("~/Scripts/libs/angular-sanitize.js")
                            .Include("~/Scripts/libs/bindonce.js")
                            .Include("~/Scripts/libs/jquery-1.11.0.js")
                            .Include("~/Scripts/libs/jsdiff.js"));
        }
    }
}