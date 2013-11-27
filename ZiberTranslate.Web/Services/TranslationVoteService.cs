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
            var vote = FindVote(translation, voter);
            if (vote == null)
            {
                vote = new TranslationVote()
                {
                    CreatedAt = DateTime.UtcNow,
                    IsPublished = false,
                    Rank = voter.Rank,
                    Translation = translation,
                    Translator = voter
                };

                Global.CurrentSession.Save(vote);
            }
        }

        public static void RemoveVote(Translation translation)
        {
            var voter = TranslatorService.FindByEmail(HttpContext.Current.User.Identity.Name);
            var vote = FindVote(translation, voter);

            if (vote != null)
            {
                if(vote.IsPublished == false) 
                    Global.CurrentSession.Delete(vote);
            }
        }

        public static TranslationVote FindVote(Translation translation, Translator translator)
        {
            return Global.CurrentSession.QueryOver<TranslationVote>()
                .Where(x => x.Translation == translation)
                .And(x => x.Translator == translator)
                .SingleOrDefault();
        }
    }
}