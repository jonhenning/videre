using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;

namespace Videre.Core.Extensions.Bootstrap.Controls
{
    public class BootstrapEmailModel : BootstrapInputControlModel
    {
        public BootstrapEmailModel() : base()
        {
        }

        //public string text {get;set;}
    }

    public interface IBootstrapEmail : IFluentBootstrapInputControl<IBootstrapEmail, BootstrapEmailModel>
    {
        //IBootstrapEmail Text(string text);
    }

    public class BootstrapEmail : BootstrapInputControl<IBootstrapEmail, BootstrapEmailModel>, IBootstrapEmail
    {
        public BootstrapEmail(HtmlHelper html) : base(html) { }
        public BootstrapEmail(HtmlHelper html, string id) : base(html, id)
        {
            _model.dataType = "email";
        }

        public override string ToHtmlString()
        {
            var ctl = new TagBuilder("input");
            base.AddBaseMarkup(ctl);

            ctl.Attributes.Add("type", "email");
            
            if (!string.IsNullOrEmpty(_model.val)) //cannot be in base as textarea is different!
                ctl.Attributes.Add("val", _model.val);

            return base.Render(ctl);
        }

    }

}
