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
    public class BootstrapCheckBoxModel : BootstrapBaseInputControlModel
    {

        public BootstrapCheckBoxModel() : base()
        {
        }
        public bool isChecked { get; set; }
        public string text { get; set; }
    }

    public interface IBootstrapCheckBox : IFluentBootstrapInputControl<IBootstrapCheckBox, BootstrapCheckBoxModel>
    {
        IBootstrapCheckBox Checked(bool isChecked);
        IBootstrapCheckBox Text(string token, string defaultText);
    }

    public class BootstrapCheckBox : BootstrapBaseInputControl<IBootstrapCheckBox, BootstrapCheckBoxModel>, IBootstrapCheckBox
    {
        public BootstrapCheckBox(HtmlHelper html) : base(html) { }
        public BootstrapCheckBox(HtmlHelper html, string id) : base(html, id)
        {

        }

        public IBootstrapCheckBox Checked(bool isChecked)
        {
            _model.isChecked = isChecked;
            return this;
        }

        public IBootstrapCheckBox Text(string token, string defaultText)
        {
            _model.text = GetText(token, defaultText);
            return this;
        }
       
        public override string ToHtmlString()
        {
            var ctl = new TagBuilder("div");
            var lbl = new TagBuilder("label");
            var chk = new TagBuilder("input");
            ctl.AddCssClass("checkbox");

            ControlType("checkbox");
            _model.CssClasses.Remove("form-control");   //checkbox doesn't like this!

            base.AddBaseMarkup(chk);

            chk.Attributes.AddSafe("type", "checkbox");
            //chk.Attributes.AddSafe("data-controltype", "checkbox");

            if (!string.IsNullOrEmpty(_model.val))
                chk.Attributes.AddSafe("value", _model.val); 

            if (_model.isChecked)
                chk.Attributes.AddSafe("checked", "checked");

            lbl.InnerHtml = chk.ToString(TagRenderMode.Normal);
            if (!string.IsNullOrEmpty(_model.text))
                lbl.InnerHtml += " " + _model.text; //todo: encode?

            ctl.InnerHtml = lbl.ToString(TagRenderMode.Normal);
            return base.Render(ctl);
        }

    }

}
