using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Web.Mvc;
using System.Web.Mvc.Html;
using System.Web;
using CodeEndeavors.Extensions;
using Videre.Core.Extensions;

namespace Videre.Core.Extensions
{
    public static class PortalExtensions
    {
        private static List<Models.Widget> DeferredWidgets
        {
            get
            {
                var widgets = HttpContext.Current.Items.GetSetting<List<Models.Widget>>("DeferredWidgets", null);
                if (widgets == null)
                {
                    widgets = new List<Models.Widget>();
                    HttpContext.Current.Items["DeferredWidgets"] = widgets;
                }
                return widgets;
            }
        }

        // Methods
        public static string RenderWidgets(this HtmlHelper helper, Models.PageTemplate Template, string PaneName)
        {
            if (Template != null)
            {
                foreach (var widget in Template.Widgets.Where(w => w.PaneName == PaneName && w.IsAuthorized))
                    RenderWidget(helper, widget);

                foreach (var widget in Template.LayoutWidgets.Where(w => w.PaneName == PaneName && w.IsAuthorized))
                    RenderWidget(helper, widget);

            }
            return "";
        }        
        
        public static string RenderWidget(this HtmlHelper helper, Models.Widget widget, bool defer = false)
        {
            if (widget.IsAuthorized)
            {
                if (!defer)
                {
                    widget.ClientId = Services.Portal.NextClientId();
                    helper.RenderPartial("Widgets/" + widget.Manifest.FullName, widget);
                }
                else
                    DeferredWidgets.Add(widget);
            }
            return "";
        }

        public static string RenderDeferredWidgets(this HtmlHelper helper)
        {
            foreach (var widget in DeferredWidgets)
                RenderWidget(helper, widget);
            return "";  //TODO: return string?????
        }

        public static string RenderWidget(this HtmlHelper helper, string manifestFullName, Dictionary<string, object> attributes = null, bool defer = false, string css = null, string style = null)
        {
            var manifest = Services.Portal.GetWidgetManifest(manifestFullName);
            var widget = new Models.Widget() { ManifestId = manifest.Id, Css = css, Style = style };
            if (attributes != null)
                widget.Attributes = attributes;
            RenderWidget(helper, widget, defer);
            return "";
        }

        public static void RenderWidgetEditor(this HtmlHelper helper, Models.WidgetManifest manifest)
        {
            //var manifest = Services.Portal.GetWidgetManifest(ManifestFullPath);
            helper.RegisterScript("~/scripts/widgets/videre.widgets.editor.base.js", true);

            var path = string.IsNullOrEmpty(manifest.EditorPath) ? "Widgets/Core/CommonEditor" : manifest.EditorPath;
            if (!HtmlExtensions.IsKeyRegistered(helper, path))
            {
                helper.RenderPartial(path, new Videre.Core.Models.WidgetEditor(manifest.Id) { ClientId = Services.Portal.NextClientId() });
                HtmlExtensions.RegisterKey(helper, path);
            }
        }

        public static void RegisterControlPresenter(this HtmlHelper helper, Models.IClientControl model, string clientType, Dictionary<string, object> properties = null)
        {
            RegisterControlPresenter(helper, model, clientType, model.ClientId, properties);
        }

        public static void RegisterControlPresenter(this HtmlHelper helper, Models.IClientControl model, string clientType, string instanceName, Dictionary<string, object> properties = null)
        {
            properties = properties == null ? new Dictionary<string, object>() : properties;
            RegisterCoreScripts(helper);

            if (!string.IsNullOrEmpty(model.ScriptPath))
                HtmlExtensions.RegisterScript(helper, model.ScriptPath + clientType + ".js", true);

            properties["id"] = model.ClientId;  //todo: not necessary now... same as ns?
            properties["ns"] = model.ClientId;

            //Properties["user"] = Services.Account.GetClientUser();
            //var ser = new System.Web.Script.Serialization.JavaScriptSerializer();   //no binders for date conversions...
            HtmlExtensions.RegisterDocumentReadyScript(helper, model.ClientId + "Presenter", string.Format("videre.widgets.register('{0}', {1}, {2});", model.ClientId, clientType, properties.ToJson(ignoreType: "client")));
        }

        public static void RegisterCoreScripts(this HtmlHelper helper)
        {
            //RegisterScript(helper, "~/scripts/date.js", true);
            HtmlExtensions.RegisterScript(helper, "~/scripts/videre.extensions.js", true);
            HtmlExtensions.RegisterScript(helper, "~/scripts/videre.js", true);
            HtmlExtensions.RegisterScript(helper, "~/ServerJS/GlobalClientTranslations", true);

            //todo: FIX!
            HtmlExtensions.ScriptMarkup(helper, "coreconstants", "var ROOT_URL = '" + Videre.Core.Extensions.HtmlExtensions.RootPath + "';");
        }

        public static void RegisterTheme(this HtmlHelper helper)
        {
            var theme = Services.UI.CurrentTheme;
            if (theme != null)
            {
                foreach (var file in theme.Files)
                {
                    if (file.Type == Models.ReferenceFileType.Css)
                        HtmlExtensions.RegisterStylesheet(helper, file.Path, true, new Dictionary<string,string>() {{"type", "theme"}});
                    if (file.Type == Models.ReferenceFileType.Script)
                        HtmlExtensions.RegisterScript(helper, file.Path, true, new Dictionary<string, string>() { { "type", "theme" } });
                }
            }
        }

        public static MvcHtmlString RenderClientControl(this HtmlHelper helper, Models.IClientControl clientControl, string id, Models.Chart model)
        {
            model.ClientId = clientControl.GetId(id);
            helper.RenderPartial("Controls/" + model.Path, model);
            return null;
        }

        public static MvcHtmlString AuthorizedControlGroup(this HtmlHelper helper, Models.IClientControl widget, string id, string textKey, string defaultText, string dataColumn)
        {
            return helper.DropDownControlGroup(widget, id, textKey, defaultText, dataColumn, new List<SelectListItem>() {
                new SelectListItem() { Text = widget.GetPortalText("None.Text", "None"), Value = "" },
                new SelectListItem() { Text = widget.GetPortalText("Authenticated.Text", "Authenticated"), Value = "true" },
                new SelectListItem() { Text = widget.GetPortalText("NotAuthenticated.Text", "Not Authenticated"), Value = "false" }});
        }

    }

}