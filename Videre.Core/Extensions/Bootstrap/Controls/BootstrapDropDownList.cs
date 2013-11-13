using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;
using Videre.Core.Extensions;

namespace Videre.Core.Extensions.Bootstrap.Controls
{

    public class BootstrapDropDownListModel : BootstrapBaseInputControlModel
    {
        public enum Plugin
        {
            [Description("bootstrap-select")]
            BootstrapSelect,
            [Description("bootstrap-multiselect")]
            BootstrapMultiSelect
        }

        public BootstrapDropDownListModel() : base()
        {
            options = new List<SelectListItem>();
        }

        public List<SelectListItem> options { get; set; }
        public string plugin { get; set; }
        public bool multiple { get; set; }
    }

    public interface IBootstrapDropDownList : IFluentBootstrapInputControl<IBootstrapDropDownList, BootstrapDropDownListModel>
    {
        IBootstrapDropDownList Options(List<SelectListItem> options);
        IBootstrapDropDownList Plugin(BootstrapDropDownListModel.Plugin plugin);
        IBootstrapDropDownList Plugin(string plugin);
        IBootstrapDropDownList Multiple();
        IBootstrapDropDownList Multiple(bool multiple);
    }

    public class BootstrapDropDownList : BootstrapBaseInputControl<IBootstrapDropDownList, BootstrapDropDownListModel>, IBootstrapDropDownList
    {
        public BootstrapDropDownList(HtmlHelper html) : base(html) { }

        public BootstrapDropDownList(HtmlHelper html, string id) : base(html, id)
        {

        }

        public IBootstrapDropDownList Plugin(BootstrapDropDownListModel.Plugin plugin)
        {
            return Plugin(plugin.GetDescription());
        }
        public IBootstrapDropDownList Plugin(string plugin)
        {
            this._model.plugin = plugin;
            return this;
        }

        public IBootstrapDropDownList Options(List<SelectListItem> options)
        {
            this._model.options.AddRange(options);
            return this;
        }

        public IBootstrapDropDownList Multiple() { return Multiple(true); }
        public IBootstrapDropDownList Multiple(bool multiple)
        {
            this._model.multiple = multiple;
            return this;
        }

        public override string ToHtmlString()
        {
            var ctl = new TagBuilder("select");
            base.AddBaseMarkup(ctl);

            if (!string.IsNullOrEmpty(_model.val))
            {
                if (_model.multiple == false)
                    _model.options.Where(o => o.Selected).ToList().ForEach(o => o.Selected = false);
                _model.options.Where(o => o.Value == _model.val).FirstOrDefault().Selected = true;
            }

            if (_model.multiple)
                ctl.Attributes.Add("multiple", "multiple");

            if (!string.IsNullOrEmpty(_model.plugin))
            {
                _html.RegisterWebReferenceGroup(_model.plugin); //todo:  use plugin name as web reference group?
                ctl.Attributes.Add("data-controltype", _model.plugin);

                //if (_model.plugin.Value == BootstrapDropDownListModel.Plugin.BootstrapSelect)
                //{
                //    _html.RegisterWebReferenceGroup("bootstrap-select");
                //    //_html.RegisterDocumentReadyScript("registerControlType bootstrap-select", "videre.UI.registerControlType('bootstrap-select', { get: function(ctl) { return ctl.val(); }, set: function(ctl, val) { ctl.val(val); ctl.selectpicker('refresh'); }, init: function(ctr) { ctr.find('[data-controltype=\"bootstrap-select\"']).selectpicker(); }});");
                //    ctl.Attributes.Add("data-controltype", "bootstrap-select");
                //    //ctl.Attributes.Add("data-plugin", "bootstrap-select");
                //}
                //else if (_model.plugin.Value == BootstrapDropDownListModel.Plugin.BootstrapMultiSelect)
                //{
                //    _html.RegisterWebReferenceGroup("bootstrap-multiselect");
                //    //_html.RegisterDocumentReadyScript("registerControlType bootstrap-multiselect", "videre.UI.registerControlType('bootstrap-multiselect', { get: function(ctl) { return ctl.val(); }, set: function(ctl, val) { ctl.val(val); ctl.multiselect('refresh'); }, init: function(ctr) { ctr.find(['data-controltype=\"bootstrap-multiselect\"']).multiselect(); }});");
                //    ctl.Attributes.Add("data-controltype", "bootstrap-multiselect");
                //    //ctl.Attributes.Add("data-plugin", "bootstrap-multiselect");
                //}
            }

            foreach (var o in this._model.options)
                ctl.InnerHtml += ToHtml(o);


            return base.Render(ctl);
        }

        private string ToHtml(SelectListItem item)
        {
            var tag = new TagBuilder("option");
            tag.SetInnerText(item.Text);
            //tag.InnerHtml = HttpUtility.HtmlEncode(item.Text);

            if (item.Value != null)
                tag.Attributes["value"] = item.Value;
            if (item.Selected)
                tag.Attributes["selected"] = "selected";
            return tag.ToString(TagRenderMode.Normal);
        }


    }

}
