using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using FluentNHibernate.Cfg;
using FluentNHibernate.Cfg.Db;
using NHibernate;
using NHibernate.Tool.hbm2ddl;
using Castle.Windsor;
using Castle.Windsor.Installer;
using ZiberTranslate.Web.App_Start;
using Castle.MicroKernel.Registration;
using ZiberTranslate.Web.Controllers;

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


        //private static void BuildSchema(NHibernate.Cfg.Configuration config)
        //{
        //    var export = new SchemaExport(config);
        //    export.SetOutputFile(@"c:\output.sql");
        //    //export.SetOutputFile(@"C:\Users\Gebruiker\Desktop\Kilian's stuff\ZiberTranslate\ziber-translate\output.sql");
        //    export.Execute(true, false, false);
        //}

        private static ISessionFactory CreateSessionFactory()
        {
            var cfg = Fluently.Configure()
                .Database(MsSqlConfiguration.MsSql2000.ConnectionString(x => x.FromConnectionStringWithKey("Resources")))
                .Mappings(x => x.FluentMappings.AddFromAssemblyOf<Models.Translation>())
                .BuildConfiguration();

#if DEBUG
            //BuildSchema(cfg);
#endif

            return cfg.BuildSessionFactory();
        }

        protected void Application_Start()
        {
            log4net.Config.XmlConfigurator.Configure();

            var container = new WindsorContainer();
            container.Install(FromAssembly.This());
          
            container.Register(
                Component.For<ISecurityService>().ImplementedBy<TranslateSecurityService>()
            );

            AreaRegistration.RegisterAllAreas();

            RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
            
        }

    }
}