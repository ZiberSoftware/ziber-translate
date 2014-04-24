﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ZiberTranslate.Web.Models;
using ZiberTranslate.Web.Services;
using NHibernate.Linq;

namespace ZiberTranslate.Web.Controllers
{
   
    public class HomeController : BaseController
    {


        public ActionResult Index()
        {
            if (HttpContext.User.Identity.IsAuthenticated)
            {
                ViewBag.IsAuthenticated = true;
                ViewBag.Username = HttpContext.User.Identity.Name;
                

                var emailAddress = HttpContext.User.Identity.Name;
                var translator = TranslatorService.FindByEmail(emailAddress);

                ViewBag.IsAdmin = translator.IsAdmin;
            }
            else
            {
                ViewBag.IsAuthenticated = false;
            }
            
            
            return View("Index");
        }
       
        //public ActionResult DefaultLanguage(string language="")
        //{
        //    if (language == "")
        //    {
        //        return View("PickLanguage");
        //    }
        //    else
        //    {
        //        var amountTranslations = DbSession.QueryOver<Translation>()
        //                .Where(x => x.Language == LanguageService.GetLanguageByIsoCode(language))
        //                .RowCount();

        //        if (amountTranslations >= 5)
        //        {
        //            using (var t = DbSession.BeginTransaction())
        //            {
        //                var me = TranslatorService.FindByEmail(HttpContext.User.Identity.Name);

        //                me.NeutralLanguage = language;

        //                Global.CurrentSession.Update(me);
        //                t.Commit();
        //            }

        //            return RedirectToAction("Index");
        //        }
        //        else
        //            return View("PickLanguage");
        //    }
            
        //}
    }
}
