using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using Newtonsoft.Json;
using CodeEndeavors.Extensions;

namespace Videre.Core.Binders
{
    public class JsonNetModelBinder : DefaultModelBinder
    {
        public override object BindModel(ControllerContext controllerContext, ModelBindingContext bindingContext)
        {
            if (!IsJSONRequest(controllerContext) || !ShouldUseJsonNet(bindingContext))
            {
                return base.BindModel(controllerContext, bindingContext);
            }
            // Get the JSON data that's been posted
            var request = controllerContext.HttpContext.Request;
            request.InputStream.Position = 0;
            var jsonStringData = new StreamReader(request.InputStream).ReadToEnd();
            //var jsonStringData = bindingContext.ValueProvider.GetValue(bindingContext.ModelName);
            //var dict = JsonConvert.DeserializeObject<Dictionary<string, object>>(jsonStringData);
            var dict = jsonStringData.ToObject<Dictionary<string, object>>();   //use standard conversion so plugins to settings can be used
            if (dict.ContainsKey(bindingContext.ModelName))
            {
                var settings = new JsonSerializerSettings()
                {
                    NullValueHandling = NullValueHandling.Ignore
                };
                //hack!
                var o = JsonConvert.DeserializeObject(JsonConvert.SerializeObject(dict[bindingContext.ModelName],  Formatting.None, settings), bindingContext.ModelType, settings: settings);
                return o;
            }
            return null;
        }

        private static bool IsJSONRequest(ControllerContext controllerContext)
        {
            var contentType = controllerContext.HttpContext.Request.ContentType;
            return contentType.Contains("application/json");
        }

        private static bool ShouldUseJsonNet(ModelBindingContext bindingContext)
        {
            return bindingContext.ModelMetadata.IsComplexType;
        }

    }
}
