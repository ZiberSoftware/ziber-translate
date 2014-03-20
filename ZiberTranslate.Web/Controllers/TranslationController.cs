using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Globalization;
using ZiberTranslate.Web.Models;
using ZiberTranslate.Web.Services;
using NHibernate.Transform;
using NHibernate.Criterion;

namespace ZiberTranslate.Web.Controllers
{
    public enum FilterType
    {
        All,
        NeedsReview,
        NeedsTranslation,
        Reviewed
    }

    public enum TranslationType
    {
        Neutral,
        Leading,
        User
    }

    public class TranslationController : BaseController
    {

        private log4net.ILog logger = log4net.LogManager.GetLogger(typeof(TranslationController));

        public ActionResult Index(int setId, string language, FilterType filter, int pageNr = 1, int? categoryId = null)
        {
            double totalPages = TranslateSetService.GetCounter(setId, language, filter) / 20;
            totalPages = (int)Math.Ceiling(totalPages);

            if (pageNr > totalPages)
                pageNr = (int)totalPages;

            var translations = BuildTranslations(setId, language, filter, pageNr);

            try
            {
                return Json(translations, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                logger.Error(ex.Message);
                return new EmptyResult();
            }

        }

        public ActionResult Filters(int setId, string language)
        {
            return Json(new
            {
                reviewed = TranslateSetService.GetCounter(setId, language, FilterType.Reviewed),
                needsReview = TranslateSetService.GetCounter(setId, language, FilterType.NeedsReview),
                needsTranslation = TranslateSetService.GetCounter(setId, language, FilterType.NeedsTranslation),
                total = TranslateSetService.GetCounter(setId, language, FilterType.All)
            }, JsonRequestBehavior.AllowGet);
        }


        private IEnumerable<Translation.TranslationDTO> BuildTranslations(int id, string language, FilterType filter, int pageNr)
        {
            var me = TranslatorService.FindByEmail(HttpContext.User.Identity.Name);
            var keys = TranslationService.FilteredKeys(id, language, filter, pageNr);
            var targetLanguage = LanguageService.GetLanguageByIsoCode(language);

            //fallback to dutch for anonymous users
            var neutralUserLanguage = LanguageService.GetLanguageByIsoCode("nl");
            if (HttpContext.User.Identity.IsAuthenticated)
            {
                neutralUserLanguage = LanguageService.GetNeutralLanguage(HttpContext.User.Identity.Name);
            }

            var neutralUserTranslations = TranslationService.GetTranslations(neutralUserLanguage, keys, TranslationType.Neutral);
            var neutralTranslations = TranslationService.GetTranslations(LanguageService.GetLanguageByIsoCode("nl"), keys, TranslationType.Neutral);
            var leadingTranslations = TranslationService.GetTranslations(targetLanguage, keys, TranslationType.Leading);
            var userTranslations = TranslationService.GetTranslations(targetLanguage, keys, TranslationType.User);

            var dc = DetachedCriteria.For<TranslationVote>()
                         .Add(Restrictions.Eq("Translator", me))
                         .CreateAlias("Translation", "t")
                         .Add(Restrictions.Eq("t.Language", targetLanguage))
                         .SetProjection(Projections.Property("t.Key.Id"));

            var votedOnKeys = DbSession.CreateCriteria<TranslateKey>()
                        .Add(Restrictions.Eq("Set.Id", id))
                        .Add(Subqueries.PropertyIn("Id", dc))
                        .SetProjection(Projections.Id())
                        .Future<int>().ToList();

            var translations = (
                        from key in keys
                        let neutralUserTranslation = neutralUserTranslations.Where(x => x.Key == key).FirstOrDefault()
                        let neutralTranslation = neutralTranslations.Where(x => x.Key == key).FirstOrDefault()
                        let leadingTranslation = leadingTranslations.Where(x => x.Key == key).FirstOrDefault()
                        let userTranslation = userTranslations.Where(x => x.Key == key).FirstOrDefault() ?? leadingTranslation
                        select new Translation.TranslationDTO
                        {
                            KeyId = key.Id,
                            Term = neutralUserTranslation == null ? neutralTranslation.Value : neutralUserTranslation.Value,
                            Value = userTranslation == null ? string.Empty : userTranslation.Value,
                            LeadingValue = leadingTranslation == null ? neutralTranslation.Value : leadingTranslation.Value,
                            Votes = leadingTranslation == null ? 0 : userTranslation.Votes,
                            Voted = votedOnKeys.Contains(key.Id),
                            SetId = id
                        }
                    ).ToList();


            return translations;

        }

        [HttpPost]
        public ActionResult Update(int id, string language, string value)
        {
            Translation translation;

            using (var t = DbSession.BeginTransaction())
            {
                translation = TranslationService.UpdateTranslation(id, language, value);

                t.Commit();
            }
            return CountChanges();
        }


        [HttpPost]
        public ActionResult Destroy(int id, string language)
        {
            var translator = TranslatorService.FindByEmail(HttpContext.User.Identity.Name);
            var translation = TranslationService.FindByKeyForTranslator(id, language, translator);
            if (translation == null)
                return HttpNotFound();

            if (translation.Translator != translator)
                return new HttpUnauthorizedResult();

            if (translation.IsPublished == false)
            {
                int setId = translation.Key.Set.Id;
                int keyId = translation.Key.Id;

                using (var t = DbSession.BeginTransaction())
                {
                    DbSession.Delete(translation);

                    t.Commit();
                }
            }

            return new EmptyResult();
        }

        [HttpPost]
        public ActionResult Approve(int id, string language)
        {
            var translation = TranslationService.FindByKey(id, language);

            TranslationVoteService.Vote(translation);

            return new EmptyResult();
        }

        [HttpPost]
        public ActionResult Disapprove(int id, string language)
        {
            Translation translation;
            using (var t = Global.CurrentSession.BeginTransaction())
            {
                translation = TranslationService.FindByKey(id, language);

                TranslationVoteService.RemoveVote(translation);

                t.Commit();
            }

            return new EmptyResult();
        }

        public ActionResult CountChanges()
        {
            var me = TranslatorService.FindByEmail(HttpContext.User.Identity.Name);

            var changes = TranslationService.GetChangesForTranslator(me);

            var votes = DbSession.QueryOver<TranslationVote>()
                .Where(x => x.IsPublished == false)
                .And(x => x.Translator == me)
                .Future();

            return Json(new
            {
                votes = votes.Count(),
                changes = changes.Count()
            },
                JsonRequestBehavior.AllowGet);
        }

        // this way the name and isocode can be found, front-end needs a way to make the list of languages chooseable options
        // which can then be used as variable for the AddLanguage function.

        public ActionResult LanguageList()
        {
            CultureInfo[] cultures = System.Globalization.CultureInfo.GetCultures(CultureTypes.SpecificCultures);

            var languageList = (
                        from culture in cultures
                        orderby culture.Name
                        select new Language.LanguageList
                        {
                            Name = culture.DisplayName,
                            IsoCode = culture.Name
                        }
                        ).ToList();

            return Json(languageList, JsonRequestBehavior.AllowGet);
        }

        public ActionResult AddLanguage(string ISOCode)
        {
            using (var t = DbSession.BeginTransaction())
            {
                var language = new Language();
                language.IsoCode = ISOCode;
                language.IsEnabled = true;

                DbSession.Save(language);
                t.Commit();
            }
            return new EmptyResult();
        }

        //public ActionResult SearchByName(int id, string language = "", string searchString = "")
        //{
        //    var searchByKey = TranslationService.FindByKey(id, language);
        //    var searchByName = DbSession.QueryOver<Translation>()
        //                       .Where(x => x.Value.Contains(searchString))
        //                       .SingleOrDefault();

        //    if (!String.IsNullOrEmpty(searchString))
        //    {
        //        return View(searchByName);
        //    }
        //    else
        //        return View();
        //}
    }
}