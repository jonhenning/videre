using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Videre.Core.Services
{
    public static class Widget
    {
        public const string WidgetIdHeader = "X-Videre-WidgetId";

        public sealed class WidgetIdentity
        {
            public string Id { get; internal set; }

            public Models.Widget Instance { get; internal set; }
        }

        public static WidgetIdentity Current
        {
            get
            {
                var widgetId = HttpContext.Current.Request.Headers[WidgetIdHeader];
                return string.IsNullOrWhiteSpace(widgetId) ? null : new WidgetIdentity {Id = widgetId, Instance = GetWidgetById(widgetId)};
            }
        }

        public static List<Models.Widget> GetWidgetInstancesByName(string name, string portalId = null)
        {
            var widgets = new List<Models.Widget>();
            widgets.AddRange(Portal.GetPageTemplates(portalId).SelectMany(t => t.Widgets).Where(w => w.Manifest.Name == name).ToList());
            widgets.AddRange(Portal.GetLayoutTemplates(portalId).SelectMany(t => t.Widgets).Where(w => w.Manifest.Name == name).ToList());
            return widgets;
        }

        public static Models.Widget GetWidgetById(string id, string portalId = null)
        {
            var instance = Portal.GetPageTemplates(portalId).SelectMany(t => t.Widgets).FirstOrDefault(w => w.Id == id);
            return instance ?? Portal.GetLayoutTemplates(portalId).SelectMany(t => t.Widgets).FirstOrDefault(w => w.Id == id);
        }
    }
}