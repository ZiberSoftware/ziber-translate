using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using FluentNHibernate.Mapping;

namespace ZiberTranslate.Web.Models
{
    public class TranslateKey : Entity
    {
        public virtual string Label { get; set; }

        public virtual TranslateCategory Category { get; set; }
        public virtual TranslateSet Set { get; set; }

        public override int GetHashCode()
        {
            if (Category == null)
                return Label.GetHashCode() + Set.GetHashCode();

            return Label.GetHashCode() + Set.GetHashCode() + Category.GetHashCode();
        }
    }

    public class TranslateKeyMap : ClassMap<TranslateKey>
    {
        public TranslateKeyMap()
        {
            Id(x => x.Id).GeneratedBy.Native();

            Map(x => x.Label);

            References(x => x.Category).Cascade.None();
            References(x => x.Set)
                .Column("TranslateSet_id")
                .Cascade.None();
        }
    }
}