using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ZiberTranslate.Web.Models;
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

            return Json(from s in sets
                        select new
                        {
                            id = s.Id,
                            name = s.Name,
                            language = language
                        }, JsonRequestBehavior.AllowGet);
        }
    }
}