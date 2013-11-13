using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;

namespace Videre.Core.Extensions.Bootstrap.Controls
{
    public class BootstrapTextBoxModel : BootstrapBaseInputControlModel
    {
        public enum Plugin
        {
            [Description("jqueryui-datepicker")]
            JqueryUIDatePicker,
            [Description("jqueryui-datetimepicker")]
            JqueryUIDateTimePicker,
            [Description("jqueryui-timepicker")]
            JqueryUITimePicker
        }

        public BootstrapTextBoxModel() : base()
        {
        }

        public string plugin { get; set; }
    }

    public interface IBootstrapTextBox : IFluentBootstrapInputControl<IBootstrapTextBox, BootstrapTextBoxModel>
    {
        IBootstrapTextBox Plugin(BootstrapTextBoxModel.Plugin plugin);
        IBootstrapTextBox Plugin(string plugin);
    }

    public class BootstrapTextBox : BootstrapBaseInputControl<IBootstrapTextBox, BootstrapTextBoxModel>, IBootstrapTextBox
    {
        public BootstrapTextBox(HtmlHelper html) : base(html) { }
        public BootstrapTextBox(HtmlHelper html, string id) : base(html, id)
        {

        }

        public IBootstrapTextBox Plugin(BootstrapTextBoxModel.Plugin plugin)
        {
            return Plugin(plugin.GetDescription());
        }
        public IBootstrapTextBox Plugin(string plugin)
        {
            this._model.plugin = plugin;
            return this;
        }
        
        public override string ToHtmlString()
        {
            var ctl = new TagBuilder("input"); ;
            base.AddBaseMarkup(ctl);

            ctl.Attributes.Add("type", "text");
            if (!string.IsNullOrEmpty(_model.val)) 
                ctl.Attributes.Add("val", _model.val);  //encode?

            if (!string.IsNullOrEmpty(_model.plugin))
            {
                _html.RegisterWebReferenceGroup(_model.plugin); //todo:  use plugin name as web reference group?
                ctl.Attributes.Add("data-controltype", _model.plugin);
            }

            return base.Render(ctl);
        }

    }

}
