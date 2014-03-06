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
            var set = DbSession.Load<TranslateSet>(setId);
            var lang = LanguageService.GetLanguageByIsoCode(language);

            TranslateSetService.UpdateCounters(set, lang);

            return Json(new
            {
                reviewed = set.Reviewed,
                needsReview = set.NeedsReview,
                needsTranslation = set.NeedsTranslation,
                total = set.AllTranslations
            }, JsonRequestBehavior.AllowGet);


        }


        private IEnumerable<ViewModels.TranslationsViewModel.TranslationDTO> BuildTranslations(int id, string language, FilterType filter, int pageNr)
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
                        select new ViewModels.TranslationsViewModel.TranslationDTO
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

                TranslateSetService.UpdateCounters(translation.Key.Set, translation.Language);

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

        public ActionResult CancelChanges()
        {
            var me = TranslatorService.FindByEmail(HttpContext.User.Identity.Name);
            var changes = TranslationService.GetChangesForTranslator(me);

            using (var t = DbSession.BeginTransaction())
            {
                DbSession.Delete(changes);

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

        public ActionResult AddLanguage()
        {
            CultureInfo[] cultures = System.Globalization.CultureInfo.GetCultures(CultureTypes.NeutralCultures);
            return new EmptyResult();
        }

        public ActionResult SearchByName(int id, string language = "", string searchString = "")
        {
            var searchByKey = TranslationService.FindByKey(id, language);
            var searchByName = DbSession.QueryOver<Translation>()
                               .Where(x => x.Value.Contains(searchString))
                               .SingleOrDefault();

            if (!String.IsNullOrEmpty(searchString))
            {
                return View(searchByName);
            }
            else
                return View();
        }
    }
}