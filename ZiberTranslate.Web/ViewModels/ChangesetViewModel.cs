using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using ZiberTranslate.Web.Models;

namespace ZiberTranslate.Web.ViewModels
{
    public class ChangesetViewModel
    {
        public IEnumerable<TranslationChange> Changes { get; set; }
        public IEnumerable<TranslationVote> Votes { get; set; }
    }

    public class TranslationChange : ViewModels.TranslationsViewModel.TranslationDTO
    {
        public string Language { get; set; }

        public string SetName { get; set; }
    }


}