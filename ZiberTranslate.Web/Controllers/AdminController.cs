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

        public ActionResult Set()
        {
            var content = BuildSetContent();
            var vm = new ReviewViewModel() { Content = content };
            return View("AdminReview", vm);

        }

        public IEnumerable<ViewModels.ReviewViewModel.SetContent> BuildSetContent()
        {
            var NeedAdminReview = DbSession.CreateCriteria<Translation>()
                 .Add(Restrictions.Eq("NeedsAdminReviewing", true))
                 .CreateAlias("Key", "k")
                 .CreateAlias("k.Set", "s")
                 .Future<Translation>();

            var votedOn = DetachedCriteria.For<TranslationVote>()
                .Add(Restrictions.Eq("NeedsAdminReviewing", true))                
                .CreateAlias("Translation", "t")
                .SetProjection(Projections.Property("t.Id"));

            var votes = DbSession.CreateCriteria<Translation>()
                .Add(Subqueries.PropertyIn("Id", votedOn))
                .Future<Translation>();

            NeedAdminReview = NeedAdminReview.Concat(votes);

            var neutralTranslations = DbSession.QueryOver<Translation>()
                .Where(Restrictions.On<Translation>(x => x.Key).IsIn(NeedAdminReview.Select(x => x.Key).ToArray()))
                .And(x => x.Language == LanguageService.GetNeutralLanguage())
                .And(x => x.NeedsAdminReviewing == false)
                .OrderBy(x => x.Votes).Desc
                .Future();

            var leadingTranslations = DbSession.QueryOver<Translation>()
                .Where(Restrictions.On<Translation>(x => x.Key).IsIn(NeedAdminReview.Select(x => x.Key).ToArray()))
                .And(x => x.IsPublished)
                .And(x => x.NeedsAdminReviewing == false)
                .OrderBy(x => x.Votes).Desc
                .Future();

            var translations = (
                from change in NeedAdminReview
                let neutralTranslation = neutralTranslations.Where(x => x.Key == change.Key).FirstOrDefault()
                let leadingTranslation = leadingTranslations.Where(x => x.Key == change.Key && x.Language == change.Language).FirstOrDefault()
                let voted = votes.Any(x => x.Key == neutralTranslation.Key)
                select new ViewModels.ReviewViewModel.SetContent
                {
                    KeyId = change.Key.Id,
                    TranslatorName = change.Translator.Name,
                    Rank = change.Translator.Rank,
                    SetId = change.Key.Set.Id,
                    SetName = change.Key.Set.Name,
                    Language = change.Language.IsoCode,
                    Term = neutralTranslation.Value,
                    LeadingValue = leadingTranslation == null ? (voted ? change.Value : string.Empty) : leadingTranslation.Value,
                    Value = change.Value,
                    Voted = voted
                }
            ).ToList();

            return translations;
        }
    }
}