using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;

namespace Videre.Core.Extensions.Bootstrap.Controls
{
    public class BootstrapPasswordModel : IBootstrapInputControlModel
    {
        public BootstrapPasswordModel() : base()
        {
        }

        //public string text {get;set;}
    }

    public interface IBootstrapPassword : IFluentBootstrapInputControl<IBootstrapPassword, BootstrapPasswordModel>
    {
        //IBootstrapPassword Text(string text);
    }

    public class BootstrapPassword : BootstrapInputControl<IBootstrapPassword, BootstrapPasswordModel>, IBootstrapPassword
    {
        public BootstrapPassword(HtmlHelper html) : base(html) { }
        public BootstrapPassword(HtmlHelper html, string id) : base(html, id)
        {

        }

        public override string ToHtmlString()
        {
            var ctl = new TagBuilder("input");
            base.AddBaseMarkup(ctl);

            ctl.Attributes.Add("type", "password");
            
            if (!string.IsNullOrEmpty(_model.val)) //cannot be in base as textarea is different!
                ctl.Attributes.Add("val", _model.val);

            return base.Render(ctl);
        }

    }

}
