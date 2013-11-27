using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using FluentNHibernate.Mapping;

namespace ZiberTranslate.Web.Models
{
    public class TranslateCategory : Entity
    {
        public virtual string Label { get; set; }

        public virtual TranslateSet TranslateSet { get; set; }
    }

    public class TranslateCategoryMap : ClassMap<TranslateCategory>
    {
        public TranslateCategoryMap()
        {
            Id(x => x.Id).GeneratedBy.Native();

            Map(x => x.Label);

            References(x => x.TranslateSet).Cascade.None();
        }
    }
}