﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using FluentNHibernate.Mapping;

namespace ZiberTranslate.Web.Models
{
    public class Translation : Entity, ICloneable
    {
        public virtual TranslateKey Key {get;set;}
        public virtual Translator Translator { get; set; }
        public virtual Language Language { get; set; }
        public virtual string Value { get; set; }
        public virtual int Votes { get; protected set; }

        public virtual bool IsPublished { get; set; }
        public virtual bool NeedsReviewing { get; set; }

        public virtual object Clone()
        {
            var translation = new Translation()
            {
                Key = Key,
                Translator = null,
                Language = Language,
                Value =Value,
                Votes = 0,
                IsPublished = false
            };

            return translation;
        }
    }

    public class TranslationMap : ClassMap<Translation>
    {
        public TranslationMap()
        {
            Id(x => x.Id).GeneratedBy.Native();

            Map(x => x.Value).CustomType("StringClob");
            Map(x => x.IsPublished);
            Map(x => x.NeedsReviewing);

            Map(x => x.Votes)
                .Generated.Always()
                .Formula("(SELECT ISNULL(COUNT(*), 0) FROM TranslationVote v WHERE v.Translation_Id = Id AND v.IsPublished = 1 AND v.NeedsReviewing = 0)");

            References(x => x.Language)
                .Cascade.None();

            References(x => x.Key)                
                .Cascade.None();

            References(x => x.Translator)
                .Cascade.None();

        }
    }
}
