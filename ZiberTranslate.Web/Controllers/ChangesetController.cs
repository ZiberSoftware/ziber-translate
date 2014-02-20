﻿using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using ZiberTranslate.Web.Models;
using ZiberTranslate.Web.ViewModels;
using ZiberTranslate.Web.Services;
using NHibernate.Criterion;

namespace ZiberTranslate.Web.Controllers
{
    [Authorize]
    public class ChangesetController : BaseController
    {
        [HttpGet]
        public ActionResult Index()
        {
            var vm = new ChangesetViewModel();
            vm.Changes = BuildTranslations();

            if (!vm.Changes.Any())
            {
                return new EmptyResult();
            }

            return View("Changeset", vm);
        }

        private IEnumerable<TranslationChange> BuildTranslations()
        {
            var me = TranslatorService.FindByEmail(HttpContext.User.Identity.Name);
            
            var changes = DbSession.CreateCriteria<Translation>()
                .Add(Restrictions.Eq("IsPublished", false))
                .Add(Restrictions.Eq("Translator", me))
                .CreateAlias("Key", "k")
                .CreateAlias("k.Set", "s")
                .Future<Translation>();

            var votedOn = DetachedCriteria.For<TranslationVote>()
                .Add(Restrictions.Eq("IsPublished", false))
                .Add(Restrictions.Eq("Translator", me))
                .CreateAlias("Translation", "t")
                .SetProjection(Projections.Property("t.Id"));

            var votes = DbSession.CreateCriteria<Translation>()
                .Add(Subqueries.PropertyIn("Id", votedOn))
                .Future<Translation>();

            changes = changes.Concat(votes);

            var neutralUserTranslations = DbSession.QueryOver<Translation>()
                .Where(Restrictions.On<Translation>(x => x.Key).IsIn(changes.Select(x => x.Key).ToArray()))
                .And(x => x.Language == LanguageService.GetNeutralLanguage(HttpContext.User.Identity.Name))
                .And(x => x.NeedsAdminReviewing == false)
                .OrderBy(x => x.Votes).Desc
                .Future();

            var neutralTranslations = DbSession.QueryOver<Translation>()
                .Where(Restrictions.On<Translation>(x => x.Key).IsIn(changes.Select(x => x.Key).ToArray()))
                .And(x => x.Language == LanguageService.GetLanguageByIsoCode("nl"))
                .And(x => x.NeedsAdminReviewing == false)
                .OrderBy(x => x.Votes).Desc
                .Future();

            var leadingTranslations = DbSession.QueryOver<Translation>()
                .Where(Restrictions.On<Translation>(x => x.Key).IsIn(changes.Select(x => x.Key).ToArray()))
                .And(x => x.IsPublished)
                .And(x => x.NeedsAdminReviewing == false)
                .OrderBy(x => x.Votes).Desc
                .Future();

            var translations = (
                from change in changes
                let neutralUserTranslation = neutralUserTranslations.Where(x => x.Key == change.Key).FirstOrDefault()
                let neutralTranslation = neutralTranslations.Where(x => x.Key == change.Key).FirstOrDefault()
                let leadingTranslation = leadingTranslations.Where(x => x.Key == change.Key && x.Language == change.Language).FirstOrDefault()
                let voted = votes.Any(x => x.Key == (neutralUserTranslation == null ? neutralUserTranslation.Key : neutralTranslation.Key))
                select new TranslationChange
                {
                    KeyId = change.Key.Id,
                    SetId = change.Key.Set.Id,
                    SetName = change.Key.Set.Name,
                    Language = change.Language.IsoCode,
                    Term = neutralUserTranslation == null ? neutralTranslation.Value : neutralUserTranslation.Value,
                    LeadingValue = leadingTranslation == null ? (voted ? change.Value : string.Empty) : leadingTranslation.Value,
                    Value = change.Value,
                    Voted = voted
                }
            ).ToList();

            return translations;
        }

        [HttpPost]
        public ActionResult Submit()
        {
            using (var t = DbSession.BeginTransaction())
            {
                var me = TranslatorService.FindByEmail(HttpContext.User.Identity.Name);
                var rank = TranslatorService.FetchRank(HttpContext.User.Identity.Name);

                var changes = DbSession.CreateCriteria<Translation>()
                    .Add(Restrictions.Eq("IsPublished", false))
                    .Add(Restrictions.Eq("Translator", me))
                    .CreateAlias("Key", "k")
                    .CreateAlias("k.Set", "s")
                    .Future<Translation>();

                var votes = DbSession.QueryOver<TranslationVote>()
                    .Where(x => x.IsPublished == false)
                    .And(x => x.Translator == me)
                    .Future();

               // TranslationService.SendEmail();

                foreach (var translation in changes.ToList())
                {
                    translation.IsPublished = true;
                    if (rank == 0)
                    {
                        translation.NeedsAdminReviewing = false;
                    }

                    DbSession.Update(translation);
                }

                foreach (var vote in votes.ToList())
                {
                    vote.IsPublished = true;
                    if (rank == 0)
                    {
                        vote.NeedsAdminReviewing = false;
                    }
                    DbSession.Update(vote);
                }

                t.Commit();
            }

            return RedirectToAction("Index");
        }
    }

    public class Changeset
    {
        public IEnumerable<Translation> Changes { get; set; }
        public IEnumerable<TranslationVote> Votes { get; set; }
    }
}