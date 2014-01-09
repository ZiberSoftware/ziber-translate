using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using FluentNHibernate.Mapping;

namespace ZiberTranslate.Web.Models
{
    public class TranslationVote : Entity
    {
        public virtual Translation Translation { get; set; }
        public virtual Translator Translator { get; set; }
        public virtual DateTime CreatedAt { get; set; }
        public virtual int Rank { get; set; }

        public virtual bool IsPublished { get; set; }
    }

    public class TranslationVoteMap : ClassMap<TranslationVote>
    {
        public TranslationVoteMap()
        {
            Id(x => x.Id).GeneratedBy.Native();

            Map(x => x.CreatedAt);
            Map(x => x.Rank);
            Map(x => x.IsPublished);

            References(x => x.Translator).Cascade.None();
            References(x => x.Translation).Cascade.None();
        }
    }
}