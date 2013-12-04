using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ZiberTranslate.Web.Models;
using System.Web.Security;

namespace ZiberTranslate.Web.Controllers
{
    public class SecurityController : BaseController
    {
        public ActionResult Login()
        {
            return View("Login");
        }

        [HttpPost]
        public ActionResult Login(string emailAddress, string password, string returnUrl = "")
        {
            var user = DbSession.QueryOver<Translator>()
                .Where(x => x.EmailAddress == emailAddress).And(x => x.Password == password)
                .SingleOrDefault();

            if (user == null)
            {
                return RedirectToAction("Login");
            }

            FormsAuthentication.SetAuthCookie(emailAddress, false);

            if (!string.IsNullOrWhiteSpace(returnUrl))
            {
                return new RedirectResult(returnUrl);
            }

            return RedirectToAction("Index", "Home");
        }

        public ActionResult Logout()
        { 
          FormsAuthentication.SignOut();
          return RedirectToAction("Login");
        }
    }
}