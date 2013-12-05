using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using FluentNHibernate.Mapping;

namespace ZiberTranslate.Web.Models
{
    public class Translator : Entity
    {
        public virtual string Name { get; set; }

        public virtual string EmailAddress { get; set; }
        public virtual string Password { get; set; }
        public virtual string Salt { get; set; }
        public virtual string Hash { get; set; }
        public virtual int Rank { get; set; }
        public virtual bool IsBlocked { get; set; }

        public virtual Iesi.Collections.Generic.ISet<Translation> Translations { get; set; }
    }

    public class TranslatorMap : ClassMap<Translator>
    {
        public TranslatorMap()
        {
            Id(x => x.Id).GeneratedBy.Native();

            Map(x => x.Name);
            Map(x => x.EmailAddress);
            Map(x => x.Password);
            Map(x => x.Salt);
            Map(x => x.Hash);
            Map(x => x.Rank);
            Map(x => x.IsBlocked);

            HasMany(x => x.Translations)
                .AsSet();
        }
    }
}