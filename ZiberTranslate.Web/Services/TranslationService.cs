using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Net.Mail;
using System.Configuration;
using ZiberTranslate.Web.Models;
using NHibernate.Criterion;

namespace ZiberTranslate.Web.Services
{
    public class TranslationService
    {
        public static Translation FindById(int id)
        {
            return Global.CurrentSession.QueryOver<Translation>().Where(x => x.Id == id).SingleOrDefault();
        }


        public static Translation FindByKeyForTranslator(int id, string language, Translator translator)
        {
            var crit = DetachedCriteria.For<Language>()
                .Add(Restrictions.Eq("IsoCode", language))
                .SetProjection(Projections.Id());

            return Global.CurrentSession.CreateCriteria<Translation>()
                .Add(Subqueries.PropertyEq("Language", crit))
                .Add(Restrictions.Eq("Translator", translator))
                .CreateAlias("Key", "k")
                .Add(Restrictions.Eq("k.Id", id))
                .UniqueResult<Translation>();
        }

        public static Translation FindBySet(int setId, string language)
        {
            var crit = DetachedCriteria.For<Language>()
                .Add(Restrictions.Eq("IsoCode", language))
                .SetProjection(Projections.Id());

            return Global.CurrentSession.CreateCriteria<Translation>()
                .Add(Subqueries.PropertyEq("Language", crit))
                //.Add(Restrictions.IsNull("Translator"))
                .CreateAlias("Key", "k")
                .Add(Restrictions.Eq("k.Set", setId))
                .UniqueResult<Translation>();
        }

        public static Translation FindByKey(int id, string language)
        {
            var crit = DetachedCriteria.For<Language>()
                .Add(Restrictions.Eq("IsoCode", language))
                .SetProjection(Projections.Id());

            return Global.CurrentSession.CreateCriteria<Translation>()
                .Add(Subqueries.PropertyEq("Language", crit))
                .Add(Restrictions.IsNull("Translator"))
                .CreateAlias("Key", "k")
                .Add(Restrictions.Eq("k.Id", id))
                .UniqueResult<Translation>();
        }

        public static Translation UpdateTranslation(int id, string language, string value)
        {
            string emailAddress = HttpContext.Current.User.Identity.Name;
            var translator = TranslatorService.FindByEmail(emailAddress);
            var translatorRank = TranslatorService.FetchRank(emailAddress);
            var translation = TranslationService.FindByKeyForTranslator(id, language, translator);
            var leading = TranslationService.FindByKey(id, language);

            if (translation != null)
            {
                if (leading != null && leading.Value == value)
                {
                    Global.CurrentSession.Delete(translation);
                }
                else
                {
                    translation.Value = value;
                    translation.NeedsReview = true;

                    Global.CurrentSession.Update(translation);
                }
            }
            else if (!string.IsNullOrWhiteSpace(value))
            {

                if (leading == null || value != leading.Value)
                {
                    translation = new Translation();
                    translation.Key = Global.CurrentSession.Load<TranslateKey>(id);
                    translation.Language = LanguageService.GetLanguageByIsoCode(language);
                    translation.Value = value;
                    translation.NeedsAdminReviewing = true;
                    translation.IsPublished = false;
                    translation.NeedsReview = true;
                    translation.Translator = translator;

                    Global.CurrentSession.Save(translation);
                }
                else { translation = leading; }

            }
            return translation;
        }

        public static void SendEmail()
        {
            string emailAddress = HttpContext.Current.User.Identity.Name;

            MailMessage mail = new MailMessage();
            mail.To.Add(emailAddress);
            mail.Subject = "Thanks for your contribution!";
            mail.Body = "Thank you for translating or reviewing on ziber.translate.nl. You will be notified when your changes are accepted and made public. \n" + "Kind regards, the Ziber team.";
            SmtpClient smtp = new SmtpClient();
            smtp.Send(mail);
        }
    }
}