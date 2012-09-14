using System;
using System.Linq;
using System.Web.Mvc;
using Videre.Core.ActionResults;
using Services = Videre.Core.Services;
using CodeEndeavors.Extensions;

namespace Videre.Web.Controllers
{
    public class ServerJSController : System.Web.Mvc.Controller
    {
        [ETagAttribute]
        public JavaScriptResult GlobalClientTranslations()
        {
            Services.Localization.GetPortalText("RequiredField.Client", "{0} is a required field");
            Services.Localization.GetPortalText("DataTypeInvalid.Client", "{0} is not a valid {1}");
            Services.Localization.GetPortalText("ValuesMustMatch.Client", "{0} requires a matching value");
            Services.Localization.GetPortalText("None.Client", "(None)");
            if (Services.Repository.Current.PendingUpdates > 0)
                Services.Repository.SaveChanges();

            var locs = Services.Localization.GetLocalizations(Core.Models.LocalizationType.Portal, l => l.Key.EndsWith(".Client"));

            var script = string.Format("videre.localization.items = {0};",
                locs.Select(l => new { key = l.Key.Replace(".Client", ""), value = l.Text, ns = "global" }).ToJson());

            //todo: register dateFormats per user or per portal?
            //var script = string.Format("videre.localization.dateFormats = {datetime: 'm-d-yy H:MM TT', date: 'm-d-yy', time: 'H:MM TT'};",

            var eTagHash = Convert.ToBase64String(System.Security.Cryptography.MD5.Create().ComputeHash(System.Text.Encoding.UTF8.GetBytes(script)));

            return new JavaScriptResult()
            {
                 Script  = script 
            };


        }


    }
}