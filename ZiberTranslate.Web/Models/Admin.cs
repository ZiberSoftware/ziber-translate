﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using FluentNHibernate.Mapping;

namespace ZiberTranslate.Web.Models
{
    public class Admin : Entity
    {
        public Admin()
        {
            Content = new List<SetContent>();
        }
        public virtual IEnumerable<SetWithReviews> ChangedSets { get; set; }
        public virtual IEnumerable<SetContent> Content { get; set; }

        public class SetContent : Translation.TranslationDTO
        {
            public string Language { get; set; }
            public string SetName { get; set; }
            public int Rank { get; set; }
            public string TranslatorName { get; set; }
            public int TranslationId { get; set; }
        }

        public class SetWithReviews
        {
            public int SetId { get; set; }
            public string SetName { get; set; }
            public string Language { get; set; }

            public override bool Equals(object obj)
            {
                var other = obj as SetWithReviews;

                if (other == null)
                    return false;

                return SetId == other.SetId && Language == other.Language;
            }

            public override int GetHashCode()
            {
                return (SetId.ToString() + "|" + Language).GetHashCode();
            }
        }
    }
}