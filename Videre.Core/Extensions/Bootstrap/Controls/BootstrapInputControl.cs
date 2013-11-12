using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Videre.Core.Extensions.Bootstrap.Controls
{
    public abstract class BootstrapInputControl<TControl, TModel> : BootstrapControlBase<TControl, TModel>, IFluentBootstrapInputControl<TControl, TModel>
        where TModel : BootstrapInputControlModel, new()
        where TControl : class, IFluentBootstrapInputControl<TControl, TModel>
    {
        public BootstrapInputControl(HtmlHelper html) : base(html)
        {
        }
        public BootstrapInputControl(HtmlHelper html, string id) : this(html)
        {
            if (!string.IsNullOrEmpty(id))
                this._model.id = GetId(id);
        }

        public TControl Val(string val)
        {
            _model.val = val;
            return _control;
        }

        public TControl InputSize(Bootstrap.BootstrapUnits.InputSize size)
        {
            _model.inputSize = size;
            return _control;
        }

        public TControl MustMatch(string controlId)
        {
            _model.mustMatch = GetId(controlId);
            return _control;
        }

        public TControl ReadOnly()
        {
            return ReadOnly(true);
        }

        public TControl ReadOnly(bool readOnly)
        {
            _model.readOnly = readOnly;
            return _control;
        }

        public TControl DataType(string type)
        {
            _model.dataType = type;
            return _control;
        }

        public TControl Required()
        {
            return Required(true);
        }
        public TControl Required(bool required)
        {
            _model.required = required;
            return _control;
        }

        public TControl DisableAutoComplete()
        {
            _model.disableAutoComplete = true;
            return _control;
        }

        public TControl Append(Bootstrap.IBootstrapBaseControl ctl)
        {
            _model.appendControl = ctl;
            return _control;
        }

        public TControl Prepend(Bootstrap.IBootstrapBaseControl ctl)
        {
            _model.prependControl = ctl;
            return _control;
        }

        protected override void AddBaseMarkup(TagBuilder ctl)
        {
            base.AddBaseMarkup(ctl);

            //if (!string.IsNullOrEmpty(_model.text)) //cannot be in base as textarea is different!
            //    ctl.Attributes.Add("val", _model.text);

            if (!string.IsNullOrEmpty(_model.dataType))
                ctl.Attributes.Add("data-datatype", _model.dataType);
            if (!string.IsNullOrEmpty(_model.mustMatch))
                ctl.Attributes.Add("data-match", _model.mustMatch);

            if (_model.inputSize != BootstrapUnits.InputSize.Default)
                ctl.AddCssClass(Bootstrap.BootstrapUnits.GetInputSizeCss(_model.inputSize));

            if (_model.required)
                ctl.Attributes.Add("required", "required");
            if (_model.readOnly)
                ctl.Attributes.Add("readonly", "readonly");
            if (_model.disableAutoComplete)
                ctl.Attributes.Add("autocomplete", "off");
        }

        protected string Render(TagBuilder ctl)
        {
            if (_model.appendControl != null || _model.prependControl != null)
            {
                var ctr = new TagBuilder("div");
                ctr.AddCssClass("input-group");

                //todo: inputgroup size
                HandleAddOn(ctr, _model.prependControl);
                ctr.InnerHtml += ctl.ToString(TagRenderMode.Normal);
                HandleAddOn(ctr, _model.appendControl);               
                return ctr.ToString(TagRenderMode.Normal);
            }
            else 
                return ctl.ToString(TagRenderMode.Normal);
        }

        private void HandleAddOn(TagBuilder ctr, IBootstrapBaseControl ctl)
        {
            if (ctl != null)
            {
                if (ctl is IBootstrapButton)
                {
                    var inputGroup = new TagBuilder("span");
                    inputGroup.AddCssClass("input-group-btn");
                    inputGroup.InnerHtml = ctl.ToHtmlString();
                    ctr.InnerHtml += inputGroup.ToString(TagRenderMode.Normal);
                }
                else
                {
                    ctl.AddCss("input-group-addon");
                    ctr.InnerHtml += ctl.ToHtmlString();
                }
            }
        }

    }

    


}
