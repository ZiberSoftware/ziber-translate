using System;
using System.IO;
using System.Web.Mvc;
using System.Xml.Linq;
using System.Linq;
using ZiberTranslate.Web.Services;
using ZiberTranslate.Web.Models;

namespace ZiberTranslate.Web.Controllers
{
    [Authorize]
    public class ImportController : BaseController
    {
        public ActionResult Index()
        {
            throw new NotSupportedException();

            var set = TranslateSetService.FindByName("Ziber Websites");
            var nl = LanguageService.GetLanguageByIsoCode("nl");
            var en = LanguageService.GetLanguageByIsoCode("en");

            if (set == null)
            {
                set = new TranslateSet
                {
                    Name = "Ziber Websites",
                    NeedsReview = 0,
                    NeedsTranslation = 0,
                    Reviewed = 0
                };

                TranslateSetService.AddTranslateSet(set);
            }

            var category = set.FindOrCreateCategory("Shop");
            set.TranslateKeys.Clear();
            DbSession.Update(set);

            if (nl == null)
            {
                nl = new Language() { IsoCode = "nl", Name = "Nederlands", IsEnabled = true };
                DbSession.Save(nl);
            }
            if (en == null)
            {
                en = new Language() { IsoCode = "en", Name = "Engels", IsEnabled = true };
                DbSession.Save(en);
            }


            Import(@"F:\dev\DL\producten\Domain.Resources\KZN\ProductShopTextDefaults.resx", set, category, nl);
            Import(@"F:\dev\DL\producten\Domain.Resources\KZN\ProductShopTextDefaults.en-US.resx", set, category, en);
            return View();
        }

        private void Import(string file, TranslateSet set, TranslateCategory category, Language language)
        {

            var xmlText = System.IO.File.ReadAllText(file);
            var xml = XElement.Parse(xmlText);
            foreach (var data in xml.Descendants("data"))
            {
                string key = data.Attribute("name").Value;
                string value = data.Descendants("value").Single().Value;

                var translateKey = set.TranslateKeys.Where(x => x.Label == key && x.Set == set).SingleOrDefault();
                if (translateKey == null)
                {
                    translateKey = new TranslateKey
                        {
                            Label = key,
                            Set = set,
                            Category = category
                        };

                    set.TranslateKeys.Add(translateKey);
                }

                var translation = new Translation() { Key = translateKey, Language = language, Translator = null, Value = value };

                DbSession.Save(translateKey);
                DbSession.Save(translation);
            }

            Global.CurrentSession.Update(set);
        }
        ///ProductShopTextDefaults.en-US.resx

    }
}
