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
        public ActionResult Index()
        {
            //var sets = Global.CurrentSession.QueryOver<TranslateSet>()
            //            .List();

            //var vm = new TranslateSetsViewModel();
            //vm.Sets = sets;

            return View("Index");
        }

        public ActionResult English()
        {
            var sets = Global.CurrentSession.QueryOver<TranslateSet>()
                        .List();

            var vm = new TranslateSetsViewModel();
            vm.Sets = sets;

            return View("EnglishTranslate", vm);
        }

        public ActionResult German()
        {
            var sets = Global.CurrentSession.QueryOver<TranslateSet>()
                        .List();

            var vm = new TranslateSetsViewModel();
            vm.Sets = sets;

            return View("GermanTranslate",vm);
        }
    }
}