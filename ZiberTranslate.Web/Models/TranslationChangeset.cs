using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using FluentNHibernate.Mapping;

namespace ZiberTranslate.Web.Models
{
    public class TranslationChangeset : Entity
    {
        public virtual IEnumerable<int> Votes { get; set; }
    }

    public class TranslationChangesetMap : ClassMap<TranslationChangeset>
    {
        public TranslationChangesetMap()
        {
            Id(x => x.Id).GeneratedBy.Native();
        }
    }
}