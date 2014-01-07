using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;

namespace Videre.Core.Extensions.Bootstrap.Controls
{
    public class BootstrapLabelModel : BootstrapBaseControlModel
    {
        public string forId {get;set;}
        public string text { get; set; }
    }

    public interface IBootstrapLabel : IFluentBootstrapControlBase<IBootstrapLabel, BootstrapLabelModel>
    {
        IBootstrapLabel Text(string text);
        IBootstrapLabel Text(string token, string defaultText);
        IBootstrapLabel Text(string token, string defaultText, bool portalText);
    }

    public class BootstrapLabel : BootstrapControlBase<IBootstrapLabel, BootstrapLabelModel>, IBootstrapLabel
    {
        public BootstrapLabel(HtmlHelper html, string token, string defaultText) : this(html, token, defaultText, null, null)
        { }
        public BootstrapLabel(HtmlHelper html, string token, string defaultText, string forId) : this(html, token, defaultText, forId, null)
        { }

        public BootstrapLabel(HtmlHelper html, string token, string defaulText, string forId, string id) : base(html, id)
        {
            this._model.forId = GetId(forId);
            if (!string.IsNullOrEmpty(token))
                this.Text(token, defaulText);
        }

        public IBootstrapLabel Text(string text)
        {
            this._model.text = text;
            return this;
        }
        public IBootstrapLabel Text(string token, string defaultText)
        {
            return Text(token, defaultText, false);
        }
        public IBootstrapLabel Text(string token, string defaultText, bool portalText)
        {
            this._model.text = portalText ? GetPortalText(token, defaultText) : GetText(token, defaultText);
            return this;
        }

        public override string ToHtmlString()
        {
            var ctl = new TagBuilder("label");
            AddBaseMarkup(ctl);
            if (!string.IsNullOrEmpty(_model.forId))
                ctl.Attributes.AddSafe("for", _model.forId);
            
            ctl.SetInnerText(_model.text);
            return ctl.ToString(TagRenderMode.Normal);
        }
    }

}
