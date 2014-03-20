using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using ZiberTranslate.Web.Models;
using ZiberTranslate.Web.Controllers;
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

        public static int GetCounter(int id, string language, FilterType filter)
        {
            var keys = Global.CurrentSession.CreateCriteria<TranslateKey>()
                .Add(Restrictions.Eq("Set.Id", id));
                

            var translations = DetachedCriteria.For<Translation>()
                                .CreateAlias("Key", "k")
                                .CreateAlias("Language", "l")
                                .Add(Restrictions.Eq("k.Set.Id", id))
                                .Add(Restrictions.Eq("l.IsoCode", language))
                                .SetProjection(Projections.Property("k.Id"));
            switch (filter)
            {
                case FilterType.NeedsReview:
                    {
                        translations.Add(Restrictions.Eq("NeedsReview", true))
                            .Add(Restrictions.Eq("NeedsAdminReviewing", false));
                        return keys.Add(Subqueries.PropertyIn("Id", translations))
                            .SetProjection(Projections.RowCount())
                            .UniqueResult<int>();
                    }

                case FilterType.NeedsTranslation:
                    {
                        return keys.Add(Subqueries.PropertyNotIn("Id", translations))
                                .SetProjection(Projections.RowCount())
                            .UniqueResult<int>(); 
                    }

                case FilterType.Reviewed:
                    {
                        translations.Add(Restrictions.Eq("NeedsReview", false))
                            .Add(Restrictions.Eq("NeedsAdminReviewing", false));
                        return keys.Add(Subqueries.PropertyIn("Id", translations))
                            .SetProjection(Projections.RowCount())
                            .UniqueResult<int>();
                    }
            }

            return keys.SetProjection(Projections.RowCount())
                            .UniqueResult<int>();          
        }
    }
}