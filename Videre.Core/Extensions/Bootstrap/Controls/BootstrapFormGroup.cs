using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;

namespace Videre.Core.Extensions.Bootstrap.Controls
{
    public class BootstrapFormGroupModel : BootstrapBaseControlModel
    {
        public BootstrapFormGroupModel() : base()
        {
            controls = new List<IBootstrapBaseControl>();
            controlGroupCssClasses = new List<string>();
        }

        public string text {get;set;}
        public List<string> controlGroupCssClasses {get;set;}
        public IBootstrapLabel label { get; set; }
        public List<IBootstrapBaseControl> controls { get; set; }
    }

    public interface IBootstrapFormGroup : IFluentBootstrapControlBase<IBootstrapFormGroup, BootstrapFormGroupModel>
    {
        IBootstrapLabel Label();
        List<IBootstrapBaseControl> Controls();
        IBootstrapFormGroup ControlsCss(string css);
    }

    public class BootstrapFormGroup : BootstrapControlBase<IBootstrapFormGroup, BootstrapFormGroupModel>, IBootstrapFormGroup
    {
        public BootstrapFormGroup(HtmlHelper html, IBootstrapLabel label, IBootstrapBaseControl control) : this(html, label, new List<IBootstrapBaseControl>() { control }, null)
        { }

        public BootstrapFormGroup(HtmlHelper html, IBootstrapLabel label, IBootstrapBaseControl control, BootstrapUnits.GridSize? controlSize) : this(html, label, new List<IBootstrapBaseControl>() { control }, controlSize)
        { }

        public BootstrapFormGroup(HtmlHelper html, IBootstrapLabel label, List<IBootstrapBaseControl> controls) : this(html, label, controls, null)
        { }

        public BootstrapFormGroup(HtmlHelper html, IBootstrapLabel label, List<IBootstrapBaseControl> controls, BootstrapUnits.GridSize? controlSize) : base(html)
        {
            this._model.label = label.Css("control-label");
            if (controlSize.HasValue)
                this._model.controlGroupCssClasses.Add(BootstrapUnits.GetGridSizeCss(controlSize));

            controls.ForEach(c =>
                {
                    if (string.IsNullOrEmpty(this._model.label.Model.forId))    //assign forId if not already assigned
                        this._model.label.Model.forId = c.Id;

                    if (c is IInputControl)
                        c.AddCss("form-control"); 
                });
            this._model.controls.AddRange(controls);
        }

        public IBootstrapFormGroup ControlsCss(string css)
        {
            this._model.controlGroupCssClasses.Add(css);
            return this;
        }

        public IBootstrapLabel Label()
        {
            return this._model.label;
        }
        public List<IBootstrapBaseControl> Controls()
        {
            return this._model.controls;
        }

        public override string ToHtmlString()
        {
            var group = new TagBuilder("div");
            AddBaseMarkup(group);
            group.AddCssClass("form-group");
            var controlsCtr = new TagBuilder("div");


            //controlsCtr.AddCssClass("controls");
            foreach (var css in _model.controlGroupCssClasses)
                controlsCtr.AddCssClass(css);

            foreach (var ctl in this._model.controls)
                controlsCtr.InnerHtml += ctl.ToHtmlString();
            group.InnerHtml = this._model.label.ToHtmlString() + controlsCtr.ToString();

            return group.ToString();
        }

    }

}
