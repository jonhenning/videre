using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;

namespace Videre.Core.Extensions.Bootstrap.Controls
{
    public class BootstrapSpanModel : BootstrapBaseControlModel
    {
        public string text { get; set; }
    }

    public interface IBootstrapSpan : IFluentBootstrapControlBase<IBootstrapSpan, BootstrapSpanModel>
    {
        IBootstrapSpan Text(string text);
        IBootstrapSpan Text(string token, string defaultText);
    }

    public class BootstrapSpan : BootstrapControlBase<IBootstrapSpan, BootstrapSpanModel>, IBootstrapSpan
    {
        public BootstrapSpan(HtmlHelper html, string id = null) : base(html, id)
        {
        }

        public IBootstrapSpan Text(string text)
        {
            this._model.text = text;
            return this;
        }
        public IBootstrapSpan Text(string token, string defaultText)
        {
            this._model.text = GetText(token, defaultText);
            return this;
        }

        public override string ToHtmlString()
        {
            var ctl = new TagBuilder("span");
            AddBaseMarkup(ctl);           
            ctl.SetInnerText(_model.text);
            return ctl.ToString(TagRenderMode.Normal);
        }
    }

}
