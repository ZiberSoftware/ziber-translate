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

namespace ZiberTranslate.Web.Controllers
{ 
    public class User
    {
        public string Username { get; set; }
        public string Email { get; set; }

    }
    public interface ISecurityService
    {
        User Login(string emailAddress, string password);
    }

    /// <summary>
    /// Connects to MediaPublisher database
    /// </summary>
    public class MPsecurityService : ISecurityService
    {        
        public User Login(string emailAddress, string password)
        {
           
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Connects to Translate.mdf and Users.mdf database
    /// </summary>
    public class TranslateSecurityService : ISecurityService
    {
        private readonly int NUMBER_OF_ITERATIONS = 1337;
        private UsersEntities usersDB = new UsersEntities();

        public User Login(string emailAddress, string password)
        {
            var userHasAccount = new User();

            var salt = (from s in usersDB.Members where s.E_mail == emailAddress select s.Salt).SingleOrDefault<string>();
            var hash = (from h in usersDB.Members where h.E_mail == emailAddress select h.Hash).SingleOrDefault<string>();
            var username = (from u in usersDB.Members where u.E_mail == emailAddress select u.Username).SingleOrDefault<string>();

            if (salt == null)
                return userHasAccount = null;

            var saltBytes = Encoding.UTF8.GetBytes(salt);
            var pbkdf2 = new Rfc2898DeriveBytes(password, saltBytes, NUMBER_OF_ITERATIONS);
            var key = pbkdf2.GetBytes(128);
            var hashKey = Convert.ToBase64String(key);

            if (hash == hashKey)
            {
                userHasAccount.Username = username;
                userHasAccount.Email = emailAddress;
                return userHasAccount;
            }
            else
                return userHasAccount = null;

        }
    }
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

            FormsAuthentication.SetAuthCookie(emailAddress, false);
            
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