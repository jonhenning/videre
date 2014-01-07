using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;

namespace Videre.Core.Extensions.Bootstrap.Controls
{
    public class BootstrapParagraphModel : BootstrapBaseControlModel
    {
        public string text { get; set; }
    }

    public interface IBootstrapParagraph : IFluentBootstrapControlBase<IBootstrapParagraph, BootstrapParagraphModel>
    {
        IBootstrapParagraph Text(string text);
        IBootstrapParagraph Text(string token, string defaultText);
    }

    public class BootstrapParagraph : BootstrapControlBase<IBootstrapParagraph, BootstrapParagraphModel>, IBootstrapParagraph
    {
        public BootstrapParagraph(HtmlHelper html, string id = null) : base(html, id)
        {
        }

        public IBootstrapParagraph Text(string text)
        {
            this._model.text = text;
            return this;
        }
        public IBootstrapParagraph Text(string token, string defaultText)
        {
            this._model.text = GetText(token, defaultText);
            return this;
        }

        public override string ToHtmlString()
        {
            var ctl = new TagBuilder("p");
            AddBaseMarkup(ctl);           
            ctl.SetInnerText(_model.text);
            return ctl.ToString(TagRenderMode.Normal);
        }
    }

}
