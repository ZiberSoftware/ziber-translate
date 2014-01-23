using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ZiberTranslate.Web.Models;
using ZiberTranslate.Web.ViewModels;
using ZiberTranslate.Web.Services;
using NHibernate.Linq;

namespace ZiberTranslate.Web.Controllers
{
    //[Authorize]
    public class HomeController : BaseController
    {
        public ActionResult Index()
        {
            var rank = TranslatorService.FetchRank(HttpContext.User.Identity.Name);
            var vm = new TranslationsViewModel();
            vm.Rank = rank;

            using (var t = DbSession.BeginTransaction())
            {
                var translators = DbSession.Query<Models.Translator>().ToList();

                t.Commit();
            }
           

            return View("Index", vm);
        }

    }
}
