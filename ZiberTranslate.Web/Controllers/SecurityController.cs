using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.SqlClient;
using System.Net;
using System.Web;
using System.Web.Mvc;
using ZiberTranslate.Web.Models;
using System.Web.Security;
using System.Security.Cryptography;
using System.Text;
using ZiberTranslate.Web.Services;

namespace ZiberTranslate.Web.Controllers
{ 
    public class SecurityController : BaseController
    {
        private log4net.ILog logger = log4net.LogManager.GetLogger(typeof(SecurityController));
        private readonly ISecurityService securityService;

        public SecurityController(ISecurityService securityService)
        {
            this.securityService = securityService;
        }

        public ActionResult Login()
        {
            return View("Login");
        }

        [HttpPost]
        public ActionResult Login(string emailAddress, string password, string returnUrl = "")
        {
            var userHasAccount = securityService.Login(emailAddress, password);           
            
            if (userHasAccount == null)
                return new HttpStatusCodeResult(HttpStatusCode.Unauthorized);

            var hasTranslateAccount = DbSession.QueryOver<Translator>()
                                      .Where(x => x.EmailAddress == userHasAccount.Email)
                                      .SingleOrDefault();

            if (hasTranslateAccount == null || hasTranslateAccount.IsBlocked)
                return new HttpStatusCodeResult(HttpStatusCode.Unauthorized);

            FormsAuthentication.SetAuthCookie(userHasAccount.Email, false);
            
            return new HttpStatusCodeResult(HttpStatusCode.OK);
        }

        //public ActionResult Signup()
        //{
        //    return View("Signup");
        //}

        //[HttpPost]
        //public ActionResult Signup(string fullName, string emailSignup, string passwordSignup, string confirmPassword)
        //{
        //    var NUMBER_OF_ITERATIONS = 1337;
        //    RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider();
        //    byte[] buff = new byte [64];
        //    rng.GetBytes(buff);
        //    var salt = Convert.ToBase64String(buff);
        //    var saltBytes = Encoding.UTF8.GetBytes(salt);
        //    var pbkdf2 = new Rfc2898DeriveBytes(passwordSignup, saltBytes, NUMBER_OF_ITERATIONS);
        //    var key = pbkdf2.GetBytes(128);
        //    var hashKey = Convert.ToBase64String(key);
        //    var checkUniqueMail = DbSession.QueryOver<Translator>()
        //                        .Where(x => x.EmailAddress == emailSignup)
        //                        .SingleOrDefault();

        //    if (checkUniqueMail == null)
        //    {
        //        if (passwordSignup == confirmPassword)
        //        {
        //            using (var r = DbSession.BeginTransaction())
        //            {
        //                var newTranslator = new Translator();
        //                newTranslator.Name = fullName;
        //                newTranslator.EmailAddress = emailSignup;
        //                newTranslator.Hash = hashKey;
        //                newTranslator.Salt = salt;
        //                newTranslator.Rank = 1;
        //                newTranslator.IsBlocked = false;

        //                Global.CurrentSession.Save(newTranslator);
        //                r.Commit();
        //            }

        //            return View("Succes");
        //        }
        //        else
        //        {
        //            return Content("User input is invalid, please try again");
        //        }
        //    }
        //    else
        //    {
        //        return Content("You allready signed up with this e-mailaddress");
        //    }
        //}

        public ActionResult Logout()
        { 
          FormsAuthentication.SignOut();
          return RedirectToAction("Index", "Home");
        }
    }
}