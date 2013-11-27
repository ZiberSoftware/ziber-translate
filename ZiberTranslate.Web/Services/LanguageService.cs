using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using ZiberTranslate.Web.Models;

namespace ZiberTranslate.Web.Services
{
    public class LanguageService
    {
        public static Language GetNeutralLanguage()
        {
            return GetLanguageByIsoCode("nl");
        }

        public static Language GetLanguageByIsoCode(string isoCode)
        {
            return Global.CurrentSession.QueryOver<Language>()
                .Where(x => x.IsoCode == isoCode)
                .SingleOrDefault();
        }
    }
}