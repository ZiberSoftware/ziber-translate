using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using ZiberTranslate.Web.Models;

namespace ZiberTranslate.Web.Services
{
    public class TranslationVoteService
    {
        public static void Vote(Translation translation)
        {
            var voter = TranslatorService.FindByEmail(HttpContext.Current.User.Identity.Name);
            var vote = FindVoteForTranslator(translation, voter);
            using (var t = Global.CurrentSession.BeginTransaction())
            {
                if (vote == null)
                {
                    vote = new TranslationVote()
                    {
                        CreatedAt = DateTime.UtcNow,
                        IsPublished = false,
                        Rank = voter.Rank,
                        NeedsAdminReviewing = true,
                        Translation = translation,
                        Translator = voter
                    };

                    Global.CurrentSession.Save(vote);
                }

                t.Commit();
            }
        }

        public static void RemoveVote(Translation translation)
        {
            var voter = TranslatorService.FindByEmail(HttpContext.Current.User.Identity.Name);
            var vote = FindVoteForTranslator(translation, voter);
            var atleastOneVote = Global.CurrentSession.QueryOver<TranslationVote>()
                            .Where(x => x.Translation == translation)
                            .RowCount();
                            

            using (var t = Global.CurrentSession.BeginTransaction())
            {
                if (vote != null)
                {
                    if (vote.IsPublished == false)
                    {
                        Global.CurrentSession.Delete(vote);
                        atleastOneVote--;
                    }
                        
                }

                if (atleastOneVote == 0)
                {
                    translation.NeedsReview = true;
                    Global.CurrentSession.Update(translation);
                }

                t.Commit();
            }
        }

        public static TranslationVote FindVoteForTranslator(Translation translation, Translator translator)
        {
            return Global.CurrentSession.QueryOver<TranslationVote>()
                .Where(x => x.Translation == translation)
                .And(x => x.Translator == translator)
                .SingleOrDefault();
        }
    }
}