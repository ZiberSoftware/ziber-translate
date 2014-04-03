using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using FluentNHibernate.Mapping;

namespace ZiberTranslate.Web.Models
{
    public class Translation : Entity, ICloneable
    {
        public Translation()
        {
            Translations = new List<TranslationDTO>();
        }

        public virtual TranslateKey Key {get;set;}
        public virtual Translator Translator { get; set; }
        public virtual Language Language { get; set; }
        public virtual string Value { get; set; }
        public virtual int Votes { get; protected set; }
        public virtual int UserVotes { get; protected set; }
        public virtual IEnumerable<TranslationDTO> Translations { get; set; }
        public virtual bool IsPublished { get; set; }
        public virtual bool NeedsAdminReviewing { get; set; }
        public virtual bool NeedsReview { get; set; }
        public virtual object Clone()
        {
            var translation = new Translation()
            {
                Key = Key,
                Translator = null,
                Language = Language,
                Value =Value,
                Votes = 0,
                UserVotes = 0,
                IsPublished = false
            };

            return translation;
        }
        public class TranslationDTO
        {
            public virtual bool Differs
            {
                get
                {
                    return Value != LeadingValue;
                }
            }
            public virtual int KeyId { get; set; }
            public virtual int SetId { get; set; }
            public virtual string Term { get; set; }
            public virtual string Value { get; set; }
            public virtual string LeadingValue { get; set; }
            public virtual int Votes { get; set; }
            public virtual int UserVotes { get; set; }
            public virtual bool Voted { get; set; }
        }
    }

    public class TranslationMap : ClassMap<Translation>
    {
        public TranslationMap()
        {
            Id(x => x.Id).GeneratedBy.Native();

            Map(x => x.Value).CustomType("StringClob");
            Map(x => x.IsPublished);
            Map(x => x.NeedsAdminReviewing);
            Map(x => x.NeedsReview);
            Map(x => x.Votes)
                .Generated.Always()
                .Formula("(SELECT ISNULL(COUNT(*), 0) FROM TranslationVote v WHERE v.Translation_Id = Id AND v.IsPublished = 1 AND v.NeedsAdminReviewing = 0)");

            Map(x => x.UserVotes)
                .Generated.Always()
                .Formula("(SELECT ISNULL(COUNT(*),0) FROM TranslationVote v WHERE v.Translation_Id = Id AND v.IsPublished = 0 AND v.NeedsAdminReviewing = 1)");

            References(x => x.Language)
                .Cascade.None();

            References(x => x.Key)                
                .Cascade.None();

            References(x => x.Translator)
                .Cascade.None();

        }
    }
}
