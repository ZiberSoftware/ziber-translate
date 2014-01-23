using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ZiberTranslate.Web.Models;
using ZiberTranslate.Web.ViewModels;
using NHibernate;

namespace ZiberTranslate.Web.Controllers
{
    //[Authorize]
    public class TranslateSetController : BaseController
    {
        public ActionResult Index(string language = "")
        {
            var sets = Global.CurrentSession.QueryOver<TranslateSet>()
                        .List();

            var vm = new TranslateSetsViewModel();
            vm.Sets = sets;
            vm.Language = language;

            if (!string.IsNullOrWhiteSpace(language))
                return View("TranslateSets", vm);
            else
                return View("Index");
        }
    }
}