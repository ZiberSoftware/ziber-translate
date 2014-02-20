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

        public ActionResult Submit (int[] TranslationId, string language)
        {
            using (var t = DbSession.BeginTransaction())
            {
                foreach (var id in TranslationId)
                {
                    var translationsToBeApproved = TranslationService.FindById(id);
                    var leading = TranslationService.FindByKey(translationsToBeApproved.Key.Id, language);
                    if (leading != null)
                    {
                        if (translationsToBeApproved.NeedsAdminReviewing && translationsToBeApproved.Value != leading.Value)
                        {
                            leading.Value = translationsToBeApproved.Value;

                            DbSession.Update(leading);
                            DbSession.Delete(translationsToBeApproved);
                        }
                    }

                    else
                    {
                        // check if translation still needs adminreviewing(in case 2 admins are reviewing)
                        if (translationsToBeApproved.NeedsAdminReviewing)
                        {
                            leading = translationsToBeApproved;
                            leading.Translator = null;
                            leading.NeedsAdminReviewing = false;

                            DbSession.Save(leading);                            
                        }
                    }
                    
                }
                
                t.Commit();
                
            }
            return RedirectToAction("Index");
        }

        public IEnumerable<ViewModels.ReviewViewModel.SetContent> BuildSetContent(int id, string language)
        {
            var translationsNeedsAdminReviewing = DbSession.CreateCriteria<Translation>()
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

            var neutralTranslations = DbSession.QueryOver<Translation>()
                .Where(Restrictions.On<Translation>(x => x.Key.Id).IsIn(translationsNeedsAdminReviewing.Select(x => x.Key.Id).ToArray()))
                .And(x => x.Language == LanguageService.GetNeutralLanguage(HttpContext.User.Identity.Name))
                .And(x => x.NeedsAdminReviewing == false)
                .OrderBy(x => x.Votes).Desc
                .Future();

            var leadingTranslations = DbSession.QueryOver<Translation>()
                .Where(Restrictions.On<Translation>(x => x.Key.Id).IsIn(translationsNeedsAdminReviewing.Select(x => x.Key.Id).ToArray()))
                .And(x => x.Language == LanguageService.GetLanguageByIsoCode(language))
                .And(x => x.IsPublished)
                .And(x => x.NeedsAdminReviewing == false)
                .OrderBy(x => x.Votes).Desc
                .Future();


            foreach (var translation in translationsNeedsAdminReviewing)
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
                c.TranslationId = translation.Id;

                yield return c;
            }
            
        }
    }
}