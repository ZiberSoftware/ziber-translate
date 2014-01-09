using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using ZiberTranslate.Web.Models;
using NHibernate.Transform;

namespace ZiberTranslate.Web.Services
{
    public class TranslatorService
    {
        public static Translator FindByEmail(string emailAddress)
        {
            return Global.CurrentSession.QueryOver<Translator>().Where(x => x.EmailAddress == emailAddress).SingleOrDefault();
        }

        public static int FetchRank(string emailAddress)
        {
            return Global.CurrentSession.QueryOver<Translator>()
                .Where(x => x.EmailAddress == emailAddress)
                .Select(x => x.Rank)
                .SingleOrDefault<int>();
        }

        public static void UpdateRank(Translator translator)
        {
            var votes = Global.CurrentSession.QueryOver<TranslationVote>()
                .Where(x => x.Translator == translator)
                .RowCount();

            translator.Rank = votes;

            Global.CurrentSession.Update(translator);
        }
    }
}