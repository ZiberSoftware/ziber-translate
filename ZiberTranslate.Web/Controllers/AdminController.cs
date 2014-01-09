using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ZiberTranslate.Web.Models;
using NHibernate.Criterion;
using NHibernate.Transform;
using ZiberTranslate.Web.ViewModels;

namespace ZiberTranslate.Web.Controllers
{
    public class AdminController : BaseController
    {
        public ActionResult Index()
        {
            var sets = DbSession.CreateCriteria<TranslateSet>()
                .List();

            TranslateKey key;

            var setsWithChanges = DbSession.CreateCriteria<Translation>()
                .Add(Restrictions.Eq("NeedsAdminReviewing", true))
                .CreateAlias("Key", "k")
                .CreateAlias("k.Set", "s")
                .CreateAlias("Language", "l")
                .SetProjection(Projections.ProjectionList()
                    .Add(Projections.Property("s.Id"), "SetId")
                    .Add(Projections.Property("s.Name"), "SetName")
                    .Add(Projections.Property("l.IsoCode"), "Language"))
                .SetResultTransformer(Transformers.AliasToBean<SetWithReviews>())
                .List<SetWithReviews>();

            var vm = new ReviewViewModel() { ChangedSets = setsWithChanges.Distinct() };

            return View(vm);
        }



        public ActionResult Set(int id, string language)
        {
            return new EmptyResult();
        }
    }
}