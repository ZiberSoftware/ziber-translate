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

        [HttpPost]
        public ActionResult Login(string emailAddress, string password)
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

        [HttpPost]
        public ActionResult Logout()
        { 
            FormsAuthentication.SignOut();
          
            return new HttpStatusCodeResult(200);
        }

        [HttpGet]
        public ActionResult TranslatorRank()
        {
            var emailAddress = HttpContext.User.Identity.Name;
            var translator = TranslatorService.FindByEmail(emailAddress);

            if (translator != null)
            {
                return Json(new
                {
                    rank = translator.Rank
                }, JsonRequestBehavior.AllowGet);
            }
            else
                return new EmptyResult();
        }
    }
}