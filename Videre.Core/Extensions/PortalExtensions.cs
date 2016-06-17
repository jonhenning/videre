using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Mvc.Html;
using CodeEndeavors.Extensions;
using Videre.Core.Models;
using Videre.Core.Services;
using Portal = Videre.Core.Services.Portal;
using Widget = Videre.Core.Models.Widget;

namespace Videre.Core.Extensions
{
    public static class PortalExtensions
    {
        private static List<Widget> DeferredWidgets
        {
            get
            {
                var widgets = HttpContext.Current.Items.GetSetting<List<Widget>>("DeferredWidgets", null);
                if (widgets == null)
                {
                    widgets = new List<Widget>();
                    HttpContext.Current.Items["DeferredWidgets"] = widgets;
                }
                return widgets;
            }
        }

        // Methods
        public static void RenderWidgets(this HtmlHelper helper, PageTemplate Template, string PaneName)
        {
            if (Template != null)
            {
                foreach (var widget in Template.Widgets.Where(w => w.PaneName == PaneName && w.IsAuthorized))
                    RenderWidget(helper, widget);

                foreach (var widget in Template.LayoutWidgets.Where(w => w.PaneName == PaneName && w.IsAuthorized))
                    RenderWidget(helper, widget);
            }
        }

        public static void RenderWidget(this HtmlHelper helper, Widget widget, bool defer = false)
        {
            if (widget.IsAuthorized)
            {
                if (!defer)
                {
                    using (Videre.Core.Services.Profiler.Timeline.Capture("Rendering Widget: " + widget.Manifest.Name))
                    {
                        widget.ClientId = Portal.NextClientId();
                        try
                        {
                            helper.RenderPartial("Widgets/" + widget.Manifest.FullName, widget);
                            helper.RegisterWebReferences(widget.WebReferences);
                        }
                        catch (Exception ex)
                        {
                            helper.RenderPartial("Widgets/Core/Error", widget, new ViewDataDictionary { { "Exception", ex } });
                        }
                    }
                }
                else
                    DeferredWidgets.Add(widget);
            }
        }

        public static void RenderLayoutHeader(this HtmlHelper helper)
        {
            try
            {
                if (Portal.CurrentTemplate != null)
                {
                    helper.RegisterWebReferences(Portal.CurrentTemplate.Layout.WebReferences);
                    helper.RegisterWebReferences(Portal.CurrentTemplate.WebReferences);
                }

                helper.ViewContext.Writer.WriteLine(helper.RenderScripts());
                helper.ViewContext.Writer.WriteLine(helper.RenderStylesheets());

            }
            catch (Exception ex)
            {
                helper.RenderPartial("Widgets/Core/Error", null, new ViewDataDictionary { { "Exception", ex } });
            }
        }

        public static void RenderLayoutDeferred(this HtmlHelper helper)
        {
            try
            {
                foreach (var widget in DeferredWidgets)
                    RenderWidget(helper, widget);

                //if (Portal.CurrentTemplate != null)
                //{
                //    helper.RegisterWebReferences(Portal.CurrentTemplate.Layout.WebReferences);
                //    helper.RegisterWebReferences(Portal.CurrentTemplate.WebReferences);
                //}

                helper.ViewContext.Writer.WriteLine(helper.RenderScripts());
                helper.ViewContext.Writer.WriteLine(helper.RenderStylesheets());
            }
            catch (Exception ex)
            {
                helper.RenderPartial("Widgets/Core/Error", null, new ViewDataDictionary { { "Exception", ex } });
            }
        }

        public static void RenderWidget(this HtmlHelper helper, string manifestFullName, Dictionary<string, object> attributes = null, bool defer = false, string css = null, string style = null)
        {
            var manifest = Services.Widget.GetWidgetManifest(manifestFullName);
            var widget = new Widget {ManifestId = manifest.Id, Css = css, Style = style};
            if (attributes != null)
                widget.Attributes = attributes;
            RenderWidget(helper, widget, defer);
        }

        public static void RenderSubWidget(this HtmlHelper helper, Widget widget, string view, Dictionary<string, object> attributes = null, bool separateNamespace = true)
        {
            helper.RenderPartial("Widgets/" + widget.Manifest.Path.PathCombine(view, "/"), new SubWidget(widget, separateNamespace) { Attributes = attributes });
        }

        public static void RenderSubWidget(this HtmlHelper helper, Widget widget, string view, object attributes = null, bool separateNamespace = true)
        {
            RenderSubWidget(helper, widget, view, attributes.AnonymousToDictionary(), separateNamespace);
        }

        public static string RenderControl(this HtmlHelper helper, string path, string name, object viewData = null, bool separateNamespace = true)
        {
            using (Videre.Core.Services.Profiler.Timeline.Capture("Rendering Control: " + name))
            {
                var control = new Models.Control(path);
                if (separateNamespace)
                    control.ClientId = Portal.NextClientId();
                //it would be nice if the ViewDataDictionary accepted anonymous objects
                helper.RenderPartial("Controls/" + path.PathCombine(name, "/"), control, new ViewDataDictionary(viewData.ToJson().ToObject<ViewDataDictionary>()));
                return control.ClientId;
            }
        }

        public static void RenderWidgetEditor(this HtmlHelper helper, WidgetManifest manifest)
        {
            helper.RegisterScript("~/scripts/widgets/videre.widgets.editor.base.js", true);

            var path = string.IsNullOrEmpty(manifest.EditorPath) ? "Widgets/Core/CommonEditor" : manifest.EditorPath;
            if (!HtmlExtensions.IsKeyRegistered(helper, path))
            {
                helper.RenderPartial(path, new WidgetEditor(manifest.Id) {ClientId = Portal.NextClientId()});
                HtmlExtensions.RegisterKey(helper, path);
            }
        }

        public static void RegisterControlPresenter(this HtmlHelper helper, string clientType, IClientControl model, object properties = null, bool preserveObjectReferences = false)
        {
            RegisterControlPresenter(helper, model, clientType, properties != null ? properties.AnonymousToDictionary() : null, preserveObjectReferences);
        }

        public static void RegisterControlPresenter(this HtmlHelper helper, IClientControl model, string clientType, Dictionary<string, object> properties = null, bool preserveObjectReferences = false)
        {
            RegisterControlPresenter(helper, model, clientType, model.ClientId, properties, preserveObjectReferences);
        }

        public static void RegisterControlPresenter(this HtmlHelper helper, IClientControl model, string clientType, string instanceName, Dictionary<string, object> properties = null, bool preserveObjectReferences = false)
        {
            properties = properties ?? new Dictionary<string, object>();

            // we ask the model to register itself. return value of false means it didn't so we perform "default" registration
            if (model.Register(helper, clientType, instanceName, properties, preserveObjectReferences))
                return;

            // default registration
            RegisterCoreScripts(helper);

            if (!string.IsNullOrEmpty(model.ScriptPath))
                helper.RegisterScript(model.ScriptPath + clientType + ".js");

            properties["id"] = model.ClientId; //todo: not necessary now... same as ns?
            properties["ns"] = model.ClientId;

            //not necessary since widget register will take care
            //var widget = model as Widget;
            //if (widget != null)
            //{
            //    properties["wid"] = widget.Id;
            //    properties["mns"] = widget.Manifest.FullName;
            //}

            //Properties["user"] = Services.Account.GetClientUser();
            //var ser = new System.Web.Script.Serialization.JavaScriptSerializer();   //no binders for date conversions...
            helper.RegisterDocumentReadyScript(model.ClientId + "Presenter", string.Format("videre.widgets.register('{0}', {1}, {2});", model.ClientId, clientType, properties.ToJson(false, "client", preserveObjectReferences).Replace("</", "<\\/")));   //Replace to allow closing </script> tags in html, not sure I fully understand this, nor whether this should be in more locations - JH - 7/9/2014
        }

        public static void RegisterCoreScripts(this HtmlHelper helper)
        {
            helper.RegisterWebReferenceGroup("videre");
            helper.RegisterScript("~/ServerJS/GlobalClientTranslations", true, excludeFromBundle: true);

            //todo: FIX!
            helper.ScriptMarkup("coreconstants", "var ROOT_URL = '" + HtmlExtensions.RootPath + "';");
        }

        public static void RegisterUserTimeZoneScript(this HtmlHelper helper)
        {
            var tz = Account.GetUserTimeZone();
            if (tz != null)
                helper.RegisterTimeZoneScript(tz.Id);
        }

        public static void RegisterTimeZoneScript(this HtmlHelper helper, string timeZone)
        {
            var tz = TimeZoneInfo.FindSystemTimeZoneById(timeZone);
            if (tz == null)
                throw new Exception("Unknown Timezone: " + timeZone);

            helper.RegisterWebReferenceGroup("videre");
            helper.RegisterScript("~/ServerJS/TimeZoneInformation/" + tz.Id, defer: true, excludeFromBundle: true);
        }

        public static void RegisterTheme(this HtmlHelper helper, PageTemplate template)
        {
            var theme = UI.PortalTheme;
            if (template != null && template.Layout != null && template.Layout.Theme != null)
                theme = template.Layout.Theme;
            if (template != null && template != null && template.Theme != null)
                theme = template.Theme;

            if (theme != null)
            {
                foreach (var file in theme.Files)
                {
                    if (file.Type == ReferenceFileType.Css)
                        helper.RegisterStylesheet(file.Path, true, new Dictionary<string, string> {{"type", "theme"}});
                    if (file.Type == ReferenceFileType.Script)
                        helper.RegisterScript(file.Path, true, new Dictionary<string, string> {{"type", "theme"}});
                }
            }
            else
                helper.RegisterWebReference("bootstrap css");
            //HtmlExtensions.RegisterStylesheet(helper, "~/scripts/bootstrap-2.1.0/css/bootstrap.css", true, new Dictionary<string, string>() { { "type", "theme" } });
        }

        //public static MvcHtmlString AuthorizedControlGroup(this HtmlHelper helper, IClientControl widget, string id, string textKey, string defaultText, string dataColumn)
        //{
        //    return helper.DropDownControlGroup(widget, id, textKey, defaultText, dataColumn, new List<SelectListItem>
        //    {
        //        new SelectListItem {Text = widget.GetPortalText("None.Text", "None"), Value = ""},
        //        new SelectListItem {Text = widget.GetPortalText("Authenticated.Text", "Authenticated"), Value = "true"},
        //        new SelectListItem {Text = widget.GetPortalText("NotAuthenticated.Text", "Not Authenticated"), Value = "false"}
        //    });
        //}
    }
}