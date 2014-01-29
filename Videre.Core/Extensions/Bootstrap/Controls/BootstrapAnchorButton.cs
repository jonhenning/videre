using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;

namespace Videre.Core.Extensions.Bootstrap.Controls
{
    public class BootstrapAnchorButtonModel : BootstrapButtonModel
    {
        public string href { get; set; }
    }

    public interface IBootstrapAnchorButton : IFluentBootstrapControlBase<IBootstrapAnchorButton, BootstrapAnchorButtonModel>
    {
        IBootstrapAnchorButton Href(string icon);
        IBootstrapAnchorButton Icon(string icon);
        IBootstrapAnchorButton Text(string text);
        IBootstrapAnchorButton Text(string token, string defaultText);
        IBootstrapAnchorButton Text(string token, string defaultText, bool portalText);
        IBootstrapAnchorButton ButtonSize(Bootstrap.BootstrapUnits.ButtonSize size);
        IBootstrapAnchorButton ButtonStyle(Bootstrap.BootstrapUnits.ButtonStyle style);
    }

    public class BootstrapAnchorButton : BootstrapControlBase<IBootstrapAnchorButton, BootstrapAnchorButtonModel>, IBootstrapAnchorButton
    {
        public BootstrapAnchorButton(HtmlHelper html, string id) : base(html, id)
        {
        }

        public IBootstrapAnchorButton Href(string href)
        {
            this._model.href = href;
            return this;
        }

        public IBootstrapAnchorButton Icon(string icon)
        {
            this._model.icon = icon;
            return this;
        }

        public IBootstrapAnchorButton ButtonSize(Bootstrap.BootstrapUnits.ButtonSize size)
        {
            this._model.size = size;
            return this;
        }

        public IBootstrapAnchorButton ButtonStyle(Bootstrap.BootstrapUnits.ButtonStyle style)
        {
            this._model.style = style;
            return this;
        }

        public IBootstrapAnchorButton Text(string text)
        {
            this._model.text = text;
            return this;
        }
        public IBootstrapAnchorButton Text(string token, string defaultText)
        {
            return Text(token, defaultText, false);
        }
        public IBootstrapAnchorButton Text(string token, string defaultText, bool portalText)
        {
            this._model.text = portalText ?  GetPortalText(token, defaultText) : GetText(token, defaultText);
            return this;
        }

        public override string ToHtmlString()
        {
            var ctl = new TagBuilder("a");
            AddBaseMarkup(ctl);
            ctl.AddCssClass("btn");
            ctl.AddCssClass(Bootstrap.BootstrapUnits.GetButtonStyleCss(_model.style));

            if (!string.IsNullOrEmpty(_model.href))
                ctl.Attributes.Add("href", _model.href);

            if (!string.IsNullOrEmpty(_model.icon))
            {
                var icon = new TagBuilder("span");
                //icon.AddCssClass("glyphicon");  //todo:  do this automatically?
                icon.AddCssClass(_model.icon);
                ctl.InnerHtml = icon.ToString(TagRenderMode.Normal) + " " + _model.text;
            }
            else
                ctl.SetInnerText(_model.text);

            if (_model.size.HasValue)
                ctl.AddCssClass(Bootstrap.BootstrapUnits.GetButtonSizeCss(_model.size.Value));

            return ctl.ToString(TagRenderMode.Normal);
        }
    }

}
