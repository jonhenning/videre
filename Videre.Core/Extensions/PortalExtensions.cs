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
using System.IO;
using System.Globalization;
using System.Collections.Concurrent;
using CodeEndeavors.Extensions.Serialization;

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
        public static void RenderWidgets(this HtmlHelper helper, PageTemplate template, string paneName)
        {
            if (template != null)
            {
                foreach (var widget in template.Widgets.Where(w => w.PaneName == paneName && w.IsAuthorized))
                    RenderWidget(helper, widget);

                foreach (var widget in template.LayoutWidgets.Where(w => w.PaneName == paneName && w.IsAuthorized))
                    RenderWidget(helper, widget);
            }
        }

        public static ConcurrentStack<Models.Widget> GetRenderedWidgetStack(HtmlHelper helper)
        {
            return helper.GetContextItem<ConcurrentStack<Models.Widget>>("RenderedWidgetStack");
        }
        public static ConcurrentDictionary<string, List<Models.Widget>> GetWidgetChildrenDictionary(HtmlHelper helper)
        {
            return helper.GetContextItem<ConcurrentDictionary<string, List<Models.Widget>>>("WidgetChildrenDictionary");
        }

        public static List<Models.Widget> GetWidgetChildren(HtmlHelper helper, string id)
        {
            var dict = GetWidgetChildrenDictionary(helper);
            if (!dict.ContainsKey(id))
                dict[id] = new List<Models.Widget>();
            return dict[id];
        }

        public static ConcurrentDictionary<string, List<string>> GetWidgetControlDictionary(HtmlHelper helper)
        {
            return helper.GetContextItem<ConcurrentDictionary<string, List<string>>>("WidgetControlDictionary");
        }

        public static List<string> GetWidgetControls(HtmlHelper helper, string id)
        {
            var dict = GetWidgetControlDictionary(helper);
            if (!dict.ContainsKey(id))
                dict[id] = new List<string>();
            return dict[id];
        }

        public static void RenderWidget(this HtmlHelper helper, Widget widget, bool defer = false)
        {
            if (widget.IsAuthorized)
            {
                try
                {
                    if (!defer)
                    {
                        Models.Widget parent = null;
                        var widgetStack = GetRenderedWidgetStack(helper);
                        if (widgetStack.IsEmpty)
                            parent = new Widget() { ClientId = "ROOT" };
                        else
                            widgetStack.TryPeek(out parent);

                        widget.ClientId = Portal.NextClientId();
                        GetWidgetChildren(helper, parent.ClientId).Add(widget);

                        widgetStack.Push(widget);     //need to be able to detect if ANY parent was a RenderAsPackage

                        using (Videre.Core.Services.Profiler.Timeline.Capture("Rendering Widget: " + widget.Manifest.Name))
                        {
                            if (widget.CacheTime.HasValue || widget.RenderAsPackage)
                            {
                                var key = Services.Widget.GetWidgetCacheKey(widget);
                                var scriptTypes = new List<string>() { "js", "documentreadyendjs", "documentreadyjs", "css", "inlinejs" };
                                var originalScripts = scriptTypes.Select(t => new { type = t, list = WebReferenceBundler.GetReferenceList(helper, t) }).JsonClone();
                                var originalAttemptedScripts = scriptTypes.Select(t => new { type = t, list = WebReferenceBundler.GetAttemptedRegistrationReferenceList(helper, t) }).JsonClone();
                                var originalRegisteredKeys = HtmlExtensions.GetRegisteredKeys(helper).JsonClone();
                                //var originalRegisteredGroups = Services.WebReferenceBundler.GetWebReferenceGroups(helper).JsonClone();
                                var pulledFromCache = true;
                                renderData renderedData = null;

                                if (widget.CacheTime.HasValue)
                                {
                                    if (helper.ViewContext.RequestContext.HttpContext.Request["nocache"] == "1")
                                        CodeEndeavors.Distributed.Cache.Client.Service.ExpireCacheEntry("VidereWidgetCache", key);

                                    renderedData = CodeEndeavors.Distributed.Cache.Client.Service.GetCacheEntry("VidereWidgetCache", TimeSpan.FromSeconds(widget.CacheTime.Value), key, () =>
                                    {
                                        pulledFromCache = false;
                                        var ret = getRenderData(helper, widget, scriptTypes);
                                        ret.registeredPackageScripts = HtmlExtensions.GetRegisteredPackageScripts(helper);  //no need to determine new ones...   registration will only register once
                                        return ret;
                                    });
                                }
                                else
                                {
                                    pulledFromCache = false;
                                    renderedData = getRenderData(helper, widget, scriptTypes);
                                }

                                if (pulledFromCache)    //if pulled from cache we need to register the references that would have been rendered
                                {
                                    Portal.SetCurrentClientId(Portal.GetCurrentClientId() + renderedData.numberOfClientIdsTaken);

                                    //Register url segments so cached widgets can have access to id even though id script is cached as previous one
                                    if (Core.Services.Portal.CurrentUrlMatchedGroups.Count > 0)
                                        HtmlExtensions.RegisterDocumentReadyScript(helper, "currentMatchedUrlGroups", "videre._currentMatchedUrlGroups = " + Core.Services.Portal.CurrentUrlMatchedGroups.ToJson());

                                    //don't register script if load on demand, will do from client
                                    if (widget.RenderAsPackage)
                                    {
                                        //we want to only register js and css (always) for perf
                                        foreach (var scriptType in renderedData.deltaScriptDict.Keys)
                                        {
                                            if (scriptType == "js" || scriptType == "css")
                                                helper.RegisterReferenceListItems(scriptType, renderedData.deltaScriptDict[scriptType]);
                                        }
                                    }
                                    else //if (!widget.RenderAsPackage)
                                    {
                                        foreach (var scriptType in renderedData.deltaScriptDict.Keys)
                                            helper.RegisterReferenceListItems(scriptType, renderedData.deltaScriptDict[scriptType]);

                                        foreach (var regKey in renderedData.newlyRegisteredKeys)  //register any keys that would have been registered prior
                                        {
                                            if (!HtmlExtensions.IsKeyRegistered(helper, regKey))
                                                HtmlExtensions.RegisterKey(helper, regKey);
                                        }

                                        foreach (var regKey in renderedData.newlyRegisteredClientLocalizations.Keys)
                                            HtmlExtensions.RegisterClientLocalizations(helper, regKey, renderedData.newlyRegisteredClientLocalizations[regKey].ToDictionary(entry => entry.Key, entry => entry.Value));

                                        foreach (var item in renderedData.registeredPackageScripts)
                                            HtmlExtensions.RegisterPackageScript(helper, item.RegistrationKey, item.Text);

                                    }
                                }
                                var keysToKeep = new List<string>();
                                if (widget.RenderAsPackage)
                                {
                                    //reset scripts - since we needed to use writer to get contents, we need to remove scripts for ondemand as we don't need them yet
                                    foreach (var scripts in originalScripts)
                                    {
                                        if (scripts.type != "js" && scripts.type != "css")  //we want to only register js and css (always) for perf
                                        {
                                            var referenceList = WebReferenceBundler.GetReferenceList(helper, scripts.type);
                                            //if (referenceList.ContainsKey(scripts.type))
                                            if (referenceList != null)
                                            {
                                                referenceList.Clear();
                                                referenceList.AddRange(scripts.list);
                                            }
                                        }
                                        else
                                            keysToKeep.AddRange(scripts.list.Select(s => s.RegistrationKey));
                                    }

                                    //reset attempted scripts as well - difference between attempted and regular is that regular won't attempt to register if already been registered once...  
                                    foreach (var scripts in originalAttemptedScripts)
                                    {
                                        if (scripts.type != "js" && scripts.type != "css")  //we want to only register js and css (always) for perf
                                        {
                                            var referenceList = WebReferenceBundler.GetAttemptedRegistrationReferenceList(helper, scripts.type);
                                            //if (referenceList.ContainsKey(scripts.type))
                                            if (referenceList != null)
                                            {
                                                referenceList.Clear();
                                                referenceList.AddRange(scripts.list);
                                            }
                                        }
                                        else
                                            keysToKeep.AddRange(scripts.list.Select(s => s.RegistrationKey));
                                    }

                                    //clear registeredKeys
                                    var keys = HtmlExtensions.GetRegisteredKeys(helper);
                                    keys.Where(k => !originalRegisteredKeys.Contains(k) && !keysToKeep.Contains(k)).ToList().ForEach(k => HtmlExtensions.UnregisterKey(helper, k));


                                    //clear registered groups
                                    //var groups = WebReferenceBundler.GetWebReferenceGroups(helper);
                                    //if (groups != null)
                                    //{
                                    //    groups.Clear();
                                    //    groups.AddRange(originalRegisteredGroups);
                                    //}

                                    //registerPackage: function(clientId, type, pkg)
                                    helper.RegisterPackageScript(widget.ClientId + "RegisterPackage", string.Format("videre.widgets.registerPackage({0});", renderedData.ToJson(pretty: false, ignoreType: "client").Replace("</", "<\\/")));   //Replace to allow closing </script> tags in html, not sure I fully understand this, nor whether this should be in more locations - JH - 7/9/2014
                                }
                                else
                                    helper.ViewContext.Writer.Write(renderedData.html);   //write out html for widget
                            }
                            else
                            {
                                helper.RenderPartial("Widgets/" + widget.Manifest.FullName, widget);
                            }

                            helper.RegisterWebReferences(widget.WebReferences);
                        }
                    }
                    else
                        DeferredWidgets.Add(widget);
                }
                catch (Exception ex)
                {
                    helper.RenderPartial("Widgets/Core/Error", widget, new ViewDataDictionary { { "Exception", ex } });
                }
                finally
                {
                    Models.Widget o;
                    var s = GetRenderedWidgetStack(helper);
                    s.TryPop(out o);
                }
            }
        }

        //todo: move?
        private class renderData
        {
            public string clientId { get; set; }
            public string clientPresenterType { get; set; }
            public Dictionary<string, IEnumerable<ReferenceListItem>> deltaScriptDict { get; set; }
            public int numberOfClientIdsTaken { get; set; }
            [SerializeIgnore(new string[] { "client" })]
            public List<string> newlyRegisteredKeys { get; set; }
            [SerializeIgnore(new string[] { "client" })]
            public ConcurrentDictionary<string, ConcurrentDictionary<string, string>> newlyRegisteredClientLocalizations { get; set; }
            [SerializeIgnore(new string[] { "client" })]
            public List<Models.ReferenceListItem> registeredPackageScripts { get; set; }
            public string html { get; set; }
        }

        private static renderData getRenderData(HtmlHelper helper, Models.Widget widget, List<string> scriptTypes)
        {
            var scriptCounts = scriptTypes.Select(t => new { type = t, scriptCount = WebReferenceBundler.GetAttemptedRegistrationReferenceList(helper, t).Count }).ToDictionary(t => t.type, t => t.scriptCount);
            var currentClientId = Portal.GetCurrentClientId();
            var registeredKeys = HtmlExtensions.GetRegisteredKeys(helper);
            var registeredClientLocalizationKeys = HtmlExtensions.GetRegisteredClientLocalizations(helper).Keys;

            var cachingWriter = new StringWriter(CultureInfo.InvariantCulture);
            var originalWriter = helper.ViewContext.Writer;
            helper.ViewContext.Writer = cachingWriter;
            helper.RenderPartial("Widgets/" + widget.Manifest.FullName, widget);
            helper.ViewContext.Writer = originalWriter;

            //determine new scripts registered
            var deltaScriptDict = new Dictionary<string, IEnumerable<ReferenceListItem>>();
            foreach (var scriptType in scriptCounts.Keys)
            {
                var scripts = WebReferenceBundler.GetAttemptedRegistrationReferenceList(helper, scriptType); //get newly registered scripts
                if (scripts.Count > scriptCounts[scriptType])
                {
                    //deltaScriptDict[scriptType] = scripts.Skip(scriptCounts[scriptType]).JsonClone();

                    var newScripts = scripts.Skip(scriptCounts[scriptType]).JsonClone();
                    //IMPORTANT:  attempted list does not look for duplicates... though it should be done in videre core... we need to only have non-duplicates here...
                    newScripts = newScripts.Distinct(s => s.RegistrationKey).ToList();  //todo: does this keep order???  it needs to!
                    deltaScriptDict[scriptType] = newScripts;
                }
            }
            var currentRegisteredKeys = HtmlExtensions.GetRegisteredKeys(helper);
            var newlyRegisteredKeys = new List<string>();
            foreach (var regKey in currentRegisteredKeys)
            {
                if (!registeredKeys.Contains(regKey))
                    newlyRegisteredKeys.Add(regKey);
            }
            var currentRegisterdClientLocalizations = HtmlExtensions.GetRegisteredClientLocalizations(helper);
            var newlyRegisteredClientLocalizations = new ConcurrentDictionary<string, ConcurrentDictionary<string, string>>();
            foreach (var regKey in currentRegisterdClientLocalizations.Keys)
            {
                if (!registeredClientLocalizationKeys.Contains(regKey))
                    newlyRegisteredClientLocalizations[regKey] = currentRegisterdClientLocalizations[regKey].JsonClone();
            }

            return new renderData() { clientId = widget.ClientId, html = cachingWriter.ToString(), deltaScriptDict = deltaScriptDict, numberOfClientIdsTaken = Portal.GetCurrentClientId() - currentClientId, newlyRegisteredKeys = newlyRegisteredKeys, newlyRegisteredClientLocalizations = newlyRegisteredClientLocalizations, clientPresenterType = widget.ClientPresenterType };
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

        //default param values cannot be added to and maintain compatibility (i.e. referencing code needs to be recompiled)...  it is a compiler trick
        public static void RenderWidget(this HtmlHelper helper, string manifestFullName, Dictionary<string, object> attributes = null, bool defer = false, string css = null, string style = null)
        {
            RenderWidget(helper, manifestFullName, null, null, attributes, defer, css, style);
        }

        public static void RenderWidget(this HtmlHelper helper, string manifestFullName, int? cacheTime, List<string> cacheKeys, Dictionary<string, object> attributes, bool defer, string css, string style)
        {
            RenderWidget(helper, manifestFullName, cacheTime, cacheKeys, attributes, defer, css, style, false);
        }
        public static void RenderWidget(this HtmlHelper helper, string manifestFullName, int? cacheTime, List<string> cacheKeys, Dictionary<string, object> attributes, bool defer, string css, string style, bool renderAsPackage)
        {
            var manifest = Services.Widget.GetWidgetManifest(manifestFullName);
            var widget = new Widget { ManifestId = manifest.Id, Css = css, Style = style };
            if (attributes != null)
                widget.Attributes = attributes;
            if (cacheTime.HasValue)
            {
                widget.CacheTime = cacheTime;
                widget.CacheKeys = cacheKeys;
            }
            widget.RenderAsPackage = renderAsPackage;

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

                Models.Widget parent = null;
                var widgetStack = GetRenderedWidgetStack(helper);
                //if (widgetStack.IsEmpty)
                //    parent = new Widget() { ClientId = "ROOT" };
                //else
                    widgetStack.TryPeek(out parent);

                var pathAndName = path.PathCombine(name, "/");
                GetWidgetControls(helper, parent.ClientId).Add(pathAndName);

                //it would be nice if the ViewDataDictionary accepted anonymous objects
                helper.RenderPartial("Controls/" + pathAndName, control, new ViewDataDictionary(viewData.ToJson().ToObject<ViewDataDictionary>()));
                return control.ClientId;
            }
        }

        public static void RenderWidgetEditor(this HtmlHelper helper, WidgetManifest manifest)
        {
            helper.RegisterScript("~/scripts/widgets/videre.widgets.editor.base.js", true);

            var path = string.IsNullOrEmpty(manifest.EditorPath) ? "Widgets/Core/CommonEditor" : manifest.EditorPath;
            if (!HtmlExtensions.IsKeyRegistered(helper, path.ToLower()))
            {
                helper.RenderPartial(path, new WidgetEditor(manifest.Id) { ClientId = Portal.NextClientId() });
                HtmlExtensions.RegisterKey(helper, path.ToLower());
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
            var locale = Services.Localization.CurrentUserLocale;
            var timeZone = Account.GetUserTimeZone() != null ? Account.GetUserTimeZone().Id : "";
            helper.RegisterScript(string.Format("~/ServerJS/GlobalClientTranslations?l={0}&tz={1}", locale, timeZone), true, excludeFromBundle: true);

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
                        helper.RegisterStylesheet(file.Path, true, new Dictionary<string, string> { { "type", "theme" } });
                    if (file.Type == ReferenceFileType.Script)
                        helper.RegisterScript(file.Path, true, new Dictionary<string, string> { { "type", "theme" } });
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