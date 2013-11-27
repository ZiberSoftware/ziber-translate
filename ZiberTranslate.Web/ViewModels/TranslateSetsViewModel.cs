using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using ZiberTranslate.Web.Models;

namespace ZiberTranslate.Web.ViewModels
{
    public class TranslateSetsViewModel
    {
        public IEnumerable<TranslateSet> Sets { get; set; }
    }
}