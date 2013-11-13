using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;

namespace Videre.Core.Extensions.Bootstrap.Controls
{
    public class BootstrapTextAreaModel : BootstrapBaseInputControlModel
    {
        public BootstrapTextAreaModel() : base()
        {
        }

        //public string text {get;set;}
        public int? rows { get; set; }
    }

    public interface IBootstrapTextArea : IFluentBootstrapInputControl<IBootstrapTextArea, BootstrapTextAreaModel>
    {
        IBootstrapTextArea Rows(int rows);

    }

    public class BootstrapTextArea : BootstrapBaseInputControl<IBootstrapTextArea, BootstrapTextAreaModel>, IBootstrapTextArea
    {
        public BootstrapTextArea(HtmlHelper html) : base(html) { }
        public BootstrapTextArea(HtmlHelper html, string id) : base(html, id)
        {

        }

        public IBootstrapTextArea Rows(int rows)
        {
            this._model.rows = rows;
            return this;
        }

        public override string ToHtmlString()
        {
            var ctl = new TagBuilder("textarea");
            base.AddBaseMarkup(ctl);

            if (!string.IsNullOrEmpty(_model.val)) 
                ctl.SetInnerText(_model.val);
            if (_model.rows.HasValue)
                ctl.Attributes.AddSafe("rows", _model.rows.Value.ToString());

            return base.Render(ctl);
        }
    }

}
