using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using ZiberTranslate.Web.Models;

namespace ZiberTranslate.Web.Services
{
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
}