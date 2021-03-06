﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using ZiberTranslate.Web.Models;
using NHibernate.Criterion;
using NHibernate.Transform;

namespace ZiberTranslate.Web.Services
{
    public class TranslateSetService
    {

        public static TranslateSet FindByName(string name)
        {
            return Global.CurrentSession.QueryOver<TranslateSet>().Where(x => x.Name == name).SingleOrDefault();
        }

        public static void AddTranslateSet(TranslateSet set)
        {
            Global.CurrentSession.Save(set);
        }

        public static void UpdateCounters(TranslateSet set, Language language)
        {
            var keyCount = set.TranslateKeys.Count;

            var keys = DetachedCriteria.For<TranslateKey>("s")
                .Add(Restrictions.Eq("Set", set))
                .Add(Restrictions.EqProperty("s.Id", "t.Key.Id"))
                .SetProjection(Projections.Id());

            var translated = Global.CurrentSession.CreateCriteria<Translation>("t")
                .Add(Restrictions.Eq("IsPublished", true))
                .Add(Restrictions.Eq("NeedsReviewing", false))
                .Add(Subqueries.Exists(keys))
                .Add(Restrictions.Eq("Language", language))
                .SetProjection(Projections.RowCount())
                .UniqueResult<int>();


            set.NeedsTranslations = keyCount - translated;
            set.Reviewed = translated;

            Global.CurrentSession.Update(set);
        }
    }
}