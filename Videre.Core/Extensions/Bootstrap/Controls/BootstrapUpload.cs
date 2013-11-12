using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;

namespace Videre.Core.Extensions.Bootstrap.Controls
{
    public class BootstrapUploadModel : BootstrapBaseControlModel
    {
        public string text { get; set; }
        public Bootstrap.BootstrapUnits.ButtonSize? size { get; set; }
        public Bootstrap.BootstrapUnits.ButtonStyle style { get; set; }
        public string icon { get; set; }
    }

    public interface IBootstrapUpload : IFluentBootstrapControlBase<IBootstrapUpload, BootstrapUploadModel>
    {
        IBootstrapUpload Icon(string icon);
        IBootstrapUpload Text(string text);
        IBootstrapUpload Text(string token, string defaultText);
        IBootstrapUpload Text(string token, string defaultText, bool portalText);
        IBootstrapUpload ButtonSize(Bootstrap.BootstrapUnits.ButtonSize size);
        IBootstrapUpload ButtonStyle(Bootstrap.BootstrapUnits.ButtonStyle style);
    }

    public class BootstrapUpload : BootstrapControlBase<IBootstrapUpload, BootstrapUploadModel>, IBootstrapUpload
    {
        public BootstrapUpload(HtmlHelper html) : base(html) { }
        public BootstrapUpload(HtmlHelper html, string id) : base(html, id)
        {
        }

        public IBootstrapUpload Icon(string icon)
        {
            this._model.icon = icon;
            return this;
        }

        public IBootstrapUpload ButtonSize(Bootstrap.BootstrapUnits.ButtonSize size)
        {
            this._model.size = size;
            return this;
        }

        public IBootstrapUpload ButtonStyle(Bootstrap.BootstrapUnits.ButtonStyle style)
        {
            this._model.style = style;
            return this;
        }

        public IBootstrapUpload Text(string text)
        {
            this._model.text = text;
            return this;
        }
        public IBootstrapUpload Text(string token, string defaultText)
        {
            return Text(token, defaultText, false);
        }
        public IBootstrapUpload Text(string token, string defaultText, bool portalText)
        {
            this._model.text = portalText ? GetPortalText(token, defaultText) : GetText(token, defaultText);
            return this;
        }

        public override string ToHtmlString()
        {
            _html.RegisterWebReferenceGroup("fileuploader");

            var ctl = new TagBuilder("a");
            base.AddBaseMarkup(ctl);

            //ctl.Attributes.Add("type", "button");
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
