using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using System.Text.RegularExpressions;

namespace Videre.Core.Binders
{
    public class DictionaryModelBinder : IModelBinder
    {
        #region IModelBinder Members

        public object BindModel(ControllerContext controllerContext, ModelBindingContext bindingContext)
        {
            if (bindingContext == null)
            {
                throw new ArgumentNullException("bindingContext");
            }

            string modelName = bindingContext.ModelName;
            var formDictionary = new Dictionary<string, object>();

            var dictionaryRegex = new Regex(modelName + @"\[(?<key>.+?)\]", RegexOptions.CultureInvariant);
            foreach (var key in controllerContext.HttpContext.Request.Form.AllKeys.Where(k => k.StartsWith(modelName + "[")))
            {
                var m = dictionaryRegex.Match(key);
                if (m.Success)
                {
                    formDictionary[m.Groups["key"].Value] = controllerContext.HttpContext.Request.Form[key];
                }
            }

            return formDictionary;
        }

        #endregion
    }
}
