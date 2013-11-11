using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;

namespace Videre.Core.Extensions.Bootstrap.Controls
{
    public class BootstrapTextBoxModel : IBootstrapInputControlModel
    {
        public BootstrapTextBoxModel() : base()
        {
        }

        //public string text {get;set;}
    }

    public interface IBootstrapTextBox : IFluentBootstrapInputControl<IBootstrapTextBox, BootstrapTextBoxModel>
    {
        //IBootstrapTextBox Text(string text);
    }

    public class BootstrapTextBox : BootstrapInputControl<IBootstrapTextBox, BootstrapTextBoxModel>, IBootstrapTextBox
    {
        public BootstrapTextBox(HtmlHelper html) : base(html) { }
        public BootstrapTextBox(HtmlHelper html, string id) : base(html, id)
        {

        }

        //public IBootstrapTextBox Text(string text)
        //{
        //    this._model.text = text;
        //    return this;
        //}

        public override string ToHtmlString()
        {
            var ctl = new TagBuilder("input");
            base.AddBaseMarkup(ctl);

            ctl.Attributes.Add("type", "text");
            
            if (!string.IsNullOrEmpty(_model.val)) //cannot be in base as textarea is different!
                ctl.Attributes.Add("val", _model.val);

            return base.Render(ctl);
            //return ctl.ToString(TagRenderMode.Normal);
        }

    }

}
