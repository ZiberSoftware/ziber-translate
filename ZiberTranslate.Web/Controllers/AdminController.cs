using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ZiberTranslate.Web.Models;
using ZiberTranslate.Web.Services;
using NHibernate.Criterion;
using NHibernate.Transform;
using ZiberTranslate.Web.ViewModels;

namespace ZiberTranslate.Web.Controllers
{
    public class AdminController : BaseController
    {
        public ActionResult Index()
        {
            var sets = DbSession.CreateCriteria<TranslateSet>()
                .List();

            var setsWithChanges = DbSession.CreateCriteria<Translation>()
                .Add(Restrictions.Eq("NeedsAdminReviewing", true))
                .CreateAlias("Key", "k")
                .CreateAlias("k.Set", "s")
                .CreateAlias("Language", "l")
                .SetProjection(Projections.ProjectionList()
                    .Add(Projections.Property("s.Id"), "SetId")
                    .Add(Projections.Property("s.Name"), "SetName")
                    .Add(Projections.Property("l.IsoCode"), "Language"))
                .SetResultTransformer(Transformers.AliasToBean<ViewModels.ReviewViewModel.SetWithReviews>())
                .List<ViewModels.ReviewViewModel.SetWithReviews>();

            var vm = new ReviewViewModel() { ChangedSets = setsWithChanges.Distinct() };

            return View(vm);
        }

        public ActionResult Set(int setId, string language)
        {
            var content = BuildSetContent(setId, language);
            var vm = new ReviewViewModel() { Content = content };

            return View("AdminReview", vm);

        }

        public IEnumerable<ViewModels.ReviewViewModel.SetContent> BuildSetContent(int id, string language)
        {
            //var translationsNeedsReviewing = DbSession.QueryOver<Translation>()
            //     .Where(x => x.Language == LanguageService.GetLanguageByIsoCode(language))
            //     .And(x => x.IsPublished)
            //     .And(x => x.NeedsAdminReviewing)
            //     .Future();

            var translationsNeedsReviewing = DbSession.CreateCriteria<Translation>()
                .CreateAlias("Key", "k")
                .CreateAlias("Language", "l")
                .Add(Restrictions.Eq("k.Set.Id", id))
                .Add(Restrictions.Eq("NeedsAdminReviewing", true))
                .Add(Restrictions.Eq("l.IsoCode", language))
                .List<Translation>();


            var votedOn = DetachedCriteria.For<TranslationVote>()
                .Add(Restrictions.Eq("NeedsAdminReviewing", true))
                .CreateAlias("Translation", "t")
                .SetProjection(Projections.Property("t.Id"));

            var votes = DbSession.CreateCriteria<Translation>()
                .Add(Subqueries.PropertyIn("Id", votedOn))
                .Future<Translation>();

            //NeedAdminReview = NeedAdminReview.Concat(votes);

            var neutralTranslations = DbSession.QueryOver<Translation>()
                .Where(Restrictions.On<Translation>(x => x.Key.Id).IsIn(translationsNeedsReviewing.Select(x => x.Key.Id).ToArray()))
                .And(x => x.Language == LanguageService.GetNeutralLanguage())
                .And(x => x.NeedsAdminReviewing == false)
                .OrderBy(x => x.Votes).Desc
                .Future();

            var leadingTranslations = DbSession.QueryOver<Translation>()
                .Where(Restrictions.On<Translation>(x => x.Key.Id).IsIn(translationsNeedsReviewing.Select(x => x.Key.Id).ToArray()))
                .And(x => x.Language == LanguageService.GetLanguageByIsoCode(language))
                .And(x => x.IsPublished)
                .And(x => x.NeedsAdminReviewing == false)
                .OrderBy(x => x.Votes).Desc
                .Future();

            //var translations = (
            //    from translation in translationsNeedsReviewing
            //    let neutralTranslation = neutralTranslations.Where(x => x.Key == translation.Key).Single()
            //    let leadingTranslation = leadingTranslations.Where(x => x.Key == translation.Key).SingleOrDefault()
            //    let voted = votes.Any(x => x.Key == translation.Key)
            //    select new ReviewViewModel.SetContent
            //    {
            //        KeyId = translation.Key.Id,
            //        SetId = id,
            //        SetName = translation.Key.Set.Name,
            //        Language = language,
            //        TranslatorName = translation.Translator.Name,
            //        Rank = translation.Translator.Rank,
            //        Term = neutralTranslation == null ? string.Empty : neutralTranslation.Value,
            //        LeadingValue = (leadingTranslation == null ? neutralTranslation.Value : leadingTranslation.Value),
            //        Value = translation.Value,
            //        Voted = voted
            //    }

            //).ToList();

            foreach (var translation in translationsNeedsReviewing)
            {
                var neutralTranslation = neutralTranslations.Where(x => x.Key == translation.Key).Single();
                var leadingTranslation = leadingTranslations.Where(x => x.Key == translation.Key).SingleOrDefault();
                var voted = votes.Any(x => x.Key == translation.Key);

                var c = new ReviewViewModel.SetContent();
                c.KeyId = translation.Key.Id;
                c.SetId = id;
                c.SetName = translation.Key.Set.Name;
                c.Language = language;
                c.TranslatorName = translation.Translator.Name;
                c.Rank = translation.Translator.Rank;
                c.Term = neutralTranslation == null ? string.Empty : neutralTranslation.Value;
                c.LeadingValue = (leadingTranslation == null ? neutralTranslation.Value : leadingTranslation.Value);
                c.Value = translation.Value;
                c.Voted = voted;

                yield return c;
            }
            
        }
    }
}