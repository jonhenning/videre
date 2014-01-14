using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;

namespace Videre.Core.Extensions.Bootstrap.Controls
{
    public class BootstrapRadioButtonModel : BootstrapButtonModel
    {
        public string groupName { get; set; }
    }

    public interface IBootstrapRadioButton : IFluentBootstrapControlBase<IBootstrapRadioButton, BootstrapRadioButtonModel>
    {
        IBootstrapRadioButton GroupName(string groupName);
        IBootstrapRadioButton Icon(string icon);
        IBootstrapRadioButton Text(string text);
        IBootstrapRadioButton Text(string token, string defaultText);
        IBootstrapRadioButton Text(string token, string defaultText, bool portalText);
        IBootstrapRadioButton ButtonSize(Bootstrap.BootstrapUnits.ButtonSize size);
        IBootstrapRadioButton ButtonStyle(Bootstrap.BootstrapUnits.ButtonStyle style);
    }

    public class BootstrapRadioButton : BootstrapControlBase<IBootstrapRadioButton, BootstrapRadioButtonModel>, IBootstrapRadioButton
    {
        public BootstrapRadioButton(HtmlHelper html, string id) : base(html, id)
        {
        }

        public IBootstrapRadioButton GroupName(string groupName)
        {
            this._model.groupName = groupName;
            return this;
        }

        public IBootstrapRadioButton Icon(string icon)
        {
            this._model.icon = icon;
            return this;
        }

        public IBootstrapRadioButton ButtonSize(Bootstrap.BootstrapUnits.ButtonSize size)
        {
            this._model.size = size;
            return this;
        }

        public IBootstrapRadioButton ButtonStyle(Bootstrap.BootstrapUnits.ButtonStyle style)
        {
            this._model.style = style;
            return this;
        }

        public IBootstrapRadioButton Text(string text)
        {
            this._model.text = text;
            return this;
        }
        public IBootstrapRadioButton Text(string token, string defaultText)
        {
            return Text(token, defaultText, false);
        }
        public IBootstrapRadioButton Text(string token, string defaultText, bool portalText)
        {
            this._model.text = portalText ?  GetPortalText(token, defaultText) : GetText(token, defaultText);
            return this;
        }

        public override string ToHtmlString()
        {
            var ctl = new TagBuilder("label");
            var input = new TagBuilder("input");
            input.Attributes.AddSafe("type", "radio");
            input.Attributes.AddSafe("name", _model.groupName);

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

            ctl.InnerHtml = input.ToString() + ctl.InnerHtml;

            return ctl.ToString(TagRenderMode.Normal);
        }
    }

}
