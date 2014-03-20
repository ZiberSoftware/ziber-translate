using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using FluentNHibernate.Mapping;

namespace ZiberTranslate.Web.Models
{
    public class Language : Entity
    {
        public virtual bool IsEnabled { get; set; }

        //ISO-639-1
        public virtual string IsoCode { get; set; }
        public virtual string Name { get; set; }
        public virtual IEnumerable<LanguageList> List { get; set; }

        public class LanguageList
        {
            public virtual string Name { get; set; }
            public virtual string IsoCode { get; set; }
        }
    }

    public class LanguageMap : ClassMap<Language>
    {
        public LanguageMap()
        {
            Id(x => x.Id).GeneratedBy.Native();

            Map(x => x.IsEnabled);
            Map(x => x.IsoCode);
        }
    }
}