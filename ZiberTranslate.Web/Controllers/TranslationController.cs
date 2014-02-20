using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
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

    public class TranslationController : BaseController
    {
        public ActionResult Index(int setId, string language, FilterType filter, int? categoryId = null)
        {
            var translations = BuildTranslations(setId, language, filter);
            var set = DbSession.Load<TranslateSet>(setId);
            var name = Global.CurrentSession.QueryOver<TranslateSet>()
                .Where(x => x.Id == setId)
                        .Select(x => x.Name)
                        .SingleOrDefault<string>();
            var lang = LanguageService.GetLanguageByIsoCode(language);

            TranslateSetService.UpdateCounters(set, lang);

            var vm = new ViewModels.TranslationsViewModel();
            vm.Translations = translations;
            vm.SetId = setId;
            vm.Name = name;
            vm.Culture = System.Globalization.CultureInfo.CreateSpecificCulture(language);
            vm.Reviewed = set.Reviewed;
            vm.NeedsReview = set.NeedsReview;
            vm.NeedsTranslation = set.NeedsTranslation;
            vm.AllTranslations = set.AllTranslations;

            if (categoryId.HasValue)
                TempData["categoryId"] = categoryId.Value;

            return View("Index", vm);
        }

        private IEnumerable<ViewModels.TranslationsViewModel.TranslationDTO> BuildTranslations(int id, string language, FilterType filter)
        {
            var me = TranslatorService.FindByEmail(HttpContext.User.Identity.Name);
            var keys = TranslationService.FilteredKeys(id, language, filter);
            var targetLanguage = LanguageService.GetLanguageByIsoCode(language);

            //fallback to dutch for anonymous users
            var neutralUserLanguage = LanguageService.GetLanguageByIsoCode("nl");
            if (HttpContext.User.Identity.IsAuthenticated)
            {
                neutralUserLanguage = LanguageService.GetNeutralLanguage(HttpContext.User.Identity.Name);
            }

            var neutralUserTranslations = DbSession.QueryOver<Translation>()
                             .Where(x => x.Language == neutralUserLanguage)
                            
                             .And(x => x.NeedsAdminReviewing == false)
                             .OrderBy(x => x.Votes).Desc
                             .Future();

            var neutralTranslations = DbSession.QueryOver<Translation>()
                             .Where(x => x.Language == LanguageService.GetLanguageByIsoCode("nl"))
                            
                             .And(x => x.NeedsAdminReviewing == false)
                             .OrderBy(x => x.Votes).Desc
                             .Future();

            var leadingTranslations = DbSession.QueryOver<Translation>()
                              .Where(x => x.Language == targetLanguage)
                              .And(x => x.IsPublished)
                              .And(x => x.NeedsAdminReviewing == false)
                              .OrderBy(x => x.Votes).Desc
                              .Future();

            var userTranslations = DbSession.QueryOver<Translation>()
                        .Where(x => x.Language == targetLanguage)
                        .And(x => x.Translator == me)
                        .And(x => x.IsPublished == false)
                        .And(x => x.NeedsAdminReviewing)
                        .OrderBy(x => x.Votes).Desc
                        .Future();

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
            return TranslationRow(translation);
        }

        private ActionResult TranslationRow(Translation translation)
        {
            return TranslationRow(translation.Key.Set.Id, translation.Language.IsoCode, translation.Key.Id);
        }

        private ActionResult TranslationRow(int setId, string language, int keyId)
        {
            FilterType filter = FilterType.All;

            var translations = BuildTranslations(setId, language, filter);


            return PartialView("TranslationRow", translations.Where(x => x.KeyId == keyId).SingleOrDefault());
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

                return TranslationRow(setId, language, keyId);
            }

            return TranslationRow(translation);
        }

        [HttpPost]
        public ActionResult Approve(int id, string language)
        {
            var translation = TranslationService.FindByKey(id, language);

            TranslationVoteService.Vote(translation);

            if (Request.IsAjaxRequest())
            {
                return TranslationRow(translation);
            }

            return RedirectToAction("Index", new { id = 0 });
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

            if (Request.IsAjaxRequest())
            {
                return TranslationRow(translation);
            }

            return RedirectToAction("Index", new { id = 0 });
        }

        public ActionResult SearchByName(int id, string language = "", string searchString = "")
        {
            var searchByKey = TranslationService.FindByKey(id, language);
            var searchByName = DbSession.QueryOver<Translation>()
                               .Where(x => x.Value.Contains(searchString))
                               .SingleOrDefault<string>();

            if (!String.IsNullOrEmpty(searchString))
            {
                return View(searchByName);
            }
            else
                return View();
        }
    }
}