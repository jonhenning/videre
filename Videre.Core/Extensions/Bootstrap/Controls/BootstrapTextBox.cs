using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;

namespace Videre.Core.Extensions.Bootstrap.Controls
{
    public class BootstrapTextBoxModel : BootstrapInputControlModel
    {
        public BootstrapTextBoxModel() : base()
        {
        }

        //public string text {get;set;}
        public bool multiLine { get; set; }
        public int? rows { get; set; }
    }

    public interface IBootstrapTextBox : IFluentBootstrapInputControl<IBootstrapTextBox, BootstrapTextBoxModel>
    {
        IBootstrapTextBox MultiLine();
        IBootstrapTextBox MultiLine(bool multiLine);
        IBootstrapTextBox Rows(int rows);

    }

    public class BootstrapTextBox : BootstrapInputControl<IBootstrapTextBox, BootstrapTextBoxModel>, IBootstrapTextBox
    {
        public BootstrapTextBox(HtmlHelper html) : base(html) { }
        public BootstrapTextBox(HtmlHelper html, string id) : base(html, id)
        {

        }

        public IBootstrapTextBox MultiLine()
        {
            return MultiLine(true);
        }

        public IBootstrapTextBox MultiLine(bool multiLine)
        {
            this._model.multiLine = multiLine;
            return this;
        }

        public IBootstrapTextBox Rows(int rows)
        {
            this._model.rows = rows;
            return this;
        }

        public override string ToHtmlString()
        {
            TagBuilder ctl = null;

            if (!_model.multiLine)
            {
                ctl = new TagBuilder("input");
                ctl.Attributes.Add("type", "text");
                if (!string.IsNullOrEmpty(_model.val)) //cannot be in base as textarea is different!
                    ctl.Attributes.Add("val", _model.val);  //encode?
            }
            else
            {
                ctl = new TagBuilder("textarea");
                if (!string.IsNullOrEmpty(_model.val)) //cannot be in base as textarea is different!
                    ctl.SetInnerText(_model.val);
                if (_model.rows.HasValue)
                    ctl.Attributes.Add("rows", _model.rows.Value.ToString());
            }

            base.AddBaseMarkup(ctl);
            return base.Render(ctl);
        }

    }

}
