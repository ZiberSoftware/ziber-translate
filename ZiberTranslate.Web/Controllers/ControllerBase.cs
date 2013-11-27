using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using NHibernate;

namespace ZiberTranslate.Web.Controllers
{
    public class BaseController : Controller
    {
        public ISession DbSession{ get; set; }
    }
}