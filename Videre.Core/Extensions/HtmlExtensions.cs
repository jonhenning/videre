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

namespace Videre.Core.Extensions
{
    public static class HtmlExtensions
    {
        public static void RegisterJQGridScript(this HtmlHelper helper)
        {
            helper.RegisterScript("~/Scripts/jqgrid-4.4/grid.locale-en.js", true);
            helper.RegisterScript("~/Scripts/jqgrid-4.4/jquery.jqGrid.min.js", true);
            helper.RegisterStylesheet("~/Scripts/jqgrid-4.4/ui.jqgrid.css", true);
        }
        public static void RegisterDataTableScript(this HtmlHelper helper)
        {
            helper.RegisterScript("~/Scripts/DataTables-1.9.0/media/js/jquery.dataTables.min.js", true);
            helper.RegisterScript("~/Scripts/DataTables-1.9.0/media/js/jquery.dataTables.bootstrap.js", true);
            helper.RegisterStylesheet("~/Scripts/DataTables-1.9.0/media/css/jquery.dataTables.bootstrap.css", true);
        }

        public static void RegisterPrettifyScriptIfNeeded(this HtmlHelper helper, string text)
        {
            if (text.Contains("prettyprint"))
                helper.RegisterPrettifyScript();
        }

        public static void RegisterPrettifyScript(this HtmlHelper helper)
        {
            helper.RegisterScript("~/Scripts/prettify/prettify.js", true);
            helper.RegisterStylesheet("~/Scripts/prettify/prettify.css", true);
            helper.RegisterDocumentReadyScript("prettyPrint", "prettyPrint();", true);
        }
        public static void RegisterTreeScript(this HtmlHelper helper)
        {
            helper.RegisterScript("~/scripts/dynatree1.1/jquery.dynatree.min.js", true);
            helper.RegisterStylesheet("~/scripts/dynatree1.1/skin/ui.dynatree.css", true);
        }
        public static void RegisterDateTimePicker(this HtmlHelper helper)
        {
            helper.RegisterScript("~/scripts/jquery-ui-timepicker-1.0.1/jquery-ui-timepicker-addon.js", true);
            helper.RegisterStylesheet("~/scripts/jquery-ui-timepicker-1.0.1/jquery-ui-timepicker-addon.css", true);
        }

        public static void RegisterDocumentReadyScript(this HtmlHelper helper, string key, string script, bool runAtEnd = false)
        {
            //lock (_lockObj)
            //{
            if (!IsKeyRegistered(helper, key))
            {
                if (runAtEnd)
                {
                    GetMarkupList(helper, "documentreadyendjs").Add(script);
                }
                else
                {
                    GetMarkupList(helper, "documentreadyjs").Add(script);
                    RegisterKey(helper, key);
                }
            }
            //}
        }
        public static void RegisterScript(this HtmlHelper helper, string src, bool defer = true, Dictionary<string, string> dataAttributes = null)
        {
            //lock (_lockObj)
            //{
                if (!IsKeyRegistered(helper, src))
                {
                    if (defer)
                        GetMarkupList(helper, "js").Add(GetPath(src));
                    else
                        helper.ViewContext.HttpContext.Response.Write(string.Format("<script src=\"{0}\" type=\"text/javascript\" {1}></script>", GetPath(src), GetDataAttributeMarkup(dataAttributes)));
                    RegisterKey(helper, src);
                }
            //}
        }
        public static void RegisterStylesheet(this HtmlHelper helper, string src, bool defer = true, Dictionary<string, string> dataAttributes = null)
        {
            //lock (_lockObj)
            //{
                if (!IsKeyRegistered(helper, src))
                {
                    string sMarkup = string.Format("<link href=\"{0}\" type=\"text/css\" rel=\"stylesheet\" {1} />", GetPath(src), GetDataAttributeMarkup(dataAttributes));
                    if (defer)
                        GetMarkupList(helper, "css").Add(sMarkup);
                    else
                        helper.ViewContext.HttpContext.Response.Write(sMarkup);
                    RegisterKey(helper, src);
                }
            //}
        }
        public static string RenderScripts(this HtmlHelper helper)
        {
            //lock (_lockObj)
            //{
                var sb = new System.Text.StringBuilder();

                var list = GetUnrenderedMarkupList(helper, "js");
                foreach (string src in list)
                {
                    sb.AppendLine(string.Format("<script src=\"{0}\" type=\"text/javascript\"></script>", src));
                }
                SetMarkupListRendered(helper, "js");

                list = GetUnrenderedMarkupList(helper, "inlinejs");
                if (list.Count > 0)
                {
                    sb.AppendLine(string.Format("<script type=\"text/javascript\">{0}</script>", String.Join("\r\n", list.ToArray())));
                }
                SetMarkupListRendered(helper, "inlinejs");

                list = GetUnrenderedMarkupList(helper, "documentreadyjs");
                if (list.Count > 0)
                {
                    sb.AppendLine(string.Format("<script type=\"text/javascript\">$(document).ready(function() {{{0}}});</script>", String.Join("\r\n", list.ToArray())));
                }
                SetMarkupListRendered(helper, "documentreadyjs");

                list = GetUnrenderedMarkupList(helper, "documentreadyendjs");
                if (list.Count > 0)
                {
                    sb.AppendLine(string.Format("<script type=\"text/javascript\">$(document).ready(function() {{{0}}});</script>", String.Join("\r\n", list.ToArray())));
                }
                SetMarkupListRendered(helper, "documentreadyendjs");

                return sb.ToString();
            //}
        }
        public static string RenderStylesheets(this HtmlHelper helper)
        {
            var sb = new System.Text.StringBuilder();

            var list = GetUnrenderedMarkupList(helper, "css");
            sb.AppendLine(String.Join("\r\n", list.ToArray()));
            SetMarkupListRendered(helper, "css");

            return sb.ToString();
        }
        public static void ScriptMarkup(this HtmlHelper helper, string key, string script)
        {
            if (!IsKeyRegistered(helper, key))
            {
                GetMarkupList(helper, "inlinejs").Add(script);
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

        public static MvcHtmlString InputControlGroup(this HtmlHelper helper, Models.IClientControl widget, string id, string textKey, string defaultText, string dataColumn = null, string inputCss = null, string inputType = null, bool readOnly = false, bool required = false, string dataType = null, string valueMatchControl = null)
        {
            if (!string.IsNullOrEmpty(valueMatchControl))
                valueMatchControl = widget.GetId(valueMatchControl);    //todo: right place for this?
            return InputControlGroup(helper, widget, id, textKey, defaultText, GetDataAttributeDict(dataColumn, dataType: dataType, valueMatchControl: valueMatchControl), required, inputCss, inputType, readOnly);
        }
        public static MvcHtmlString InputControlGroup(this HtmlHelper helper, Models.IClientControl widget, string id, string textKey, string defaultText, Dictionary<string, string> dataAttributes, bool required, string inputCss = null, string inputType = null, bool readOnly = false)
        {
            return GetControlGroup(widget, id, textKey, defaultText, 
                                string.Format("<input type=\"{3}\" class=\"{2}\" id=\"{0}\" name=\"{0}\" {1} {4} {5} />",
                                    widget.GetId(id), GetDataAttributeMarkup(dataAttributes), inputCss, inputType, readOnly ? "readonly=\"readonly\"" : "", required ? "required=\"required\"" : ""));
        }

        public static MvcHtmlString InputFileBrowserControlGroup(this HtmlHelper helper, Models.IClientControl widget, string id, string textKey, string defaultText, string dataColumn, string inputCss = null, string mimeType = "", bool required = false)
        {
            return InputFileBrowserControlGroup(helper, widget, id, textKey, defaultText, GetDataAttributeDict(dataColumn), inputCss, mimeType);
        }       
        public static MvcHtmlString InputFileBrowserControlGroup(this HtmlHelper helper, Models.IClientControl widget, string id, string textKey, string defaultText, Dictionary<string, string> dataAttributes, string inputCss = null, string mimeType = "", bool required = false)
        {
            //ONLY REGISTER ONCE AND AT END?!?!
            helper.RenderWidget("Core/Admin/FileBrowser", new Dictionary<string, object>() { { "MimeType", mimeType } }, true);

            return GetControlGroup(widget, id, textKey, defaultText,  
                                    string.Format("   <input type=\"text\" class=\"{2}\" id=\"{0}\" {1} {3}/>" +
                                    "   <a class=\"btn\" data-action=\"filebrowser\" data-control=\"{0}\" ><i class=\"icon-picture\"></i></a>",
                                widget.GetId(id), GetDataAttributeMarkup(dataAttributes), inputCss, required ? "required=\"required\"" : ""), "input-append");
        }

        public static MvcHtmlString UploadButtonControlGroup(this HtmlHelper helper, Models.IClientControl widget, string id, string textKey, string defaultText, string buttonTextKey, string defaultButtonText, string inputCss = null, string inputType = null)
        {
            helper.RegisterScript("~/scripts/fileuploader.js", true);
            return GetControlGroup(widget, id, textKey, defaultText, 
                                    string.Format("   <a class=\"btn {1}\" id=\"{0}\" >{2}</a>",
                                widget.GetId(id), inputCss, HttpUtility.HtmlEncode(widget.GetText(buttonTextKey, defaultButtonText))));
        }

        public static MvcHtmlString TextAreaControlGroup(this HtmlHelper helper, Models.IClientControl widget, string id, string textKey, string defaultText, string dataColumn, string controlType = "", string inputCss = "", int rows = 3, bool required = false)
        {
            return TextAreaControlGroup(helper, widget, id, textKey, defaultText, GetDataAttributeDict(dataColumn, controlType), required, inputCss, rows);
        }
        public static MvcHtmlString TextAreaControlGroup(this HtmlHelper helper, Models.IClientControl widget, string id, string textKey, string defaultText, Dictionary<string, string> dataAttributes, bool required, string inputCss = "", int rows = 3)
        {
            return GetControlGroup(widget, id, textKey, defaultText, 
                                    string.Format("<textarea type=\"text\" class=\"{2}\" id=\"{0}\" {1} rows=\"{3}\" {4}></textarea>",
                                widget.GetId(id), GetDataAttributeMarkup(dataAttributes), inputCss, rows, required ? "required=\"required\"" : ""));
        }

        public static MvcHtmlString TextEditorControl(this HtmlHelper helper, Models.IClientControl clientControl, string id, string dataColumn, bool required = false, string labelText = null)
        {
            var model = new Models.TextEditor(dataColumn, Services.Portal.CurrentPortal.GetAttribute("Core", "TextEditor", "Core/CKTextEditor"));
            model.ClientId = clientControl.GetId(id);
            model.Required = required;
            model.LabelText = labelText;
            helper.RenderPartial("Controls/" + model.Path, model);
            return null;
        }

        public static MvcHtmlString RoleControlGroup(this HtmlHelper helper, Models.IClientControl widget, string id, string textKey, string defaultText, string dataColumn, List<string> selectedRoles = null, string inputCss = null, bool required = false)
        {
            return RoleControlGroup(helper, widget, id, textKey, defaultText, GetDataAttributeDict(dataColumn, "multiselect"), required, selectedRoles, inputCss);
        }
        public static MvcHtmlString RoleControlGroup(this HtmlHelper helper, Models.IClientControl widget, string id, string textKey, string defaultText, Dictionary<string, string> dataAttributes, bool required, List<string> selectedRoles = null, string inputCss = null)
        {
            var clientId = widget.GetId(id);
            helper.RegisterScript("~/scripts/multiselect/jquery.multiselect.min.js", true);
            helper.RegisterStylesheet("~/scripts/multiselect/jquery.multiselect.css", true);
            //helper.RegisterDocumentReadyScript(clientId + "multiselect", string.Format("$('#{0}').multiselect({{ selectedList: 3 }});", clientId));
            //todo: localize text (Select Options)
            helper.RegisterDocumentReadyScript("multiselect-init", string.Format("$('select[data-controltype=\"multiselect\"]').multiselect({{ selectedList: 3 }});", clientId));

            selectedRoles = selectedRoles == null ? new List<string>() : selectedRoles;

            var roles = Services.Account.GetRoles();
            var options = new System.Text.StringBuilder();
            foreach (var role in roles)
                options.AppendLine(string.Format("<option {0} value=\"{1}\">{2}</option>", selectedRoles.Contains(role.Name) ? "selected=\"selected\" " : "", role.Id, HttpUtility.HtmlEncode(role.Name)));

            return GetControlGroup(widget, id, textKey, defaultText, 
                                    string.Format("   <select class=\"{2}\" id=\"{0}\" {1}  multiple=\"multiple\" {4}>{3}</select>",
                                clientId, GetDataAttributeMarkup(dataAttributes), inputCss, options.ToString(), required ? "required=\"required\"" : ""));
        }

        public static MvcHtmlString DropDownControlGroup(this HtmlHelper helper, Models.IClientControl widget, string id, string textKey, string defaultText, string dataColumn, IEnumerable<SelectListItem> selectList, string inputCss = null, SelectListItem blankItem = null)
        {
            var list = selectList.ToList(); //todo: minor little hack...
            if (blankItem != null)
                list.Insert(0, blankItem);
            return GetControlGroup(widget, id, textKey, defaultText, helper.DropDownList(id, list, new {@class = inputCss, data_column = dataColumn}).ToString());
        }

        // Properties
        public static string RootPath
        {
            get
            {
                return VirtualPathUtility.ToAbsolute("~/");
            }
        }

        // private methods
        private static List<string> GetMarkupList(HtmlHelper helper, string type)
        {
            var dict = GetContextItem<ConcurrentDictionary<string, List<string>>>(helper, "MarkupList");
            //var dict = helper.ViewContext.HttpContext.Items.GetSetting<ConcurrentDictionary<string, List<string>>>("MarkupList", null);
            //if (dict == null)
            //{
            //    dict = new ConcurrentDictionary<string, List<string>>();
            //    helper.ViewContext.HttpContext.Items["MarkupList"] = dict;
            //}
            if (!dict.ContainsKey(type))
                dict[type] = new List<string>();

            return dict[type];
        }

        private static List<string> GetUnrenderedMarkupList(HtmlHelper helper, string type)
        {
            var list = GetMarkupList(helper, type);
            var alreadyRendered = GetRenderedMarkupList(helper, type);
            return list.Where(l => alreadyRendered.ContainsKey(l) == false).ToList();
        }

        private static void SetMarkupListRendered(HtmlHelper helper, string type)
        {
            var list = GetMarkupList(helper, type);
            var alreadyRendered = GetRenderedMarkupList(helper, type);
            foreach (var script in list)
                alreadyRendered[script] = true;
        }

        private static ConcurrentDictionary<string, bool> GetRenderedMarkupList(HtmlHelper helper, string type)
        {
            return GetContextItem<ConcurrentDictionary<string, bool>>(helper, "MarkupListRendered_" + type);
        }

        private static string GetPath(string src)
        {
            return src.Replace("~/", RootPath);
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

        private static ConcurrentDictionary<string, bool> GetRegisteredKeyDict(HtmlHelper helper)
        {
            return GetContextItem<ConcurrentDictionary<string, bool>>(helper, "KeyRegisteredDict");
        }

        private static T GetContextItem<T>(this HtmlHelper helper, string key) where T : class, new()
        {
            T o = helper.ViewContext.HttpContext.Items.GetSetting<T>(key, null);
            if (o == null)
            {
                o = new T();
                helper.ViewContext.HttpContext.Items[key] = o;
            }
            return o;
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
        private static string GetDataAttributeMarkup(Dictionary<string, string> dataAttributes)
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