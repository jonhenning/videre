using System;
using System.Linq;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;
using System.Web;
using System.Web.Mvc;
using System.Web.Mvc.Html;
using CodeEndeavors.Extensions;
using System.Collections.Concurrent;
using System.Web.Helpers;

namespace Videre.Core.Extensions
{
    public static class HtmlExtensions
    {
        public static void RegisterWebReferences(this HtmlHelper helper, List<string> names)
        {
            if (names != null)
                names.ForEach(n => RegisterWebReference(helper, n));
        }

        public static void RegisterWebReference(this HtmlHelper helper, string name)
        {
            var refs = Services.Web.GetWebReferences();
            //if (refs.Count == 0)    //todo: detect compat mode (query string???)
            //    refs = Services.Web.GetDefaultWebReferences();
            var wr = refs.Where(r => r.Name.Equals(name, StringComparison.InvariantCultureIgnoreCase)).FirstOrDefault();

            if (wr != null)
            {
                foreach (var dep in wr.DependencyGroups)
                    RegisterWebReferenceGroup(helper, dep);

                if (wr.Type == Models.WebReferenceType.ScriptReference)
                    helper.RegisterScript(wr.Url, wr.LoadType == Models.WebReferenceLoadType.Defer, excludeFromBundle: wr.ExcludeFromBundle);
                else if (wr.Type == Models.WebReferenceType.StyleSheetReference)
                    helper.RegisterStylesheet(wr.Url, wr.LoadType == Models.WebReferenceLoadType.Defer, excludeFromBundle: wr.ExcludeFromBundle);
                else if (wr.Type == Models.WebReferenceType.Script)
                {
                    if (wr.LoadType == Models.WebReferenceLoadType.Inline)
                        helper.RegisterScript(wr.Name, wr.Text);
                    else
                        helper.RegisterDocumentReadyScript(wr.Name, wr.Text, wr.LoadType == Models.WebReferenceLoadType.EndStartup);
                }
                else if (wr.Type == Models.WebReferenceType.StyleSheet)
                {
                    //if (wr.LoadType == Models.WebReferenceLoadType.Inline)
                        helper.RegisterStylesheet(wr.Name, wr.Text);
                    //else
                }
            }
        }

        public static void RegisterWebReferenceGroup(this HtmlHelper helper, string name)
        {
            var refs = Services.Web.GetWebReferences(); 
            //if (refs.Count == 0)    //todo: detect compat mode (query string???)
            //    refs = Services.Web.GetDefaultWebReferences();
            var groupRefs = refs.Where(r => r.Group.Equals(name, StringComparison.InvariantCultureIgnoreCase)).OrderByDescending(r => r.DependencyGroups.Count);
            foreach (var r in groupRefs.OrderBy(r => r.Sequence))
                RegisterWebReference(helper, r.Name);
        }
        
        //public static void RegisterDataTableScript(this HtmlHelper helper)
        //{
        //    helper.RegisterWebReferenceGroup("datatables");
        //    //helper.RegisterScript("~/Scripts/DataTables-1.9.0/media/js/jquery.dataTables.min.js", true);
        //    //helper.RegisterScript("~/Scripts/DataTables-1.9.0/media/js/jquery.dataTables.bootstrap.js", true);
        //    //helper.RegisterStylesheet("~/Scripts/DataTables-1.9.0/media/css/jquery.dataTables.bootstrap.css", true);
        //}

        public static void RegisterPrettifyScriptIfNeeded(this HtmlHelper helper, string text)
        {
            if (text.Contains("prettyprint"))
                helper.RegisterWebReferenceGroup("prettify");
        }

        //public static void RegisterPrettifyScript(this HtmlHelper helper)
        //{
        //    helper.RegisterWebReferenceGroup("prettify");
        //    //helper.RegisterScript("~/Scripts/prettify/prettify.js", true);
        //    //helper.RegisterStylesheet("~/Scripts/prettify/prettify.css", true);
        //    //helper.RegisterDocumentReadyScript("prettyPrint", "prettyPrint();", true);
        //}
        //public static void RegisterTreeScript(this HtmlHelper helper)
        //{
        //    helper.RegisterWebReferenceGroup("dynatree");
        //    //helper.RegisterScript("~/scripts/dynatree1.1/jquery.dynatree.min.js", true);
        //    //helper.RegisterStylesheet("~/scripts/dynatree1.1/skin/ui.dynatree.css", true);
        //}
        //public static void RegisterDateTimePicker(this HtmlHelper helper)
        //{
        //    helper.RegisterWebReferenceGroup("timepicker");
        //    //helper.RegisterScript("~/scripts/jquery-ui-timepicker-1.0.1/jquery-ui-timepicker-addon.js", true);
        //    //helper.RegisterStylesheet("~/scripts/jquery-ui-timepicker-1.0.1/jquery-ui-timepicker-addon.css", true);
        //}

        public static void RegisterAntiForgeryVerificationToken(this HtmlHelper helper)
        {
            if (Services.API.AntiForgeryTokenVerification)
            {
                //string cookieToken;
                //string formToken;
                //AntiForgery.GetTokens(null, out cookieToken, out formToken);
                helper.RegisterDocumentReadyScript("AntiForgeryToken", string.Format("videre._csrfToken = '{0}';", Services.API.GetAntiForgeryToken()));//'{0}:{1}'", cookieToken, formToken));
            }
        }

        public static void RegisterScript(this HtmlHelper helper, string key, string script)
        {
            if (!IsKeyRegistered(helper, key))
            {
                //todo: test
                helper.ViewContext.HttpContext.Response.Write(string.Format("<script type=\"text/javascript\">{0}</script>", script));
            }
        }

        public static void RegisterDocumentReadyScript(this HtmlHelper helper, string key, string script, bool runAtEnd = false)
        {
            //lock (_lockObj)
            //{
            if (!IsKeyRegistered(helper, key))
            {
                if (runAtEnd)
                {
                    Services.WebReferenceBundler.GetReferenceList(helper, "documentreadyendjs").Add(new Models.ReferenceListItem() { Src = key, Text = script } );
                }
                else
                {
                    Services.WebReferenceBundler.GetReferenceList(helper, "documentreadyjs").Add(new Models.ReferenceListItem() { Src = key, Text = script });
                    RegisterKey(helper, key);
                }
            }
            //}
        }
        public static void RegisterScript(this HtmlHelper helper, string src, bool defer = true, Dictionary<string, string> dataAttributes = null, bool excludeFromBundle = false)
        {
            //lock (_lockObj)
            //{
                if (!IsKeyRegistered(helper, src))
                {
                    if (defer)
                        Services.WebReferenceBundler.GetReferenceList(helper, "js").Add(new Models.ReferenceListItem() { Src = GetPath(src).ToLower(), DataAttributes = dataAttributes, ExcludeFromBundle = excludeFromBundle });
                    else
                        helper.ViewContext.HttpContext.Response.Write(string.Format("<script src=\"{0}\" type=\"text/javascript\" {1}></script>", GetPath(src).ToLower(), GetDataAttributeMarkup(dataAttributes)));
                    RegisterKey(helper, src);
                }
            //}
        }

        public static void RegisterStylesheet(this HtmlHelper helper, string key, string css)
        {
            if (!IsKeyRegistered(helper, key))
            {
                //todo: test
                helper.ViewContext.HttpContext.Response.Write(string.Format("<style>{0}</style>", css));
            }
        }

        public static void RegisterStylesheet(this HtmlHelper helper, string src, bool defer = true, Dictionary<string, string> dataAttributes = null, bool excludeFromBundle = false)
        {
            //lock (_lockObj)
            //{
                if (!IsKeyRegistered(helper, src))
                {
                    if (defer)
                        Services.WebReferenceBundler.GetReferenceList(helper, "css").Add(new Models.ReferenceListItem() { Src = GetPath(src).ToLower(), DataAttributes = dataAttributes, ExcludeFromBundle = excludeFromBundle });
                    else
                        helper.ViewContext.HttpContext.Response.Write(string.Format("<link href=\"{0}\" type=\"text/css\" rel=\"stylesheet\" {1} />", GetPath(src).ToLower(), GetDataAttributeMarkup(dataAttributes)));
                    RegisterKey(helper, src);
                }
            //}
        }
        public static string RenderScripts(this HtmlHelper helper)
        {
            return Services.WebReferenceBundler.GenerateUnrenderedScriptMarkup(helper);
        }
        
        public static string RenderStylesheets(this HtmlHelper helper)
        {
            return Services.WebReferenceBundler.GenerateUnrenderedStylesheetMarkup(helper);
        }

        public static void ScriptMarkup(this HtmlHelper helper, string key, string script)
        {
            if (!IsKeyRegistered(helper, key))
            {
                Services.WebReferenceBundler.GetReferenceList(helper, "inlinejs").Add(new Models.ReferenceListItem() { Src = key, Text = script });
                RegisterKey(helper, key);
            }
        }

        public static MvcHtmlString DefinitionListItem(this HtmlHelper helper, Models.IClientControl widget, string labelKey, string defaultLabel, string text, string separator = null, string labelCss = null, string textCss = null)
        {
            return new MvcHtmlString(string.Format("<dt class=\"{2}\" title=\"{0}\">{0}{4}</dt><dd class=\"{3}\">{1}</dd>", HttpUtility.HtmlEncode(widget.GetText(labelKey, defaultLabel)), text, labelCss, textCss, separator));
        }
        public static MvcHtmlString DefinitionListItem(this HtmlHelper helper, Models.IClientControl widget, string labelKey, string defaultLabel, string id, string dataColumn, IEnumerable<SelectListItem> selectList, string inputCss = null, SelectListItem blankItem = null, string separator = null, string labelCss = null, string textCss = null)
        {
            var list = selectList.ToList(); //todo: minor little hack...
            if (blankItem != null)
                list.Insert(0, blankItem);
            return new MvcHtmlString(string.Format("<dt class=\"{2}\" title=\"{0}\">{0}{4}</dt><dd class=\"{3}\">{1}</dd>", HttpUtility.HtmlEncode(widget.GetText(labelKey, defaultLabel)), helper.DropDownList(id, list, new { @class = inputCss, data_column = dataColumn }).ToString(), labelCss, textCss, separator));
        }

        //public static MvcHtmlString LabelControlGroup(this HtmlHelper helper, Models.IClientControl widget, string id, string textKey, string defaultText, string dataColumn, string css = null)
        //{
        //    return GetControlGroup(widget, id, textKey, defaultText,
        //                        string.Format("<label class=\"{2}\" id=\"{0}\" {1}></label>",
        //                            widget.GetId(id), GetDataAttributeMarkup(GetDataAttributeDict(dataColumn)), css));
        //}

        //public static MvcHtmlString InputControlGroup(this HtmlHelper helper, Models.IClientControl widget, string id, string textKey, string defaultText, string dataColumn = null, string inputCss = null, string inputType = null, bool readOnly = false, bool required = false, string dataType = null, string valueMatchControl = null, bool disableAutoComplete = false, int? maxLength = null, string placeHolderText = null)
        //{
        //    if (!string.IsNullOrEmpty(valueMatchControl))
        //        valueMatchControl = widget.GetId(valueMatchControl);    //todo: right place for this?

        //    if (dataType == "datetime") //todo: auto do this?
        //        helper.RegisterWebReferenceGroup("timepicker");

        //    return InputControlGroup(helper, widget, id, textKey, defaultText, GetDataAttributeDict(dataColumn, dataType: dataType, valueMatchControl: valueMatchControl), required, inputCss, inputType, readOnly, disableAutoComplete, maxLength, placeHolderText);
        //}
        //public static MvcHtmlString InputControlGroup(this HtmlHelper helper, Models.IClientControl widget, string id, string textKey, string defaultText, Dictionary<string, string> dataAttributes, bool required, string inputCss = null, string inputType = null, bool readOnly = false, bool disableAutoComplete = false, int? maxLength = null, string placeHolderText = null)
        //{
        //    return GetControlGroup(widget, id, textKey, defaultText, 
        //                        string.Format("<input type=\"{3}\" class=\"{2}\" id=\"{0}\" name=\"{0}\" {1} {4} {5} {6} {7} {8}/>",
        //                        widget.GetId(id), GetDataAttributeMarkup(dataAttributes), inputCss, string.IsNullOrEmpty(inputType) ? "text" : inputType, readOnly ? "readonly=\"readonly\"" : "", required ? "required=\"required\"" : "", disableAutoComplete ? "autocomplete=\"off\"" : "", maxLength.HasValue ? string.Format("maxlength=\"{0}\"", maxLength) : "", !string.IsNullOrEmpty(placeHolderText) ? string.Format("placeholder=\"{0}\"", placeHolderText) : ""));
        //}

        //public static MvcHtmlString InputFileBrowserControlGroup(this HtmlHelper helper, Models.IClientControl widget, string id, string textKey, string defaultText, string dataColumn, string inputCss = null, string mimeType = "", bool required = false)
        //{
        //    return InputFileBrowserControlGroup(helper, widget, id, textKey, defaultText, GetDataAttributeDict(dataColumn), inputCss, mimeType);
        //}       
        //public static MvcHtmlString InputFileBrowserControlGroup(this HtmlHelper helper, Models.IClientControl widget, string id, string textKey, string defaultText, Dictionary<string, string> dataAttributes, string inputCss = null, string mimeType = "", bool required = false)
        //{
        //    //ONLY REGISTER ONCE AND AT END?!?!
        //    helper.RenderWidget("Core/Admin/FileBrowser", new Dictionary<string, object>() { { "MimeType", mimeType } }, true);

        //    return GetControlGroup(widget, id, textKey, defaultText,  
        //                            string.Format("   <input type=\"text\" class=\"{2}\" id=\"{0}\" {1} {3} />" +
        //                            "   <a class=\"btn\" data-action=\"filebrowser\" data-control=\"{0}\" ><i class=\"icon-picture\"></i></a>",
        //                            widget.GetId(id), GetDataAttributeMarkup(dataAttributes), inputCss, required ? "required=\"required\"" : ""), "input-append");
        //}

        //public static MvcHtmlString UploadButtonControlGroup(this HtmlHelper helper, Models.IClientControl widget, string id, string textKey, string defaultText, string buttonTextKey, string defaultButtonText, string inputCss = null, string inputType = null)
        //{
        //    //helper.RegisterScript("~/scripts/fileuploader.js", true);
        //    helper.RegisterWebReference("fileuploader");
        //    return GetControlGroup(widget, id, textKey, defaultText, 
        //                            string.Format("   <a class=\"btn {1}\" id=\"{0}\" >{2}</a>",
        //                        widget.GetId(id), inputCss, HttpUtility.HtmlEncode(widget.GetText(buttonTextKey, defaultButtonText))));
        //}

        //public static MvcHtmlString TextAreaControlGroup(this HtmlHelper helper, Models.IClientControl widget, string id, string textKey, string defaultText, string dataColumn, string controlType = "", string inputCss = "", int rows = 3, bool required = false, bool readOnly = false)
        //{
        //    return TextAreaControlGroup(helper, widget, id, textKey, defaultText, GetDataAttributeDict(dataColumn, controlType), required, inputCss, rows, readOnly);
        //}
        //public static MvcHtmlString TextAreaControlGroup(this HtmlHelper helper, Models.IClientControl widget, string id, string textKey, string defaultText, Dictionary<string, string> dataAttributes, bool required, string inputCss = "", int rows = 3, bool readOnly = false )
        //{
        //    return GetControlGroup(widget, id, textKey, defaultText, 
        //                            string.Format("<textarea type=\"text\" class=\"{2}\" id=\"{0}\" {1} rows=\"{3}\" {4} {5}></textarea>",
        //                        widget.GetId(id), GetDataAttributeMarkup(dataAttributes), inputCss, rows, required ? "required=\"required\"" : "", readOnly ? "readonly=\"readonly\"" : ""));
        //}

        //public static MvcHtmlString TextEditorControl(this HtmlHelper helper, Models.IClientControl clientControl, string id, string dataColumn, bool required = false, string labelText = null)
        //{
        //    var model = new Models.DataControl(dataColumn, Services.Portal.CurrentPortal.GetAttribute("Core", "TextEditor", "Core/CKTextEditor"));
        //    model.ClientId = clientControl.GetId(id);
        //    model.Required = required;
        //    model.LabelText = labelText;
        //    helper.RenderPartial("Controls/" + model.Path, model);
        //    return null;
        //}

        //public static MvcHtmlString RoleControlGroup(this HtmlHelper helper, Models.IClientControl widget, string id, string textKey, string defaultText, string dataColumn, List<string> selectedRoles = null, string inputCss = null, bool required = false)
        //{
        //    return RoleControlGroup(helper, widget, id, textKey, defaultText, GetDataAttributeDict(dataColumn, "multiselect"), required, selectedRoles, inputCss);
        //}
        //public static MvcHtmlString RoleControlGroup(this HtmlHelper helper, Models.IClientControl widget, string id, string textKey, string defaultText, Dictionary<string, string> dataAttributes, bool required, List<string> selectedRoles = null, string inputCss = null)
        //{
        //    selectedRoles = selectedRoles == null ? new List<string>() : selectedRoles;
        //    var items = Services.Account.GetRoles().Select(r => new SelectListItem() { Value = r.Id, Text = r.Name, Selected = selectedRoles.Contains(r.Id) }).ToList();
        //    return MultiSelectControlGroup(helper, widget, id, textKey, defaultText, dataAttributes, required, items, inputCss);
        //}

        //public static MvcHtmlString MultiSelectControlGroup(this HtmlHelper helper, Models.IClientControl widget, string id, string textKey, string defaultText, string dataColumn, bool required, List<SelectListItem> items, string inputCss = null)
        //{
        //    return MultiSelectControlGroup(helper, widget, id, textKey, defaultText, GetDataAttributeDict(dataColumn, "multiselect"), required, items, inputCss);
        //}

        //public static MvcHtmlString MultiSelectControlGroup(this HtmlHelper helper, Models.IClientControl widget, string id, string textKey, string defaultText, Dictionary<string, string> dataAttributes, bool required, List<SelectListItem> items, string inputCss = null)
        //{
        //    var clientId = widget.GetId(id);
            
        //    helper.RegisterWebReferenceGroup("multiselect");
        //    //helper.RegisterScript("~/scripts/multiselect/jquery.multiselect.min.js", true);
        //    //helper.RegisterStylesheet("~/scripts/multiselect/jquery.multiselect.css", true);

        //    //todo: localize text (Select Options)
        //    //helper.RegisterDocumentReadyScript("multiselect-init", string.Format("$('select[data-controltype=\"multiselect\"]').multiselect({{ selectedList: 3 }});", clientId));

        //    var options = new System.Text.StringBuilder();
        //    foreach (var item in items)
        //        options.AppendLine(string.Format("<option {0} value=\"{1}\">{2}</option>", item.Selected ? "selected=\"selected\" " : "", item.Value, HttpUtility.HtmlEncode(item.Text)));

        //    return GetControlGroup(widget, id, textKey, defaultText,
        //                            string.Format("   <select class=\"{2}\" id=\"{0}\" {1}  multiple=\"multiple\" {4}>{3}</select>",
        //                        clientId, GetDataAttributeMarkup(dataAttributes), inputCss, options.ToString(), required ? "required=\"required\"" : ""));
        //}

        //public static MvcHtmlString DropDownControlGroup(this HtmlHelper helper, Models.IClientControl widget, string id, string textKey, string defaultText, string dataColumn, IEnumerable<SelectListItem> selectList, string inputCss = null, SelectListItem blankItem = null)
        //{
        //    var list = selectList.ToList(); //todo: minor little hack...
        //    if (blankItem != null)
        //        list.Insert(0, blankItem);
        //    return GetControlGroup(widget, id, textKey, defaultText, helper.DropDownList(widget.GetId(id), list, new {@class = inputCss, data_column = dataColumn}).ToString());
        //}

        public static List<SelectListItem> ToListItemList(this Enum type)//<T>(this T type) where T : Type//struct //todo: where is the enum constraint?!?!
        {
            //var names = Enum.GetNames(t);
            //var values = Enum.GetValues(t);
            //var ret = new List<SelectListItem>();
            //for (var i = 0; i < names.Length; i++)
            //    ret.Add(new SelectListItem() { Text = names[i], Value = values[i] });
            //return ret;
            //return Enum.GetNames(typeof(T)).Select(t => new SelectListItem() { Value = ((int)t.ToType<T>()).ToString(), Text = t });
            
            //return Enum.GetValues(typeof(T)).OfType<int>().Select(v => new SelectListItem() { Value = v.ToString(), Text = Enum.GetName(typeof(T), v) }).ToList();
            var ret = new List<SelectListItem>();
            foreach (var v in Enum.GetValues(type.GetType()))
                ret.Add(new SelectListItem() { Text = v.ToString(), Value = ((int)v).ToString() });
            return ret;           
            //return Enum.GetValues(type.GetType()).OfType<T>().Select(v => new SelectListItem() { Value = ((int)v).ToString(), Text = Enum.GetName(type.GetType(), v) }).ToList();
        }

        // Properties
        public static string RootPath
        {
            get
            {
                return VirtualPathUtility.ToAbsolute("~/");
            }
        }

        public static bool IsKeyRegistered(HtmlHelper helper, string key)
        {
            var dict = GetRegisteredKeyDict(helper);
            return dict.ContainsKey(key.ToLower());
        }

        public static void RegisterKey(HtmlHelper helper, string key)
        {
            var dict = GetRegisteredKeyDict(helper);
            dict[key.ToLower()] = true;
        }

        public static T GetContextItem<T>(this HtmlHelper helper, string key) where T : class, new()
        {
            T o = helper.ViewContext.HttpContext.Items.GetSetting<T>(key, null);
            if (o == null)
            {
                o = new T();
                helper.ViewContext.HttpContext.Items[key] = o;
            }
            return o;
        }

        // private methods
        private static string GetPath(string src)
        {
            return src.Replace("~/", HtmlExtensions.RootPath);
        }

        private static ConcurrentDictionary<string, bool> GetRegisteredKeyDict(HtmlHelper helper)
        {
            return GetContextItem<ConcurrentDictionary<string, bool>>(helper, "KeyRegisteredDict");
        }

        private static MvcHtmlString GetControlGroup(Models.IClientControl widget, string id, string textKey, string defaultText, string controlMarkup, string controlsCss = "")
        {
            return new MvcHtmlString(string.Format(
                                "<div class=\"control-group\">" +
                                    "<label class=\"control-label\" for=\"{0}\">{1}</label>" +
                                    "<div class=\"controls {2}\">" +
                                    "   {3}" +
                                    "</div>" +
                                "</div>",
                                widget.GetId(id),
                                HttpUtility.HtmlEncode(widget.GetText(textKey, defaultText)),
                                controlsCss,
                                controlMarkup));
        }
        private static Dictionary<string, string> GetDataAttributeDict(string dataColumn, string controlType = "", string dataType = "", string valueMatchControl = "")
        {
            var dataAttributes = new Dictionary<string, string>();
            //if (required)
            //    dataAttributes["required"] = "true";
            if (!string.IsNullOrEmpty(dataColumn))
                dataAttributes["column"] = dataColumn;
            if (!string.IsNullOrEmpty(controlType))
                dataAttributes["controltype"] = controlType;
            if (!string.IsNullOrEmpty(dataType))
                dataAttributes["datatype"] = dataType;
            if (!string.IsNullOrEmpty(valueMatchControl))
                dataAttributes["match"] = valueMatchControl;
            return dataAttributes;
        }
        
        //todo: move to better location?
        public static string GetDataAttributeMarkup(Dictionary<string, string> dataAttributes)
        {
            var markup = "";
            if (dataAttributes != null)
            {
                foreach (var key in dataAttributes.Keys)
                    markup += string.Format(" data-{0}=\"{1}\"", key, dataAttributes[key]);
            }
            return markup;
        }


    }

}