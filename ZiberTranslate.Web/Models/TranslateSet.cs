using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using FluentNHibernate.Mapping;

namespace ZiberTranslate.Web.Models
{
    public class TranslateSet : Entity
    {
        public TranslateSet()
        {
            TranslateKeys = new Iesi.Collections.Generic.HashedSet<TranslateKey>();
        }

        public virtual string Name { get; set; }
        public virtual int NeedsReview { get; set; }
        public virtual int NeedsTranslation { get; set; }
        public virtual int Reviewed { get; set; }
        public virtual int AllTranslations { get; set; }
        public virtual string InternalSetName { get; set; }

        public virtual Iesi.Collections.Generic.ISet<TranslateKey> TranslateKeys { get; set; }

        public virtual TranslateCategory FindOrCreateCategory(string name)
        {
            return null;
        }
    }

    public class TranslateSetMap : ClassMap<TranslateSet>
    {
        public TranslateSetMap()
        {
            Id(x => x.Id).GeneratedBy.Native();

            Map(x => x.Name);
            Map(x => x.NeedsReview);
            Map(x => x.NeedsTranslation);
            Map(x => x.Reviewed);
            Map(x => x.AllTranslations);
            Map(x => x.InternalSetName);

            HasMany(x => x.TranslateKeys)
                .AsSet()
                .Cascade.None();
        }
    }
}