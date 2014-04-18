using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace ZiberTranslate.Web.App_Start
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");
            routes.IgnoreRoute("Content/{*pathInfo}");
            routes.IgnoreRoute("Scripts/{*pathInfo}");

            routes.MapRoute(
                name: "Approve translation",
                url: "sets/{setId}/translations-{language}/{id}/approve",
                defaults: new { controller = "Translation", action = "Approve", language = "", setId = 0, id = 0 }
            );

            routes.MapRoute(
                name: "Disapprove translation",
                url: "sets/{setId}/translations-{language}/{id}/disapprove",
                defaults: new { controller = "Translation", action = "Disapprove", language = "", setId = 0, id = 0 }
            );

            routes.MapRoute(
                name: "Update translation",
                url: "sets/{setId}/translations-{language}/{id}/update",
                defaults: new { controller = "Translation", action = "Update", language = "", setId = 0, id = 0 }
            );

            routes.MapRoute(
                name: "Destroy translation",
                url: "sets/{setId}/translations-{language}/{id}/destroy",
                defaults: new { controller = "Translation", action = "Destroy", language = "", setId = 0, id = 0 }
            );

            routes.MapRoute(
                name: "Translations",
                url: "sets/{setId}/translations-{language}/filter-{filter}",
                defaults: new { controller = "Translation", action = "Index", language = "", setId = 0, filter = "All" }
            );

            routes.MapRoute(
                name: "AdminTranslations",
                url: "admin/{setId}/review-translations-{language}",
                defaults: new { controller = "Admin", action = "SetContent", language = "", setId = 0 }
            );

            //fallback
            routes.MapRoute(
                "Default", // Route name
                "{controller}/{action}/{id}", // URL with parameters
                new { controller = "Home", action = "Index", id = UrlParameter.Optional } // Parameter defaults
            );
        }
    }
}