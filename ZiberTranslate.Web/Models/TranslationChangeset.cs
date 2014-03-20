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
        public virtual IEnumerable<TranslationVote> VoteChanges { get; set; }
        public virtual IEnumerable<TranslationChange> Changes { get; set; }
        
        public class TranslationChange : Translation.TranslationDTO
        {
            public string Language { get; set; }

            public string SetName { get; set; }
        }
    }

    

    public class TranslationChangesetMap : ClassMap<TranslationChangeset>
    {
        public TranslationChangesetMap()
        {
            Id(x => x.Id).GeneratedBy.Native();
        }
    }
}