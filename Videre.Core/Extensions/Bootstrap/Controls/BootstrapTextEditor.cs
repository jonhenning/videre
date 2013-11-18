using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;
using Videre.Core.Extensions;

namespace Videre.Core.Extensions.Bootstrap.Controls
{
    public class BootstrapTextEditorModel : BootstrapBaseInputControlModel
    {
        public enum Plugin
        {
            [Description("cktexteditor")]
            CKTextEditor,
            [Description("cltexteditor")]
            CLTextEditor,
            [Description("wysihtml5texteditor")]
            WYSIHTML5TextEditor
        }

        public BootstrapTextEditorModel() : base()
        {
        }
        public int? rows { get; set; }
        public string plugin { get; set; }
    }

    public interface IBootstrapTextEditor : IFluentBootstrapInputControl<IBootstrapTextEditor, BootstrapTextEditorModel>
    {
        IBootstrapTextEditor Plugin(BootstrapTextEditorModel.Plugin plugin);
        IBootstrapTextEditor Plugin(string plugin);
        IBootstrapTextEditor Rows(int rows);
    }

    public class BootstrapTextEditor : BootstrapBaseInputControl<IBootstrapTextEditor, BootstrapTextEditorModel>, IBootstrapTextEditor
    {
        public BootstrapTextEditor(HtmlHelper html) : base(html) { }
        public BootstrapTextEditor(HtmlHelper html, string id) : base(html, id)
        {

        }

        public IBootstrapTextEditor Rows(int rows)
        {
            this._model.rows = rows;
            return this;
        }

        public IBootstrapTextEditor Plugin(BootstrapTextEditorModel.Plugin plugin)
        {
            return Plugin(plugin.GetDescription());
        }
        public IBootstrapTextEditor Plugin(string plugin)
        {
            this._model.plugin = plugin;
            return this;
        }
        
        public override string ToHtmlString()
        {
            var ctl = new TagBuilder("textarea");
            base.AddBaseMarkup(ctl);

            if (_model.rows.HasValue)
                ctl.Attributes.AddSafe("rows", _model.rows.Value.ToString());

            if (!string.IsNullOrEmpty(_model.plugin))
            {
                _html.RegisterWebReferenceGroup(_model.plugin); //todo:  use plugin name as web reference group?
                ctl.Attributes.AddSafe("data-controltype", _model.plugin);
            }

            if (!string.IsNullOrEmpty(_model.val))
                ctl.SetInnerText(_model.val);

            return base.Render(ctl);

        }

    }

}
