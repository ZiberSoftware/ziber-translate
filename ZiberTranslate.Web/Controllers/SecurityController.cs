using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ZiberTranslate.Web.Models;
using System.Web.Security;
using System.Security.Cryptography;
using System.Text;

namespace ZiberTranslate.Web.Controllers
{
    public class SecurityController : BaseController
    {
        public ActionResult Login()
        {
            return View("Login");
        }

        /*[HttpPost]
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
        */
 
        [HttpPost]
        public ActionResult Login(string emailAddress, string password, string returnUrl = "")
        {
            var NUMBER_OF_ITERATIONS = 1337;
            var salt = "c4x2J66hVK2ptF6MrrEG3h6yTGCUTtZ6U6erf+rTFvfLjiNqBrr0vzTyCre7ZLZTP430YBoC1+97MSsyAX6wUzc=";
            var saltBytes = Encoding.UTF8.GetBytes(salt);
            var pbkdf2 = new Rfc2898DeriveBytes(password, saltBytes, NUMBER_OF_ITERATIONS);
            var key = pbkdf2.GetBytes(128);
            var hashKey = Convert.ToBase64String(key);
            var hash = DbSession.QueryOver<Translator>()
                .Where(x => x.EmailAddress == emailAddress).And(x => x.Hash == hashKey)
                .SingleOrDefault();

            if (hash == null)
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
          return RedirectToAction("Index", "Home");
        }
    }
}