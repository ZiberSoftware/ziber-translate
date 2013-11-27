using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Castle.MicroKernel.Registration;
using System.Web.Mvc;
using NHibernate;

namespace ZiberTranslate.Web.IoC
{
    public class CoreWindsorInstaller : IWindsorInstaller
    {
        public void Install(Castle.Windsor.IWindsorContainer container, Castle.MicroKernel.SubSystems.Configuration.IConfigurationStore store)
        {
            container.Register(AllTypes.FromThisAssembly().BasedOn<IController>().WithServiceSelf().LifestyleTransient());
            container.Register(
                Component.For<ISession>().UsingFactoryMethod(x => Global.CurrentSession).LifestyleTransient()
            );

            System.Web.Mvc.ControllerBuilder.Current.SetControllerFactory(new WindsorControllerFactory(container));
        }
    }
}