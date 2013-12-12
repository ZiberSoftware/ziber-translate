using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using NHibernate.Linq;

namespace ZiberTranslate.Web.Controllers
{
    //[Authorize]
    public class HomeController : BaseController
    {
        public ActionResult Index()
        {
            using (var t = DbSession.BeginTransaction())
            {
                var translators = DbSession.Query<Models.Translator>().ToList();

                t.Commit();
            }
            return View("Index");
        }

    }
}
