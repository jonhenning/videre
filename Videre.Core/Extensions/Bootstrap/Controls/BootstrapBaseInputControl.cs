using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Videre.Core.Extensions.Bootstrap.Controls
{
    public abstract class BootstrapBaseInputControl<TControl, TModel> : BootstrapControlBase<TControl, TModel>, IFluentBootstrapInputControl<TControl, TModel>
        where TModel : BootstrapBaseInputControlModel, new()
        where TControl : class, IFluentBootstrapInputControl<TControl, TModel>
    {
        public BootstrapBaseInputControl(HtmlHelper html)
            : base(html)
        {
        }
        public BootstrapBaseInputControl(HtmlHelper html, string id)
            : this(html)
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

        public TControl ControlType(string type)
        {
            _model.controlType = type;
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

        public TControl MaxLength(int maxLength)
        {
            _model.maxLength = maxLength;
            return _control;
        }

        public TControl DisableAutoComplete()
        {
            _model.disableAutoComplete = true;
            return _control;
        }

        public TControl Append(Bootstrap.IBootstrapBaseControl ctl)
        {
            if (_model.appendControls == null)
                _model.appendControls = new List<IBootstrapBaseControl>();
            _model.appendControls.Add(ctl);
            return _control;
        }

        public TControl Prepend(Bootstrap.IBootstrapBaseControl ctl)
        {
            if (_model.prependControls == null)
                _model.prependControls = new List<IBootstrapBaseControl>();
            _model.prependControls.Add(ctl);
            return _control;
        }

        protected override void AddBaseMarkup(TagBuilder ctl)
        {
            base.AddBaseMarkup(ctl);

            //if (!string.IsNullOrEmpty(_model.text)) //cannot be in base as textarea is different!
            //    ctl.Attributes.Add("val", _model.text);

            if (!string.IsNullOrEmpty(_model.dataType))
                ctl.Attributes.AddSafe("data-datatype", _model.dataType);
            if (!string.IsNullOrEmpty(_model.controlType))
                ctl.Attributes.AddSafe("data-controltype", _model.controlType);
            if (!string.IsNullOrEmpty(_model.mustMatch))
                ctl.Attributes.AddSafe("data-match", _model.mustMatch);

            if (_model.maxLength.HasValue)
                ctl.Attributes.AddSafe("maxlength", _model.maxLength.Value.ToString());

            if (_model.inputSize != BootstrapUnits.InputSize.Default)
                ctl.AddCssClass(Bootstrap.BootstrapUnits.GetInputSizeCss(_model.inputSize));

            if (_model.required)
                ctl.Attributes.AddSafe("required", "required");
            if (_model.readOnly)
                ctl.Attributes.AddSafe("readonly", "readonly");
            if (_model.disableAutoComplete)
                ctl.Attributes.AddSafe("autocomplete", "off");
        }

        protected string Render(TagBuilder ctl)
        {
            if (_model.appendControls != null || _model.prependControls != null)
            {
                var ctr = new TagBuilder("div");
                ctr.AddCssClass("input-group");

                //todo: inputgroup size
                HandleAddOn(ctr, _model.prependControls);
                ctr.InnerHtml += ctl.ToString(TagRenderMode.Normal);
                HandleAddOn(ctr, _model.appendControls);
                return ctr.ToString(TagRenderMode.Normal);
            }
            else
                return ctl.ToString(TagRenderMode.Normal);
        }

        private void HandleAddOn(TagBuilder ctr, List<IBootstrapBaseControl> ctls)
        {
            if (ctls != null)
            {
                foreach (var ctl in ctls)
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




}
