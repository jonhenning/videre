using System;
using System.Web;
using System.Web.Mvc;
using System.Web.Mvc.Html;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Videre.Core.Models;

namespace Videre.Core.Extensions
{
    public static class PortalExtensions
    {
        public static MvcHtmlString RenderClientControl(this HtmlHelper helper, IClientControl clientControl, string id, Chart model)
        {
            model.ClientId = clientControl.GetId(id);
            helper.RenderPartial("Controls/" + model.Path, model);
            return null;
        }

    }
}
