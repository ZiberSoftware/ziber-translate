using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using FluentNHibernate.Cfg;
using FluentNHibernate.Cfg.Db;
using NHibernate;
using NHibernate.Tool.hbm2ddl;
using Castle.Windsor;
using Castle.Windsor.Installer;

namespace ZiberTranslate.Web
{
    // Note: For instructions on enabling IIS6 or IIS7 classic mode, 
    // visit http://go.microsoft.com/?LinkId=9394801

    public class Global : System.Web.HttpApplication
    {
        public static ISessionFactory SessionFactory = CreateSessionFactory();

        public static ISession CurrentSession
        {
            get { return (ISession)HttpContext.Current.Items["current.session"]; }
            set { HttpContext.Current.Items["current.session"] = value; }
        }
        
        protected Global()
        {
            BeginRequest += delegate
            {
                CurrentSession = SessionFactory.OpenSession();
            };
            EndRequest += delegate
            {
                if (CurrentSession != null)
                    CurrentSession.Dispose();
            };
        }

        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new HandleErrorAttribute());
        }


        private static void BuildSchema(NHibernate.Cfg.Configuration config)
        {
            var export = new SchemaExport(config);
            export.SetOutputFile(@"d:\output.sql");
            export.Execute(true, false, false);
        }

        private static ISessionFactory CreateSessionFactory()
        {
            var cfg = Fluently.Configure()
                .Database(MsSqlConfiguration.MsSql2008.ConnectionString(x => x.FromConnectionStringWithKey("Resources")))
                .Mappings(x => x.FluentMappings.AddFromAssemblyOf<Models.Translation>())
                .BuildConfiguration();

#if DEBUG
            BuildSchema(cfg);
#endif

            return cfg.BuildSessionFactory();
        }

        protected void Application_Start()
        {
            log4net.Config.XmlConfigurator.Configure();

            var container = new WindsorContainer();
            container.Install(FromAssembly.This());

            AreaRegistration.RegisterAllAreas();

            RegisterGlobalFilters(GlobalFilters.Filters);
            RegisterRoutes(RouteTable.Routes);
        }

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
                url: "sets/{setId}/translations-{language}",
                defaults: new { controller = "Translation", action = "Index", language = "", setId = 0 }
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