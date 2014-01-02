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

            if (language == "en")
                return View("EnglishTranslate", vm);
            else if (language == "de")
                return View("GermanTranslate", vm);
            else 
                return View("Index");
        }
    }
}