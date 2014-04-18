using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ZiberTranslate.Web.Models;
using ZiberTranslate.Web.Services;
using NHibernate.Criterion;
using NHibernate.Transform;

namespace ZiberTranslate.Web.Controllers
{
    public class AdminController : BaseController
    {
        private log4net.ILog logger = log4net.LogManager.GetLogger(typeof(AdminController));
        [HttpGet]
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
                .SetResultTransformer(Transformers.AliasToBean<Admin.SetWithReviews>())
                .List<Admin.SetWithReviews>();

            var setsWithReviews = new Admin() { ChangedSets = setsWithChanges.Distinct() };

            return Json(new 
                {
                    setsWithReviews
                }, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult SetContent(int setId, string language)
        {
            var setContent = BuildSetContent(setId, language);

            return Json(new 
                { 
                    setContent 
                }, JsonRequestBehavior.AllowGet);
        }


        public ActionResult Submit (string language, int setId, int[] TranslationId=null)
        {
            using (var t = DbSession.BeginTransaction())
            {
                if (TranslationId == null)
                {

                    var translationsToBeDisapproved = DbSession.CreateCriteria<Translation>()
                    .CreateAlias("Key", "k")
                    .CreateAlias("Language", "l")
                    .Add(Restrictions.Eq("k.Set.Id", setId))
                    .Add(Restrictions.Eq("NeedsAdminReviewing", true))
                    .Add(Restrictions.Eq("l.IsoCode", language))
                    .List<Translation>();

                    foreach (var translation in translationsToBeDisapproved)
                    {
                        DbSession.Delete(translation);
                    }
                }

                else
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

                    var translationsToBeDisapproved = DbSession.CreateCriteria<Translation>()
                    .CreateAlias("Key", "k")
                    .CreateAlias("Language", "l")
                    .Add(Restrictions.Eq("k.Set.Id", setId))
                    .Add(Restrictions.Eq("NeedsAdminReviewing", true))
                    .Add(Restrictions.Eq("l.IsoCode", language))
                    .List<Translation>();

                    foreach (var translation in translationsToBeDisapproved)
                    {
                        DbSession.Delete(translation);
                    }

                } 
                t.Commit();
                
            }
            return RedirectToAction("Index");
        }

        public IEnumerable<Admin.SetContent> BuildSetContent(int id, string language)
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

            var neutralUserTranslations = TranslationService.GetTranslations(LanguageService.GetNeutralLanguage(HttpContext.User.Identity.Name), translationsNeedsAdminReviewing.Select(x => x.Key), TranslationType.Neutral);
            var neutralTranslations = TranslationService.GetTranslations(LanguageService.GetLanguageByIsoCode("nl"), translationsNeedsAdminReviewing.Select(x => x.Key), TranslationType.Neutral);
            var leadingTranslations = TranslationService.GetTranslations(LanguageService.GetLanguageByIsoCode(language), translationsNeedsAdminReviewing.Select(x => x.Key), TranslationType.Leading);

            foreach (var translation in translationsNeedsAdminReviewing)
            {
                var neutralUserTranslation = neutralUserTranslations.Where(x => x.Key == translation.Key).Single();
                var neutralTranslation = neutralTranslations.Where(x => x.Key == translation.Key).Single();
                var leadingTranslation = leadingTranslations.Where(x => x.Key == translation.Key).SingleOrDefault();
                var voted = votes.Any(x => x.Key == translation.Key);

                var c = new Admin.SetContent();
                c.KeyId = translation.Key.Id;
                c.SetId = id;
                c.SetName = translation.Key.Set.Name;
                c.Language = language;
                c.TranslatorName = translation.Translator.Name;
                c.Rank = translation.Translator.Rank;
                c.Term = neutralUserTranslation == null ? neutralTranslation.Value : neutralUserTranslation.Value;
                c.LeadingValue = (leadingTranslation == null ? neutralUserTranslation.Value : leadingTranslation.Value);
                c.Value = translation.Value;
                c.Voted = voted;
                c.TranslationId = translation.Id;

                yield return c;
            }
            
        }
    }
}