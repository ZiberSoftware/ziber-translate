using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using ZiberTranslate.Web.Models;

namespace ZiberTranslate.Web.ViewModels
{
    public class TranslationsViewModel
    {
        public TranslationsViewModel()
        {
            Translations = new List<TranslationDTO>();
        }

        public IEnumerable<TranslationDTO> Translations { get; set; }
        public int NeedsTranslation { get; set; }
        public int NeedsReview { get; set; }
        public int Reviewed { get; set; }
        public int AllTranslations { get; set; }

        public class TranslationDTO
        {
            public bool Differs
            {
                get
                {
                    return Value != LeadingValue;
                }
            }
            public int KeyId { get; set; }
            public int SetId { get; set; }
            public string Term { get; set; }
            public string Value { get; set; }
            public string LeadingValue { get; set; }
            public int Votes { get; set; }
            public bool Voted { get; set; }
        }

        public IEnumerable<int> VotedOnKeys { get; set; }
    }
}