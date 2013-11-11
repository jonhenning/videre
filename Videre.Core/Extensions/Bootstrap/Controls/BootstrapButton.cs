using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;

namespace Videre.Core.Extensions.Bootstrap.Controls
{
    public class BootstrapButtonModel : IBootstrapBaseControlModel
    {
        public string text { get; set; }
        public Bootstrap.BootstrapUnits.ButtonSize? size { get; set; }
        public Bootstrap.BootstrapUnits.ButtonStyle style { get; set; }
        public string icon {get; set;}
    }

    public interface IBootstrapButton : IFluentBootstrapControlBase<IBootstrapButton, BootstrapButtonModel>
    {
        IBootstrapButton Icon(string icon);
        IBootstrapButton Text(string text);
        IBootstrapButton Text(string token, string defaultText);
        IBootstrapButton Text(string token, string defaultText, bool portalText);
        IBootstrapButton ButtonSize(Bootstrap.BootstrapUnits.ButtonSize size);
        IBootstrapButton ButtonStyle(Bootstrap.BootstrapUnits.ButtonStyle style);
    }

    public class BootstrapButton : BootstrapControlBase<IBootstrapButton, BootstrapButtonModel>, IBootstrapButton
    {
        public BootstrapButton(HtmlHelper html, string id) : base(html, id)
        {
        }

        public IBootstrapButton Icon(string icon)
        {
            this._model.icon = icon;
            return this;
        }

        public IBootstrapButton ButtonSize(Bootstrap.BootstrapUnits.ButtonSize size)
        {
            this._model.size = size;
            return this;
        }

        public IBootstrapButton ButtonStyle(Bootstrap.BootstrapUnits.ButtonStyle style)
        {
            this._model.style = style;
            return this;
        }

        public IBootstrapButton Text(string text)
        {
            this._model.text = text;
            return this;
        }
        public IBootstrapButton Text(string token, string defaultText)
        {
            return Text(token, defaultText, false);
        }
        public IBootstrapButton Text(string token, string defaultText, bool portalText)
        {
            this._model.text = portalText ?  GetPortalText(token, defaultText) : GetText(token, defaultText);
            return this;
        }

        public override string ToHtmlString()
        {
            var ctl = new TagBuilder("button");
            ctl.Attributes.Add("type", "button");
            AddBaseMarkup(ctl);
            ctl.AddCssClass("btn");
            ctl.AddCssClass(Bootstrap.BootstrapUnits.GetButtonStyleCss(_model.style));

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
